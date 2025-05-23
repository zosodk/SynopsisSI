// Corrected based on the physical project structure (src/... paths)
// and assuming the assembly name is SynopsisSI.Gateway.dll

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copy .sln and ALL .csproj files first, relative to context (solution root), including 'src' prefix
COPY SynopsisSI.sln .
COPY src/Gateways/SynopsisSI.Gateway/SynopsisSI.Gateway.csproj ./src/Gateways/SynopsisSI.Gateway/

# Service Projects (needed for solution restore)
COPY src/Services/ListingService/ListingService.Domain/ListingService.Domain.csproj ./src/Services/ListingService/ListingService.Domain/
COPY src/Services/ListingService/ListingService.Application/ListingService.Application.csproj ./src/Services/ListingService/ListingService.Application/
COPY src/Services/ListingService/ListingService.Infrastructure/ListingService.Infrastructure.csproj ./src/Services/ListingService/ListingService.Infrastructure/
COPY src/Services/ListingService/ListingService.API/ListingService.API.csproj ./src/Services/ListingService/ListingService.API/
# (Add other service project .csproj files here as they are created)

# Shared Library Projects
COPY src/Shared/SynopsisSI.Shared.Domain/SynopsisSI.Shared.Domain.csproj ./src/Shared/SynopsisSI.Shared.Domain/
COPY src/Shared/SynopsisSI.Shared.Events/SynopsisSI.Shared.Events.csproj ./src/Shared/SynopsisSI.Shared.Events/
COPY src/Shared/SynopsisSI.Shared.Infrastructure/SynopsisSI.Shared.Infrastructure.csproj ./src/Shared/SynopsisSI.Shared.Infrastructure/

RUN dotnet restore SynopsisSI.sln

# Copy the rest of the source code from the context (solution root)
COPY . .

# Set the working directory to the specific API project to be published
WORKDIR /source/src/Gateways/SynopsisSI.Gateway
RUN dotnet publish "SynopsisSI.Gateway.csproj" -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "SynopsisSI.Gateway.dll"]