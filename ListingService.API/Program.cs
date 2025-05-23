using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
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
    using Amazon.S3;
    using Amazon.Extensions.NETCore.Setup;

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

        AWSOptions? awsOptions = configuration.GetAWSOptions("CloudStorage");
        if (awsOptions != null)
        {
            var cloudStorageSection = configuration.GetSection("CloudStorage");
            if (!string.IsNullOrEmpty(cloudStorageSection["ServiceURL"]))
                awsOptions.DefaultClientConfig.ServiceURL = cloudStorageSection["ServiceURL"];
            if (bool.TryParse(cloudStorageSection["ForcePathStyle"], out bool forcePathStyleValue))
                awsOptions.DefaultClientConfig.ForcePathStyle = forcePathStyleValue;
            builder.Services.AddDefaultAWSOptions(awsOptions);
            builder.Services.AddAWSService<IAmazonS3>();
        }
        else Log.Warning("CloudStorage (AWS) Options not configured.");
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