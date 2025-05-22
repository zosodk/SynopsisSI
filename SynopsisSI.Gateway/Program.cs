using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Load YARP configuration from appsettings.json and environment variables
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// app.UseHttpsRedirection(); // Enable if your gateway and services use HTTPS

// Enable YARP routing for all configured routes
app.MapReverseProxy();

//A simple root endpoint for the gateway itself
app.MapGet("/", () => $"API Gateway (SynopsisSI) is running. Environment: {app.Environment.EnvironmentName}");

app.Run();