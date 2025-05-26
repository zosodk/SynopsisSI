# SynopsisSI E-commerce Platform 🚀 (Microservice Architecture)

Welcome to SynopsisSI, a modern, highly scalable e-commerce platform designed for second-hand item transactions. Built with **.NET 9** and a **microservice architecture**, this platform emphasizes clean architecture principles within each service, and system-wide resilience and scalability through distributed design patterns. It features dedicated services for core functionalities, robust data management with MongoDB, efficient caching with Redis, flexible cloud storage via MinIO/S3, and comprehensive observability.

## Solution Overview

SynopsisSI is architected as a collection of **independent, focused microservices** that work together to provide a comprehensive e-commerce experience. This approach allows for:

* **Independent Scalability:** Each service (e.g., Listings, Orders, Users) can be scaled up or down based on its specific load, optimizing resource usage.
* **Independent Deployability:** Changes to one service can be deployed without impacting others, leading to faster release cycles and reduced risk.
* **Technology Flexibility:** While primarily built on .NET 9, each microservice *could* theoretically use different technologies if a specific need arose (though consistency is maintained for this project).
* **Team Autonomy:** Enables smaller, focused teams to own and develop individual services.
* **Resilience:** Issues in one service are less likely to bring down the entire platform, especially when combined with patterns like circuit breakers.

An **API Gateway** (built with YARP) serves as the single entry point for all external client requests. It handles routing to the appropriate backend microservices, and can also manage cross-cutting concerns like initial authentication, rate limiting, and request/response transformations.

Communication between services is designed to be primarily **asynchronous and event-driven** where appropriate (e.g., using a message broker like RabbitMQ for events like `OrderPlacedEvent`), promoting loose coupling and resilience. For queries that might need data from multiple services, strategies like API composition at the gateway or dedicated query services can be employed.

## ✨ Core Features (Distributed by Service)

* **API Gateway (`SynopsisSI.Gateway`):** Single entry point, routing, and potential cross-cutting concerns.
* **Listing Service (`ListingService`):** Manages all aspects of product listings (CRUD, search, categories, status updates) with its own dedicated data considerations.
* **Order Service (`OrderService`):** Handles secure and atomic transaction processing for placing orders. Will use Sagas for distributed consistency when interacting with other services (e.g., ListingService for inventory/status updates, PaymentService).
* **User Service (`UserService` - Future):** Manages user profiles, registration, preferences.
* **Authentication Service (`AuthService` - Future):** Dedicated service for handling user authentication (e.g., JWT generation/validation) and authorization policies.
* **Review Service (`ReviewService` - Future):** Manages user-submitted reviews for sellers and items.
* **Cloud Storage Integration:** Seamless AWS S3/MinIO integration for storing and serving item images, managed by services that handle media (e.g., ListingService for its images).
* **Caching System:** Distributed Redis-based caching for frequently accessed data to improve performance across services.
* **API Documentation:** Integrated OpenAPI/Swagger per service, potentially aggregatable at the API Gateway.
* **Distributed Observability:**
  * **Structured Logging:** Comprehensive logging with Serilog, designed for aggregation in a central system.
  * **Distributed Tracing:** OpenTelemetry for end-to-end request tracing across microservices.
  * **Metrics:** OpenTelemetry for collecting metrics from each service and system components.
* **CORS Support:** Configured at the API Gateway and/or per-service for client applications.
* **Health Checks:** Per-service and potentially aggregated health checks for monitoring the entire system.
* **Containerized Environment:** Full `docker-compose` setup for easy local development of multiple services and dependencies. Kubernetes is recommended for production deployments.

## 🛠 Tech Stack

* **Framework:** ASP.NET Core 9 (for each microservice and the API Gateway)
* **Databases & Data Management:**
  * **MongoDB:** Primary choice for services requiring flexible schemas and scalability (e.g., `ListingService`, `UserService`). Accessed via Entity Framework Core with the official MongoDB provider or the native MongoDB C# driver, depending on the service's needs.
  * **Polyglot Persistence (Concept):** While initially focused on MongoDB, the microservice architecture allows for future use of different database technologies for different services if their specific requirements dictate (e.g., a relational database for a complex financial transaction service, a graph database for social features). Each service owns its data schema and can choose the best storage solution.
  * **Entity Framework Core 9:** Used for data access within services where an ORM is beneficial.
* **Caching:** Redis (potentially clustered in production)
* **Cloud Storage:** AWS S3 / MinIO (S3-compatible)
* **API Gateway:** YARP (Yet Another Reverse Proxy)
* **Messaging (for Asynchronous Communication - to be introduced):** RabbitMQ (or Kafka, Azure Service Bus, AWS SQS/SNS)
* **API Documentation:** Swagger (Swashbuckle) per service.
* **Observability:** Serilog, OpenTelemetry (with exporters for backends like Jaeger, Prometheus).
* **Architecture:** Microservices, Clean Architecture (applied within each service), CQRS (applied within each service), Event-Driven Architecture patterns, Saga pattern for distributed transactions.
* **Containerization:** Docker & Docker Compose (for development), Kubernetes (recommended for production).

## 🏗 Project Structure

The solution is organized to support multiple independent microservices, shared libraries, and an API Gateway.

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

## 💾 Database Solution & Data Management in a Microservice Architecture

The transition to microservices fundamentally changes how data is managed, moving from a single, centralized database to a decentralized model.

### Data Ownership & Decentralization ("Database per Service")

* **Core Principle:** Each microservice is responsible for its own data and should ideally have its own dedicated database or a clearly isolated section within a larger data store. For example:
  * `ListingService` manages its listing data in its own MongoDB collection(s) or database.
  * `OrderService` manages order data in its own store.
* **Benefits:** This strong data encapsulation ensures services are loosely coupled. Changes to one service's data schema do not directly impact other services. It also allows each service to choose the database technology best suited for its needs (leading to **polyglot persistence** if different database types are used across the system, though we are starting with MongoDB for services like Listings).
* **Implementation with MongoDB:** While each service *could* have a physically separate MongoDB instance, it's also common for services to have their own *databases* within a shared MongoDB cluster, or at least their own dedicated *collections* prefixed or clearly namespaced to that service. This provides logical separation while potentially simplifying infrastructure management in some scenarios.

### CQRS within Microservices

The Command Query Responsibility Segregation (CQRS) pattern remains highly beneficial *within each individual microservice*:

* The `ListingService.API` will have its own set of commands (e.g., `CreateListingCommand`, `UpdateListingStatusCommand`) and queries (e.g., `GetListingByIdQuery`, `SearchListingsQuery`) specific to listing management.
* Similarly, the `OrderService.API` will manage its own order-related commands and queries.
* This allows each service to optimize its write models for consistency and business rule enforcement, and its read models for efficient data retrieval tailored to specific use cases.

### Distributed Transactions & Eventual Consistency (The Saga Pattern)

When a single business operation requires changes across multiple microservices (e.g., placing an order might involve the `OrderService`, `ListingService` (to update stock/status), a future `PaymentService`, and a `NotificationService`), traditional distributed ACID transactions (like 2PC) are generally avoided due to their complexity and impact on availability in a distributed system.

* **The Saga Pattern:** This is the primary approach for managing data consistency across services in such scenarios. A saga is a sequence of local transactions. Each step in the saga is a local transaction within a single service. If one local transaction fails, the saga executes compensating transactions to undo the work done by preceding successful local transactions, aiming for **eventual consistency**.
  * **Example Saga Flow (Order Placement):**
    1.  `OrderService`: Creates an order in a "Pending" state (local transaction). Publishes `OrderPendingEvent`.
    2.  `ListingService`: Consumes `OrderPendingEvent`, attempts to reserve stock/mark listing as "Reserved" (local transaction). Publishes `ListingReservedEvent` or `StockReservationFailedEvent`.
    3.  `PaymentService` (Future): Consumes `ListingReservedEvent`, processes payment (local transaction). Publishes `PaymentSuccessfulEvent` or `PaymentFailedEvent`.
    4.  `OrderService`: Consumes payment events. If successful, updates order to "Paid" (local transaction) and publishes `OrderConfirmedEvent`. If payment failed, it might publish `OrderFailedEvent` and trigger compensating actions (e.g., `ListingService` unreserving the item).
    5.  `NotificationService` (Future): Consumes `OrderConfirmedEvent` or `OrderFailedEvent` to notify the user.
* **Asynchronous Messaging:** A **message broker** (e.g., RabbitMQ, Kafka, which will be added to `docker-compose.yml` later) is essential for implementing sagas and other event-driven communication. It allows services to publish events and other services to subscribe to them reliably and asynchronously.
* **Eventual Consistency:** It's crucial to understand that with this pattern, the overall system state becomes consistent *eventually*, not instantaneously. UIs and business processes must be designed with this in mind (e.g., an order might briefly appear "Pending" even if payment is processing).

### Data Access within Each Microservice

* **EF Core with MongoDB Provider / Native MongoDB Driver:** Each microservice can choose its data access strategy.
  * EF Core (via a service-specific `DbContext` like `ListingServiceDbContext` and a service-specific `IUnitOfWork`) can be used for services where its ORM capabilities are beneficial.
  * Alternatively, a service might opt to use the native MongoDB C# driver for more fine-grained control or to leverage MongoDB features not fully exposed through EF Core.
* **Optimistic Concurrency:** The `Version` property and `IsConcurrencyToken()` configuration remain important for handling concurrent updates to documents *within the scope of a single service's database*.

This distributed data management strategy, combined with CQRS and Sagas, allows for building a highly scalable and resilient e-commerce platform where services can evolve independently.

## 🚀 Achieving High Scalability (The Scale Cube in Microservices)

The microservice architecture inherently enables advanced scalability strategies.

### X-axis Scaling: Horizontal Duplication & Replication

* **Each Microservice:** Can be independently scaled by running multiple container instances behind its own internal load balancer (often managed by Kubernetes).
* **Databases (per service):** MongoDB replica sets for each service's data store.
* **Redis:** Can be a shared cluster or dedicated instances per service group if caching needs are isolated.
* **API Gateway:** Can also be scaled horizontally.

### Y-axis Scaling: Functional Decomposition

* This is the core principle of the microservice architecture itself. The application is decomposed into services based on function/domain.
* Further decomposition is possible if a single microservice becomes too large or complex.

### Z-axis Scaling: Data Partitioning (Sharding)

* **MongoDB Sharding (per service):** If a specific microservice's dataset (e.g., `ListingService` or `OrderService`) grows immensely, its dedicated MongoDB instance/cluster can be sharded.
* **Redis Clustering:** A shared Redis cluster can be used by multiple services, with data sharded across Redis nodes.
* **MinIO/S3:** These object storage systems inherently manage data distribution and sharding internally across their infrastructure.

### Infrastructure & Orchestration for Microservices

* **Container Orchestration (Kubernetes):** Essential for managing a fleet of microservices. Handles deployment, scaling, service discovery, load balancing, self-healing, and configuration management.
* **API Gateway:** Manages external traffic, routing, authentication/authorization offload, rate limiting, request/response transformation.
* **Message Broker (e.g., RabbitMQ, Kafka):** For asynchronous communication, event-driven architectures, and implementing Sagas.
* **Service Discovery (e.g., Consul, Kubernetes DNS):** Allows services to find and communicate with each other dynamically.
* **Distributed Tracing & Monitoring (OpenTelemetry):** Even more critical in a microservice environment to track requests across multiple service hops and diagnose issues. Centralized logging and metrics aggregation are vital.
* **Configuration Management:** Centralized configuration (e.g., Spring Cloud Config, HashiCorp Consul, Kubernetes ConfigMaps/Secrets) for managing settings across many services. HashiCorp Vault remains crucial for secrets.

This microservice architecture provides the ultimate flexibility for scaling different parts of your e-commerce platform independently, choosing the right tools for each job, and enabling autonomous development teams. However, it also introduces significant operational complexity and requires careful design of inter-service communication and data consistency strategies.

## 🚦 Getting Started

### Prerequisites

* .NET 9.0 SDK or later
* Docker Desktop
* An IDE like JetBrains Rider or Visual Studio 2022+
* Git

### Configuration

* Each microservice (e.g., `ListingService.API`, `OrderService.API`) will have its own `appsettings.json` and environment-specific overrides (e.g., `appsettings.Development.json`).
* The `docker-compose.yml` file will define environment variables for each service, including connection strings for its dedicated database (or shared dev database), message broker URLs, API gateway addresses, and potentially Vault details for secrets management.

### Running the Microservices with Docker Compose

1.  **Clone the repository.**
2.  **Navigate to the root directory** containing `docker-compose.yml`.
3.  **The `docker-compose.yml` will define:**
  * Build contexts and Dockerfiles for each microservice API (e.g., `ListingService.API`, `OrderService.API`).
  * The `SynopsisSI.Gateway` service.
  * Backing services: MongoDB, Redis, MinIO.
  * (Later) A message broker (e.g., RabbitMQ).
  * (Later) HashiCorp Vault for secrets.
4.  **Start all services:**
    ```bash
    docker-compose up --build -d
    ```

### Accessing Services

* Clients (like a Blazor WASM app or Postman) will primarily interact with the **API Gateway's** exposed URL (e.g., `http://localhost:8080` as configured in `docker-compose.yml`).
* Individual services typically do not expose ports directly to the host in a microservice setup managed by Docker Compose; all traffic goes through the gateway. For development debugging, you can temporarily map ports.
* Swagger UI will be available per service (e.g., `http://localhost:PORT_FOR_LISTING_SERVICE/swagger` if you temporarily map its port) and can potentially be aggregated or linked from the API Gateway.

## 🐳 Basic Docker Compose Commands

* **View running services:** `docker-compose ps`
* **View logs for all services (follow):** `docker-compose logs -f`
* **View logs for a specific service:** `docker-compose logs -f <service_name_in_compose_file>`
* **Stop all services:** `docker-compose down`
* **Stop and remove volumes (deletes data):** `docker-compose down -v`
* **Rebuild and restart a specific service (e.g., API):**
    ```bash
    docker-compose build <service_name_in_compose_file>
    docker-compose up -d --no-deps <service_name_in_compose_file>
    ```

## 📖 API Documentation (via Swagger UI)

*(This section would detail how to access Swagger, likely via the Gateway or per service for dev, and highlight key endpoints as they are developed. The previous example content for endpoints, response codes, formats, etc., can be adapted and expanded here as each service's API is defined.)*

### Example Endpoints (Accessed via API Gateway: `http://localhost:8080`)

#### Listings (`/api/listings`)

* **`GET /api/listings/{id}`**
* **`POST /api/listings`**

#### Orders (`/api/orders` - To be implemented)

* **`POST /api/orders`**
* **`GET /api/orders/{id}`**

*(More endpoint details will be added as services are built.)*

### Health Checks

* **Gateway Health:** `http://localhost:8080/` (or a dedicated `/health` endpoint on the gateway)
* **ListingService Health (via Gateway if routed, or direct if port mapped for dev):** The Gateway's `appsettings.json` configures it to check `/health/listings` on the `listingservice.api`.

### Authentication

* Authentication will be handled centrally, likely by a dedicated `AuthService`, and tokens (e.g., JWT) will be validated at the API Gateway or by individual services.
* Protected endpoints will require a Bearer token in the `Authorization` header:
  `Authorization: Bearer {your_jwt_access_token}`

*(The sections "Standard Response Codes", "General API Conventions (Placeholders)", "Contributing", and "License" would remain conceptually similar to what you provided, updated as the project evolves.)*
