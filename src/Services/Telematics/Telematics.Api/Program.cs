using MassTransit;

using Microsoft.EntityFrameworkCore;

using Telematics.Api.Endpoints;
using Telematics.Api.Errors;
using Telematics.Application.Commands;
using Telematics.Application.Interfaces;
using Telematics.Infrastructure.Persistence;
using Telematics.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Database: this service owns the "telematics" schema ---
builder.Services.AddDbContext<TelematicsDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("TelematicsDb"),
        npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", TelematicsDbContext.Schema)));

builder.Services.AddScoped<ITripRepository, TripRepository>();
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TelematicsDbContext>());

// --- 2. Application ---
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IngestTripCommandHandler>();

// --- 3. Messaging: published events go to the outbox table and are sent after the transaction commits ---
var rabbitMq = builder.Configuration.GetSection("RabbitMQ");
builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<TelematicsDbContext>(outbox =>
    {
        outbox.UsePostgres();
        outbox.UseBusOutbox();
    });

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitMq["Host"] ?? "localhost", "/", host =>
        {
            host.Username(rabbitMq["Username"] ?? "guest");
            host.Password(rabbitMq["Password"] ?? "guest");
        });
    });
});

// --- 4. HTTP ---
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapTelematicsEndpoints();
app.MapHealthChecks("/health");

// Apply pending migrations on startup so `docker compose up` needs no manual database setup.
await using (var scope = app.Services.CreateAsyncScope())
{
    await scope.ServiceProvider.GetRequiredService<TelematicsDbContext>().Database.MigrateAsync();
}

await app.RunAsync();
