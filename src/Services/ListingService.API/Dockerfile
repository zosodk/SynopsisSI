    # Stage 1: Build
    FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
    WORKDIR /source

    # Copy solution file (it's at the root of the context)
    COPY SynopsisSI.sln .

    # Copy ALL .csproj files from the solution, maintaining their physical paths from solution root
    # Service Projects
    COPY ListingService.Domain/ListingService.Domain.csproj ./ListingService.Domain/
    COPY ListingService.Application/ListingService.Application.csproj ./ListingService.Application/
    COPY ListingService.Infrastructure/ListingService.Infrastructure.csproj ./ListingService.Infrastructure/
    COPY ListingService.API/ListingService.API.csproj ./ListingService.API/
    # (Add other service project .csproj files here as they are created, e.g., OrderService, UserService)

    # Gateway Project
    COPY SynopsisSI.Gateway/SynopsisSI.Gateway.csproj ./SynopsisSI.Gateway/

    # Shared Library Projects
    COPY SynopsisSI.Shared.Domain/SynopsisSI.Shared.Domain.csproj ./SynopsisSI.Shared.Domain/
    COPY SynopsisSI.Shared.Events/SynopsisSI.Shared.Events.csproj ./SynopsisSI.Shared.Events/
    COPY SynopsisSI.Shared.Infrastructure/SynopsisSI.Shared.Infrastructure.csproj ./SynopsisSI.Shared.Infrastructure/
    # (Ensure SynopsisSI.Shared.Infrastructure.csproj exists if it's in your .sln)


    # Restore dependencies for the entire solution
    RUN dotnet restore SynopsisSI.sln

    # Copy the entire source code from the context (solution root) into /source in the image
    # The .dockerignore file at the solution root will exclude unnecessary files.
    COPY . .

    # Set the working directory to the specific API project to be published
    WORKDIR /source/ListingService.API
    RUN dotnet publish -c Release -o /app/publish --no-restore

    # Stage 2: Runtime
    FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
    WORKDIR /app
    COPY --from=build-env /app/publish .

    EXPOSE 8080
    ENTRYPOINT ["dotnet", "SynopsisSI.Services.ListingService.API.dll"]