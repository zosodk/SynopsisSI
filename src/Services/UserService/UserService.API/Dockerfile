FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /source

COPY SynopsisSI.sln .
# UserService specific projects
COPY src/Services/UserService/UserService.Domain/UserService.Domain.csproj ./src/Services/UserService/UserService.Domain/
COPY src/Services/UserService/UserService.Application/UserService.Application.csproj ./src/Services/UserService/UserService.Application/
COPY src/Services/UserService/UserService.Infrastructure/UserService.Infrastructure.csproj ./src/Services/UserService/UserService.Infrastructure/
COPY src/Services/UserService/UserService.API/UserService.API.csproj ./src/Services/UserService/UserService.API/

# Shared libraries
COPY src/Shared/SynopsisSI.Shared.Domain/SynopsisSI.Shared.Domain.csproj ./src/Shared/SynopsisSI.Shared.Domain/
COPY src/Shared/SynopsisSI.Shared.Events/SynopsisSI.Shared.Events.csproj ./src/Shared/SynopsisSI.Shared.Events/
COPY src/Shared/SynopsisSI.Shared.Infrastructure/SynopsisSI.Shared.Infrastructure.csproj ./src/Shared/SynopsisSI.Shared.Infrastructure/

# Other services' .csproj for full solution restore
COPY src/Gateways/SynopsisSI.Gateway/SynopsisSI.Gateway.csproj ./src/Gateways/SynopsisSI.Gateway/
COPY src/Services/ListingService/ListingService.Domain/ListingService.Domain.csproj ./src/Services/ListingService/ListingService.Domain/
COPY src/Services/ListingService/ListingService.Application/ListingService.Application.csproj ./src/Services/ListingService/ListingService.Application/
COPY src/Services/ListingService/ListingService.Infrastructure/ListingService.Infrastructure.csproj ./src/Services/ListingService/ListingService.Infrastructure/
COPY src/Services/ListingService/ListingService.API/ListingService.API.csproj ./src/Services/ListingService/ListingService.API/

# Assuming OrderService projects also exist and are part of the solution:
COPY src/Services/OrderService/OrderService.Domain/OrderService.Domain.csproj ./src/Services/OrderService/OrderService.Domain/
COPY src/Services/OrderService/OrderService.Application/OrderService.Application.csproj ./src/Services/OrderService/OrderService.Application/
COPY src/Services/OrderService/OrderService.Infrastructure/OrderService.Infrastructure.csproj ./src/Services/OrderService/OrderService.Infrastructure/
COPY src/Services/OrderService/OrderService.API/OrderService.API.csproj ./src/Services/OrderService/OrderService.API/


RUN dotnet restore SynopsisSI.sln
COPY . .

WORKDIR /source/src/Services/UserService/UserService.API
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build-env /app/publish .

# Install curl for healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/* \
    && adduser --system --no-create-home --disabled-password --group appuser \
    && chown -R appuser:appuser /app
USER appuser

EXPOSE 80 
ENV ASPNETCORE_URLS=http://+:80

HEALTHCHECK --interval=15s --timeout=5s --retries=3 --start-period=30s \
    CMD curl -f http://localhost:80/health/users || exit 1

# Default assembly name for a project named "UserService.API" is "UserService.API.dll".
# If you have set <AssemblyName>SynopsisSI.Services.UserService.API</AssemblyName> in csproj, then use that.
ENTRYPOINT ["dotnet", "UserService.API.dll"]