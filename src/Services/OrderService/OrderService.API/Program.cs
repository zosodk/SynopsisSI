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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

    Log.Logger = new LoggerConfiguration().MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
        .Enrich.FromLogContext().WriteTo.Console().CreateBootstrapLogger();
    try
    {
        Log.Information("Starting OrderService.API host builder...");
        var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration).Enrich.WithProperty("ApplicationName", "OrderService.API"));

        var config = builder.Configuration;
        builder.Services.AddDbContext<OrderServiceDbContext>(opt =>
            opt.UseMongoDB(config.GetConnectionString("OrderServiceMongoDb")!, config["MongoDbDatabaseName"]!));
        
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IOrderRepository, EfCoreOrderRepository>();
        builder.Services.AddScoped<PlaceOrderCommandHandler>();

        builder.Services.AddScoped<IMessageBusPublisher, MassTransitMessageBusPublisher>();
        builder.Services.AddMassTransit(x => {
            // Add consumers if OrderService consumes any events
            // x.AddConsumer<SomeEventConsumerForOrderService>();
            x.UsingRabbitMq((context, cfg) => {
                var rabbitMqHost = config["MessageBroker:RabbitMQ:Host"] ?? "rabbitmq";
                var rabbitMqUser = config["MessageBroker:RabbitMQ:Username"] ?? "user";
                var rabbitMqPass = config["MessageBroker:RabbitMQ:Password"] ?? "password";
                cfg.Host(rabbitMqHost, "/", h => { h.Username(rabbitMqUser); h.Password(rabbitMqPass); });
                // cfg.ConfigureEndpoints(context); // If OrderService has consumers
            });
        });

        var jwtSettings = config.GetSection("JwtSettings");
        var secretKeyString = jwtSettings["SecretKey"];
        if (!string.IsNullOrEmpty(secretKeyString) && secretKeyString.Length >= 32)
        {
            var key = Encoding.ASCII.GetBytes(secretKeyString);
            builder.Services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options => {
                options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
                options.TokenValidationParameters = new TokenValidationParameters {
                    ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true, ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true, ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true, ClockSkew = TimeSpan.Zero
                };
            });
        } else Log.Warning("JWT SecretKey for OrderService token validation is not configured or too short.");
        builder.Services.AddAuthorization();


        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c => {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "SynopsisSI - Order Service API", Version = "v1" });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
                In = ParameterLocation.Header, Description = "JWT Authorization header. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization", Type = SecuritySchemeType.Http, Scheme = "Bearer", BearerFormat = "JWT"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement {{
                new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }},
                Array.Empty<string>()
            }});
        });
        builder.Services.AddCors(opt => opt.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

        var app = builder.Build();
        app.UseSerilogRequestLogging();
        if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderService.API v1")); app.UseDeveloperExceptionPage(); }
        
        app.UseRouting();
        app.UseCors("AllowAll");
        app.UseAuthentication(); 
        app.UseAuthorization();
        app.MapControllers();
        app.MapGet("/health/orders", () => Results.Ok(new { Status = "Healthy", Service = "OrderService", Timestamp = DateTime.UtcNow }));
        Log.Information("OrderService.API host starting...");
        app.Run();
    }
    catch (Exception ex) { Log.Fatal(ex, "OrderService.API host terminated unexpectedly."); throw; }
    finally { Log.CloseAndFlush(); }