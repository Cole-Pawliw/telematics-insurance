using Billing.Api.Endpoints;
using Billing.Application.EventConsumers;
using Billing.Application.Interfaces;
using Billing.Infrastructure.Persistence;
using Billing.Infrastructure.Repositories;

using MassTransit;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Database: this service owns the "billing" schema ---
builder.Services.AddDbContext<BillingDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("BillingDb"),
        npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", BillingDbContext.Schema)));

builder.Services.AddScoped<IBillingRepository, BillingRepository>();

// --- 2. Application ---
builder.Services.AddSingleton(TimeProvider.System);

// --- 3. Messaging: consume TripCompleted events from RabbitMQ ---
var rabbitMq = builder.Configuration.GetSection("RabbitMQ");
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<TripCompletedConsumer>();

    // Inbox: records each processed message so a redelivered trip is not counted twice.
    x.AddEntityFrameworkOutbox<BillingDbContext>(outbox => outbox.UsePostgres());

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitMq["Host"] ?? "localhost", "/", host =>
        {
            host.Username(rabbitMq["Username"] ?? "guest");
            host.Password(rabbitMq["Password"] ?? "guest");
        });

        cfg.ReceiveEndpoint("billing-trip-completed", endpoint =>
        {
            // Retries cover concurrent updates to the same invoice (unique index or row version conflicts).
            endpoint.UseMessageRetry(retry => retry.Intervals(100, 500, 1000, 5000));
            endpoint.UseEntityFrameworkOutbox<BillingDbContext>(context);
            endpoint.ConfigureConsumer<TripCompletedConsumer>(context);
        });
    });
});

// --- 4. HTTP ---
builder.Services.AddProblemDetails();
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

app.MapBillingEndpoints();
app.MapHealthChecks("/health");

// Apply pending migrations on startup so `docker compose up` needs no manual database setup.
await using (var scope = app.Services.CreateAsyncScope())
{
    await scope.ServiceProvider.GetRequiredService<BillingDbContext>().Database.MigrateAsync();
}

await app.RunAsync();
