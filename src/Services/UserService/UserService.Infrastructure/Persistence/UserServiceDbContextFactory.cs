using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using System; // For Environment

namespace SynopsisSI.Services.UserService.Infrastructure.Persistence;

public class UserServiceDbContextFactory : IDesignTimeDbContextFactory<UserServiceDbContext>
{
    public UserServiceDbContext CreateDbContext(string[] args)
    {
        string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var apiProjectBasePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "UserService.API"));

        if (!Directory.Exists(apiProjectBasePath))
        {
            apiProjectBasePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "src", "Services", "UserService", "UserService.API"));
             if (!Directory.Exists(apiProjectBasePath))
             {
                throw new InvalidOperationException($"Could not find UserService.API project directory at '{apiProjectBasePath}' to load appsettings.json. Current: '{Directory.GetCurrentDirectory()}'.");
             }
        }

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(apiProjectBasePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables().Build();

        var optionsBuilder = new DbContextOptionsBuilder<UserServiceDbContext>();
        var connectionString = configuration.GetConnectionString("UserServicePostgresDb");

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException($"Connection string 'UserServicePostgresDb' not found. Searched in: '{Path.Combine(apiProjectBasePath, $"appsettings.{environment}.json")}'.");

        optionsBuilder.UseNpgsql(connectionString, npgsqlOptionsAction: sqlOptions =>
            sqlOptions.MigrationsAssembly(typeof(UserServiceDbContextFactory).Assembly.FullName));

        return new UserServiceDbContext(optionsBuilder.Options);
    }
}