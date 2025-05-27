FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# First copy only the solution file
COPY ["SynopsisSI.sln", "./"]

# Create necessary directories and copy project files maintaining structure
COPY ["src/Services/ListingService/ListingService.API/*.csproj", "src/Services/ListingService/ListingService.API/"]
COPY ["src/Services/ListingService/ListingService.Application/*.csproj", "src/Services/ListingService/ListingService.Application/"]
COPY ["src/Services/ListingService/ListingService.Domain/*.csproj", "src/Services/ListingService/ListingService.Domain/"]
COPY ["src/Services/ListingService/ListingService.Infrastructure/*.csproj", "src/Services/ListingService/ListingService.Infrastructure/"]
COPY ["src/Services/OrderService/OrderService.API/*.csproj", "src/Services/OrderService/OrderService.API/"]
COPY ["src/Services/OrderService/OrderService.Application/*.csproj", "src/Services/OrderService/OrderService.Application/"]
COPY ["src/Services/OrderService/OrderService.Domain/*.csproj", "src/Services/OrderService/OrderService.Domain/"]
COPY ["src/Services/OrderService/OrderService.Infrastructure/*.csproj", "src/Services/OrderService/OrderService.Infrastructure/"]
COPY ["src/Shared/SynopsisSI.Shared.Domain/*.csproj", "src/Shared/SynopsisSI.Shared.Domain/"]
COPY ["src/Shared/SynopsisSI.Shared.Events/*.csproj", "src/Shared/SynopsisSI.Shared.Events/"]
COPY ["src/Shared/SynopsisSI.Shared.Infrastructure/*.csproj", "src/Shared/SynopsisSI.Shared.Infrastructure/"]
COPY ["src/Gateways/SynopsisSI.Gateway/*.csproj", "src/Gateways/SynopsisSI.Gateway/"]
# UserService specific projects
COPY src/Services/UserService/UserService.Domain/UserService.Domain.csproj ./src/Services/UserService/UserService.Domain/
COPY src/Services/UserService/UserService.Application/UserService.Application.csproj ./src/Services/UserService/UserService.Application/
COPY src/Services/UserService/UserService.Infrastructure/UserService.Infrastructure.csproj ./src/Services/UserService/UserService.Infrastructure/
COPY src/Services/UserService/UserService.API/UserService.API.csproj ./src/Services/UserService/UserService.API/
# Restore as distinct layers
RUN dotnet restore "SynopsisSI.sln"

# Copy everything else
COPY . .

WORKDIR "/source/src/Services/ListingService/ListingService.API"
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/* \
    && adduser --system --no-create-home --disabled-password --group appuser \
    && chown -R appuser:appuser /app
USER appuser

EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

HEALTHCHECK --interval=15s --timeout=5s --retries=3 --start-period=30s \
    CMD curl -f http://localhost:80/health/listings || exit 1

ENTRYPOINT ["dotnet", "SynopsisSI.Services.ListingService.API.dll"]