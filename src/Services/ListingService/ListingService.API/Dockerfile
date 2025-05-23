    # Stage 1: Build
    FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
    WORKDIR /source

    # Copy solution file (it's at the root of the context)
    COPY SynopsisSI.sln .

    # Copy .csproj files, maintaining their physical paths from solution root (including 'src')
    COPY src/Services/ListingService/ListingService.Domain/ListingService.Domain.csproj ./src/Services/ListingService/ListingService.Domain/
    COPY src/Services/ListingService/ListingService.Application/ListingService.Application.csproj ./src/Services/ListingService/ListingService.Application/
    COPY src/Services/ListingService/ListingService.Infrastructure/ListingService.Infrastructure.csproj ./src/Services/ListingService/ListingService.Infrastructure/
    COPY src/Services/ListingService/ListingService.API/ListingService.API.csproj ./src/Services/ListingService/ListingService.API/

    # Copy shared library project files
    COPY src/Shared/SynopsisSI.Shared.Domain/SynopsisSI.Shared.Domain.csproj ./src/Shared/SynopsisSI.Shared.Domain/
    COPY src/Shared/SynopsisSI.Shared.Events/SynopsisSI.Shared.Events.csproj ./src/Shared/SynopsisSI.Shared.Events/
    COPY src/Shared/SynopsisSI.Shared.Infrastructure/SynopsisSI.Shared.Infrastructure.csproj ./src/Shared/SynopsisSI.Shared.Infrastructure/

    # Copy Gateway .csproj for solution restore
    COPY src/Gateways/SynopsisSI.Gateway/SynopsisSI.Gateway.csproj ./src/Gateways/SynopsisSI.Gateway/
    # (Add other service .csproj files here as they are created for the solution restore)

    # Restore dependencies for the entire solution
    RUN dotnet restore SynopsisSI.sln

    # Copy the entire source code from the context (solution root) into /source in the image
    # The .dockerignore file at the solution root will exclude unnecessary files.
    COPY . .

    # Set the working directory to the specific API project to be published
    WORKDIR /source/src/Services/ListingService/ListingService.API
    RUN dotnet publish -c Release -o /app/publish --no-restore

    # Stage 2: Runtime
    FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
    WORKDIR /app
    COPY --from=build-env /app/publish .

    EXPOSE 8080
    ENTRYPOINT ["dotnet", "ListingService.API.dll"]