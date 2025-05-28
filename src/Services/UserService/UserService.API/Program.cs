using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using SynopsisSI.Services.UserService.Infrastructure.Persistence;
using SynopsisSI.Services.UserService.Application.Interfaces.Persistence;
using SynopsisSI.Services.UserService.Infrastructure.Persistence.Repositories;
using SynopsisSI.Services.UserService.Infrastructure.Persistence.Common;
using SynopsisSI.Services.UserService.Application.Features.Users.Commands.RegisterUser;
using SynopsisSI.Services.UserService.Application.Features.Auth.Commands.LoginUser;
using SynopsisSI.Services.UserService.Application.Interfaces.Infrastructure; 
using SynopsisSI.Services.UserService.Infrastructure.Auth; 
using SynopsisSI.Services.UserService.Infrastructure.Security; 
using SynopsisSI.Services.UserService.Application.Interfaces.MessageBus;
using SynopsisSI.Services.UserService.Infrastructure.EventBus;
using SynopsisSI.Services.UserService.Application.Features.Users.Queries.GetUserById; // Added
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
    Log.Information("Starting UserService.API host builder...");
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration).Enrich.WithProperty("ApplicationName", "UserService.API"));

    var config = builder.Configuration;
    builder.Services.AddDbContext<UserServiceDbContext>(opt =>
        opt.UseNpgsql(config.GetConnectionString("UserServicePostgresDb"), 
            npgsqlOptionsAction: sqlOptions => {
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorCodesToAdd: null);
            }));

    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<IUserRepository, EfCoreUserRepository>();
    builder.Services.AddScoped<RegisterUserCommandHandler>();
    builder.Services.AddScoped<LoginUserCommandHandler>();
    builder.Services.AddScoped<GetUserByIdQueryHandler>(); // Added registration

    builder.Services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();
    builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();

    builder.Services.AddScoped<IMessageBusPublisher, MassTransitMessageBusPublisher>();
    builder.Services.AddMassTransit(x => {
        x.UsingRabbitMq((context, cfg) => {
            var rabbitMqHost = config["MessageBroker:RabbitMQ:Host"] ?? "rabbitmq";
            var rabbitMqUser = config["MessageBroker:RabbitMQ:Username"] ?? "user";
            var rabbitMqPass = config["MessageBroker:RabbitMQ:Password"] ?? "password";
            cfg.Host(rabbitMqHost, "/", h => { h.Username(rabbitMqUser); h.Password(rabbitMqPass); });
        });
    });

    var jwtSettings = config.GetSection("JwtSettings");
    var secretKeyString = jwtSettings["SecretKey"];
    if (string.IsNullOrEmpty(secretKeyString) || secretKeyString.Length < 32) 
        throw new InvalidOperationException("JWT SecretKey not configured or is too short (must be at least 32 bytes).");
    var key = Encoding.ASCII.GetBytes(secretKeyString);

    builder.Services.AddAuthentication(options => {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options => {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true, ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true, ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true, ClockSkew = TimeSpan.Zero
        };
    });
    builder.Services.AddAuthorization();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c => {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "SynopsisSI - User & Auth Service API", Version = "v1" });
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
    if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserService.API v1")); app.UseDeveloperExceptionPage(); }

    app.UseRouting();
    app.UseCors("AllowAll");
    app.UseAuthentication(); 
    app.UseAuthorization();
    app.MapControllers();
    app.MapGet("/health/users", () => Results.Ok(new { Status = "Healthy", Service = "UserService", Timestamp = DateTime.UtcNow }));
    Log.Information("UserService.API host starting...");
    app.Run();
}
catch (Exception ex) { Log.Fatal(ex, "UserService.API host terminated unexpectedly."); throw; }
finally { Log.CloseAndFlush(); }
