using MassTransit;

using Microsoft.EntityFrameworkCore;

using Policy.Api.Endpoints;
using Policy.Api.Errors;
using Policy.Application.Commands;
using Policy.Application.Interfaces;
using Policy.Infrastructure.Persistence;
using Policy.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Database: this service owns the "policy" schema ---
builder.Services.AddDbContext<PolicyDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("PolicyDb"),
        npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", PolicyDbContext.Schema)));

builder.Services.AddScoped<IPolicyRepository, PolicyRepository>();
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PolicyDbContext>());

// --- 2. Application ---
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<CreatePolicyCommandHandler>();

// --- 3. Messaging: published events go to the outbox table and are sent after the transaction commits ---
var rabbitMq = builder.Configuration.GetSection("RabbitMQ");
builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<PolicyDbContext>(outbox =>
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

app.MapPolicyEndpoints();
app.MapHealthChecks("/health");

// Apply pending migrations on startup so `docker compose up` needs no manual database setup.
await using (var scope = app.Services.CreateAsyncScope())
{
    await scope.ServiceProvider.GetRequiredService<PolicyDbContext>().Database.MigrateAsync();
}

await app.RunAsync();
