using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using SynopsisSI.Services.ListingService.Infrastructure.Persistence;
using SynopsisSI.Services.ListingService.Application.Interfaces.Persistence;
using SynopsisSI.Services.ListingService.Infrastructure.Persistence.Repositories;
using SynopsisSI.Services.ListingService.Infrastructure.Persistence.Common;
using SynopsisSI.Services.ListingService.Application.Features.Listings.Commands.CreateListing;
using SynopsisSI.Services.ListingService.Application.Features.Listings.Queries.GetListingById;
using SynopsisSI.Services.ListingService.Application.Features.Listings.Commands.UpdateListing;
using SynopsisSI.Services.ListingService.Application.Features.Listings.Commands.DeleteListing;
using SynopsisSI.Services.ListingService.Application.Features.Listings.Queries.SearchListings;
using SynopsisSI.Services.ListingService.Application.Interfaces.Infrastructure;
using SynopsisSI.Services.ListingService.Infrastructure.Services;
using Serilog;
using System;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http;
using Amazon.S3;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using MassTransit;
using SynopsisSI.Services.ListingService.Application.Features.Listings.EventConsumers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

        Log.Logger = new LoggerConfiguration().MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
            .Enrich.FromLogContext().WriteTo.Console().CreateBootstrapLogger();
        try
        {
            Log.Information("Starting ListingService.API host builder...");
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration).Enrich.WithProperty("ApplicationName", "ListingService.API"));

            var config = builder.Configuration;
            builder.Services.AddDbContext<ListingServiceDbContext>(opt =>
                opt.UseMongoDB(config.GetConnectionString("ListingServiceMongoDb")!, config["MongoDbDatabaseName"]!));
            
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IListingRepository, EfCoreListingRepository>();

            builder.Services.AddScoped<CreateListingCommandHandler>();
            builder.Services.AddScoped<GetListingByIdQueryHandler>();
            builder.Services.AddScoped<UpdateListingCommandHandler>();
            builder.Services.AddScoped<DeleteListingCommandHandler>();
            builder.Services.AddScoped<SearchListingsQueryHandler>();

            // Configure AWS S3 client specifically for MinIO or other S3-compatible storage
            var cloudStorageConfig = config.GetSection("CloudStorage");
            var s3ClientConfig = new AmazonS3Config();

            if (!string.IsNullOrEmpty(cloudStorageConfig["ServiceURL"]))
            {
                s3ClientConfig.ServiceURL = cloudStorageConfig["ServiceURL"];
            }
            if (bool.TryParse(cloudStorageConfig["S3ForcePathStyle"], out bool forcePathStyle))
            {
                s3ClientConfig.ForcePathStyle = forcePathStyle;
            }
            if (!string.IsNullOrEmpty(cloudStorageConfig["Region"]))
            {
                s3ClientConfig.RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(cloudStorageConfig["Region"]);
            }

            // Register IAmazonS3 with the custom configuration
            builder.Services.AddSingleton<IAmazonS3>(sp => new AmazonS3Client(s3ClientConfig));
            builder.Services.AddSingleton<ICloudStorageService, CloudStorageService>();


            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<OrderPlacedEventConsumer>();
                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqHost = config["MessageBroker:RabbitMQ:Host"] ?? "rabbitmq";
                    var rabbitMqUser = config["MessageBroker:RabbitMQ:Username"] ?? "user";
                    var rabbitMqPass = config["MessageBroker:RabbitMQ:Password"] ?? "password";
                    cfg.Host(rabbitMqHost, "/", h => { h.Username(rabbitMqUser); h.Password(rabbitMqPass); });
                    cfg.ReceiveEndpoint("listing-service-order-placed-event-consumer", e =>
                    {
                        e.ConfigureConsumer<OrderPlacedEventConsumer>(context);
                    });
                });
            });

            var jwtSettings = config.GetSection("JwtSettings");
            var secretKeyString = jwtSettings["SecretKey"];
            if (string.IsNullOrEmpty(secretKeyString) || secretKeyString.Length < 32)
                Log.Warning("JWT SecretKey for ListingService token validation is not configured or too short.");
            var key = Encoding.ASCII.GetBytes(secretKeyString ?? Guid.NewGuid().ToString("N"));

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
            builder.Services.AddAuthorization();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SynopsisSI - Listing Service API", Version = "v1" });
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
            if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ListingService.API v1")); app.UseDeveloperExceptionPage(); }
            
            app.UseRouting();
            app.UseCors("AllowAll");
            app.UseAuthentication(); 
            app.UseAuthorization();
            app.MapControllers();
            app.MapGet("/health/listings", () => Results.Ok(new { Status = "Healthy", Service = "ListingService", Timestamp = DateTime.UtcNow }));
            Log.Information("ListingService.API host starting...");
            app.Run();
        }
        catch (Exception ex) { Log.Fatal(ex, "ListingService.API host terminated unexpectedly."); throw; }
        finally { Log.CloseAndFlush(); }