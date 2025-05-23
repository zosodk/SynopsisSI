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
    using SynopsisSI.Services.ListingService.Application.Interfaces.Infrastructure;
    using SynopsisSI.Services.ListingService.Infrastructure.Services;
    using Serilog;
    using System;
    using Microsoft.OpenApi.Models;
    using Microsoft.AspNetCore.Http;
    using Amazon.S3; // For IAmazonS3 and AmazonS3Config
    using Amazon.Extensions.NETCore.Setup; // For AWSOptions
    // using Amazon.Runtime; // ClientConfig is in AWSSDK.Core, usually brought by AWSSDK.S3

    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
        .Enrich.FromLogContext().WriteTo.Console().CreateBootstrapLogger();

    try
    {
        Log.Information("Starting ListingService.API host builder...");
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ApplicationName", "ListingService.API")
            .Enrich.WithEnvironmentName());

        var configuration = builder.Configuration;
        var mongoConnectionString = configuration.GetConnectionString("ListingServiceMongoDb");
        var mongoDatabaseName = configuration["MongoDbDatabaseName"];

        if (string.IsNullOrEmpty(mongoConnectionString) || string.IsNullOrEmpty(mongoDatabaseName))
        {
            Log.Fatal("ListingService MongoDB connection string or database name is not configured.");
            throw new InvalidOperationException("ListingService MongoDB connection string or database name is not configured.");
        }

        builder.Services.AddDbContext<ListingServiceDbContext>(options =>
            options.UseMongoDB(mongoConnectionString, mongoDatabaseName));

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IListingRepository, EfCoreListingRepository>();

        builder.Services.AddScoped<CreateListingCommandHandler>();
        builder.Services.AddScoped<GetListingByIdQueryHandler>();

        // Configure AWS S3 client specifically for MinIO
        var cloudStorageConfig = configuration.GetSection("CloudStorage");
        var s3ClientConfig = new AmazonS3Config(); // Use the service-specific config

        if (!string.IsNullOrEmpty(cloudStorageConfig["ServiceURL"]))
        {
            s3ClientConfig.ServiceURL = cloudStorageConfig["ServiceURL"];
        }
        if (bool.TryParse(cloudStorageConfig["S3ForcePathStyle"], out bool forcePathStyle))
        {
            s3ClientConfig.ForcePathStyle = forcePathStyle;
        }
        // If a specific AWS region is set in config and relevant (e.g., for SDK's default behavior if not MinIO)
        if (!string.IsNullOrEmpty(cloudStorageConfig["Region"]))
        {
             s3ClientConfig.RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(cloudStorageConfig["Region"]);
        }

        // Register IAmazonS3 with the custom configuration for MinIO
        // AWSCredentials can be picked up from environment/profile by default if not specified here.
        // For explicit credentials (e.g. from Vault or other config source), you'd pass them to AmazonS3Client constructor.
        builder.Services.AddSingleton<IAmazonS3>(sp => new AmazonS3Client(s3ClientConfig));
        
        builder.Services.AddSingleton<ICloudStorageService, CloudStorageService>();


        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "SynopsisSI - Listing Service API", Version = "v1" });
        });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
                policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        });

        var app = builder.Build();
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ListingService.API v1"));
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();
        app.UseCors("AllowAll");
        app.MapControllers();
        app.MapGet("/health/listings", () => Results.Ok(new { Status = "Healthy", Service = "ListingService", Timestamp = DateTime.UtcNow }));

        Log.Information("ListingService.API host starting...");
        app.Run();
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "ListingService.API host terminated unexpectedly.");
        throw;
    }
    finally
    {
        Log.CloseAndFlush();
    }