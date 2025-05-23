# Build
    FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
    WORKDIR /app

    COPY ../../../../SynopsisSI.sln ./SynopsisSI.sln
    COPY ../ListingService.Domain/ListingService.Domain.csproj ./Services/ListingService/ListingService.Domain/
    COPY ../ListingService.Application/ListingService.Application.csproj ./Services/ListingService/ListingService.Application/
    COPY ../ListingService.Infrastructure/ListingService.Infrastructure.csproj ./Services/ListingService/ListingService.Infrastructure/
    COPY ./ListingService.API.csproj ./Services/ListingService/ListingService.API/
    COPY ../../../../Shared/SynopsisSI.Shared.Domain/SynopsisSI.Shared.Domain.csproj ./Shared/SynopsisSI.Shared.Domain/
    COPY ../../../../Shared/SynopsisSI.Shared.Events/SynopsisSI.Shared.Events.csproj ./Shared/SynopsisSI.Shared.Events/

    RUN dotnet restore SynopsisSI.sln

    COPY ../../../../Shared/SynopsisSI.Shared.Domain ./Shared/SynopsisSI.Shared.Domain
    COPY ../../../../Shared/SynopsisSI.Shared.Events ./Shared/SynopsisSI.Shared.Events
    COPY ../ListingService.Domain ./Services/ListingService/ListingService.Domain
    COPY ../ListingService.Application ./Services/ListingService/ListingService.Application
    COPY ../ListingService.Infrastructure ./Services/ListingService/ListingService.Infrastructure
    COPY . ./Services/ListingService/ListingService.API

    WORKDIR /app/Services/ListingService/ListingService.API
    RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime
    FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
    WORKDIR /app
    COPY --from=build-env /app/publish .
    EXPOSE 8080
    ENTRYPOINT ["dotnet", "SynopsisSI.Services.ListingService.API.dll"]