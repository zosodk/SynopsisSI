using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using SynopsisSI.Services.OrderService.Infrastructure.Persistence;
using SynopsisSI.Services.OrderService.Application.Interfaces.Persistence;
using SynopsisSI.Services.OrderService.Infrastructure.Persistence.Repositories;
using SynopsisSI.Services.OrderService.Infrastructure.Persistence.Common;
using SynopsisSI.Services.OrderService.Application.Features.Orders.Commands.PlaceOrder;
using SynopsisSI.Services.OrderService.Application.Interfaces.MessageBus;
using SynopsisSI.Services.OrderService.Infrastructure.EventBus;
using Serilog;
using System;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http;
using MassTransit;

Log.Logger = new LoggerConfiguration().CreateBootstrapLogger();

try
{
    Log.Information("Starting OrderService.API host builder...");
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("ApplicationName", "OrderService.API"));

    var config = builder.Configuration;
    builder.Services.AddDbContext<OrderServiceDbContext>(opt =>
        opt.UseMongoDB(config.GetConnectionString("OrderServiceMongoDb")!, config["MongoDbDatabaseName"]!));
    
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<IOrderRepository, EfCoreOrderRepository>();
    builder.Services.AddScoped<PlaceOrderCommandHandler>();

    builder.Services.AddScoped<IMessageBusPublisher, MassTransitMessageBusPublisher>();
    builder.Services.AddMassTransit(x =>
    {
        x.UsingRabbitMq((context, cfg) =>
        {
            var rabbitMqHost = config["MessageBroker:RabbitMQ:Host"] ?? "rabbitmq";
            cfg.Host(rabbitMqHost, "/", h => {
                h.Username(config["MessageBroker:RabbitMQ:Username"] ?? "user");
                h.Password(config["MessageBroker:RabbitMQ:Password"] ?? "password");
            });
        });
    });

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "SynopsisSI - Order Service API", 
        Version = "v1" 
    }));
    
    builder.Services.AddCors(opt => 
        opt.AddPolicy("AllowAll", p => 
            p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

    var app = builder.Build();
    app.UseSerilogRequestLogging();
    
    if (app.Environment.IsDevelopment())
    { 
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderService.API v1"));
        app.UseDeveloperExceptionPage();
    }
    
    app.UseRouting();
    app.UseCors("AllowAll");
    app.MapControllers();
    app.MapGet("/health/orders", () => Results.Ok(new { 
        Status = "Healthy", 
        Service = "OrderService", 
        Timestamp = DateTime.UtcNow 
    }));
    
    Log.Information("OrderService.API host starting...");
    app.Run();
}
catch (Exception ex)
{ 
    Log.Fatal(ex, "OrderService.API host terminated unexpectedly.");
    throw;
}
finally
{ 
    Log.CloseAndFlush();
}