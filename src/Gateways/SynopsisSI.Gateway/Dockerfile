# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

    # Copy .sln and ALL .csproj files first, relative to context (solution root)
    COPY SynopsisSI.sln .
    COPY SynopsisSI.Gateway/SynopsisSI.Gateway.csproj ./SynopsisSI.Gateway/

    # Service Projects (needed for solution restore if gateway doesn't directly reference them but .sln does)
    COPY ListingService.Domain/ListingService.Domain.csproj ./ListingService.Domain/
    COPY ListingService.Application/ListingService.Application.csproj ./ListingService.Application/
    COPY ListingService.Infrastructure/ListingService.Infrastructure.csproj ./ListingService.Infrastructure/
    COPY ListingService.API/ListingService.API.csproj ./ListingService.API/
    # (Add other service project .csproj files here as they are created)

    # Shared Library Projects
    COPY SynopsisSI.Shared.Domain/SynopsisSI.Shared.Domain.csproj ./SynopsisSI.Shared.Domain/
    COPY SynopsisSI.Shared.Events/SynopsisSI.Shared.Events.csproj ./SynopsisSI.Shared.Events/
    COPY SynopsisSI.Shared.Infrastructure/SynopsisSI.Shared.Infrastructure.csproj ./SynopsisSI.Shared.Infrastructure/
    # (Ensure SynopsisSI.Shared.Infrastructure.csproj exists if it's in your .sln)

    RUN dotnet restore SynopsisSI.sln

    # Copy the rest of the source code from the context (solution root)
    COPY . .

    # Set the working directory to the specific API project to be published
    WORKDIR /source/SynopsisSI.Gateway
    RUN dotnet publish "SynopsisSI.Gateway.csproj" -c Release -o /app/publish --no-restore

    # Stage 2: Runtime
    FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
    WORKDIR /app
    COPY --from=build /app/publish .
    EXPOSE 8080
    ENTRYPOINT ["dotnet", "SynopsisSI.Gateway.dll"]