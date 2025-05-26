# SynopsisSI E-commerce Platform 🚀

A modern, scalable e-commerce platform for second-hand item transactions, built with **.NET 9** and microservice architecture. Features clean architecture principles, distributed design patterns, and comprehensive observability.

## Architecture Overview

Built as independent, focused microservices communicating through an **API Gateway** (YARP):

* **Independent Scaling & Deployment:** Each service scales and deploys independently
* **Resilient Design:** Issues in one service don't cascade to others
* **Event-Driven Communication:** Asynchronous messaging via message brokers
* **Data Autonomy:** Each service owns and manages its data

## Services & Features

### Current Services
* **API Gateway** (`SynopsisSI.Gateway`): Entry point and routing
* **Listing Service** (`ListingService`): Product listing management
* **Order Service** (`OrderService`): Transaction processing with Saga pattern

### Planned Services
* User Service
* Authentication Service
* Review Service

### Infrastructure Components
* **Storage & Caching:**
    * MongoDB (primary datastore)
    * Redis (distributed caching)
    * MinIO/S3 (object storage)
* **Messaging:** RabbitMQ (planned)
* **Observability:**
    * Serilog (logging)
    * OpenTelemetry (tracing & metrics)
* **Documentation:** OpenAPI/Swagger
* **Security:** CORS, Authentication, Authorization

## Project Structure

```text
SynopsisSI/
├── src/
│   ├── Gateways/
│   │   └── SynopsisSI.Gateway/              # YARP API Gateway Project
│   │
│   ├── Services/                              # Contains all individual microservice solutions/projects
│   │   ├── ListingService/
│   │   │   ├── ListingService.Domain/         # Listing-specific entities, value objects, domain logic
│   │   │   ├── ListingService.Application/    # Listing-specific CQRS, interfaces, application logic
│   │   │   ├── ListingService.Infrastructure/ # Listing-specific data access, external service clients
│   │   │   └── ListingService.API/            # Listing microservice's ASP.NET Core API
│   │   │
│   │   ├── OrderService/                      # (Similar structure for OrderService)
│   │   │   ├── OrderService.Domain/
│   │   │   ├── OrderService.Application/
│   │   │   ├── OrderService.Infrastructure/
│   │   │   └── OrderService.API/
│   │   │
│   │   └── ... (UserService, AuthService, etc.)
│   │
│   ├── Shared/                                # Libraries shared across microservices (use judiciously)
│   │   ├── SynopsisSI.Shared.Domain/          # Truly common domain primitives, base classes (optional)
│   │   ├── SynopsisSI.Shared.Events/          # DTOs for inter-service events (e.g., OrderPlacedEvent)
│   │   └── SynopsisSI.Shared.Infrastructure/  # Common infrastructure helpers (e.g., OTel setup, base message consumers - use with care)
│
├── tests/                                     # Test projects, ideally per service
│   ├── ListingService.UnitTests/
│   ├── ListingService.IntegrationTests/
│   └── ...
│
├── docker-compose.yml                         # Orchestrates all services and backing stores for local development
├── docker-compose.override.yml                # Optional: For local developer-specific overrides
├── SynopsisSI.sln                             # Main solution file including all projects
└── README.md                                  # This file
```


## Data Management

* **Database per Service:** Each service manages its own data store
* **CQRS Pattern:** Separate command and query models within services
* **Saga Pattern:** Maintains consistency across service boundaries
* **Event-Driven:** Asynchronous communication via message broker

## Scalability

* **X-axis:** Horizontal duplication
* **Y-axis:** Functional decomposition (microservices)
* **Z-axis:** Data sharding where needed

## Getting Started

### Prerequisites
* .NET 9.0 SDK
* Docker Desktop
* IDE (Rider/VS 2022+)
* Git

### Quick Start
1. Clone repository
2. Navigate to root directory
3. Run: `docker-compose up --build -d`

### Access Points
* API Gateway: `http://localhost:8080`
* Swagger UI: Available per service
* Health Checks: Via gateway `/health` endpoints

## Essential Commands
# Start services
docker-compose up -d
# View logs
docker-compose logs -f [service_name]
# Stop services
docker-compose down
# Rebuild specific service
docker-compose build  docker-compose up -d --no-deps


## API Documentation

### Base URL
`http://localhost:8080`

### Current Endpoints
* Listings: `GET/POST /api/listings`
* Orders: `GET/POST /api/orders`

### Authentication
Bearer token required: