# SynopsisSI E-commerce Platform 🚀 (Microservice Architecture)

Welcome to SynopsisSI, a modern, highly scalable e-commerce platform designed for second-hand item transactions. Built with **.NET 9** and a **microservice architecture**, this platform emphasizes clean architecture principles within each service, alongside system-wide resilience and scalability through distributed design patterns. Key features include dedicated services for core e-commerce functionalities, robust data management with polyglot persistence (MongoDB and PostgreSQL), efficient caching with Redis, flexible cloud storage via MinIO (S3-compatible), comprehensive observability features (planned), and a containerized development environment.

This project serves as a practical case study for applying advanced software architecture principles and is intended to provide concrete assignments for academic coursework in "System Integration," "Databases for Developers,", but also work as a demonstration platform for the exam of "Distributed Large Systems."

The Documents submitted for the exam are located in the documentation folder in the root of this repository.

## Solution Overview

SynopsisSI is architected as a collection of **independent, focused microservices** that collaborate to deliver a comprehensive e-commerce experience. This design offers significant advantages:

* **Independent Scaling & Deployment:** Each core service (Listings, Orders, Users, etc.) can be scaled up or down based on its specific load and deployed independently, allowing for faster release cycles and optimized resource utilization.
* **Resilient Design:** Issues within one microservice are less likely to cascade and impact the entire platform, particularly when combined with resilience patterns like circuit breakers and retries (planned).
* **Technology Flexibility (Polyglot Persistence):** Services can leverage the database technology best suited to their specific data characteristics and needs (e.g., MongoDB for flexible product schemas, PostgreSQL for structured user identity).
* **Data Autonomy:** Each microservice owns and manages its data store, promoting clear boundaries and reducing inter-service dependencies at the data layer.
* **Event-Driven Communication:** Asynchronous messaging via a message broker (RabbitMQ) is used for critical inter-service workflows, promoting loose coupling and enhancing system responsiveness and fault tolerance.

An **API Gateway** (implemented with YARP) acts as the single, unified entry point for all external client requests, handling routing, and providing a location for cross-cutting concerns.

## 🛠 Tech Stack

* **Framework:** .NET 9 (for all microservices and API Gateway)
* **Primary Programming Language:** C#
* **Databases:**
  * **MongoDB (NoSQL Document Store):** Used by `ListingService` and `OrderService`. Accessed via Entity Framework Core (MongoDB Provider).
  * **PostgreSQL (Relational Database):** Used by `UserService`. Accessed via Entity Framework Core (Npgsql Provider).
* **Caching:** Redis
* **Object Storage:** MinIO (S3-compatible)
* **API Gateway:** YARP (Yet Another Reverse Proxy)
* **Messaging:** RabbitMQ (using MassTransit as an abstraction layer)
* **Containerization:** Docker & Docker Compose
* **API Documentation:** Swagger/OpenAPI (Swashbuckle)
* **Observability (Foundational Setup):** Serilog (for structured logging), OpenTelemetry (planned for comprehensive distributed tracing and metrics collection)
* **Architecture Principles:** Microservices, Clean Architecture (within each service), CQRS (within each service), Event-Driven Architecture, Polyglot Persistence, Saga Pattern (future).
* **Security:** JWT-based Authentication, CORS, Password Hashing (`Microsoft.AspNetCore.Cryptography.KeyDerivation`).

## ✨ Services & Features

### Current Core Services (Implemented/In Progress):

* **`SynopsisSI.Gateway`:**
  * **Role:** Single entry point for all client requests.
  * **Features:** Routes requests to appropriate downstream microservices (`ListingService`, `OrderService`, `UserService`) using YARP. Configured for path-based routing and health checks of backend services. Future: SSL termination, rate limiting, enhanced security policies.
* **`ListingService`:**
  * **Role:** Manages product listings.
  * **Features:** Full CRUD operations (Create, Read, Update, Delete/Delist), search and filtering capabilities, management of listing-specific attributes, image URL construction (from MinIO keys), optimistic concurrency control. Consumes `OrderPlacedEvent` to update listing status.
  * **Database:** MongoDB.
* **`OrderService`:**
  * **Role:** Handles order creation and processing.
  * **Features:** Placing new orders (transactional within its boundary), storing order details and line items. Publishes `OrderPlacedEvent` upon successful order creation.
  * **Database:** MongoDB.
* **`UserService`:**
  * **Role:** Manages user identity, profiles, and authentication.
  * **Features:** User registration (with password hashing), user login, JWT generation. Publishes `UserRegisteredEvent`.
  * **Database:** PostgreSQL.

### Planned Future Services & Features (for "Distributed Large Systems" & further development):

This section outlines services and features that demonstrate the scalability and extensibility of the `SynopsisSI` architecture. While not fully implemented within the initial project iteration due to time constraints, they represent logical next steps and serve as practical examples for advanced system design concepts.

* **Dedicated `AuthService`:** To centralize and enhance authentication (OAuth 2.0/OpenID Connect, social logins, MFA) and authorization policy management, further decoupling these concerns from the `UserService`.
* **`PaymentService`:** Integrate with payment gateways (e.g., Stripe, PayPal) to handle financial transactions. This service would be a critical participant in the order placement Saga, demonstrating robust distributed transaction management and compensation logic.
* **`ReviewService`:** Manage user-submitted reviews and ratings for sellers and/or listings. Consume events like `OrderCompletedEvent` to enable review submissions and publish `ReviewSubmittedEvent`.
* **`NotificationService`:** Handle all asynchronous user communications (email, SMS, push notifications). React to various system events (`UserRegisteredEvent`, `OrderPlacedEvent`, `OrderShippedEvent`, etc.) from the message bus.
* **`SearchService` (Advanced):** Provide advanced full-text search, faceted search, and potentially recommendation capabilities for listings using a dedicated search engine like Elasticsearch or OpenSearch. Data would be synchronized from `ListingService` via events.
* **Comprehensive Observability Suite:** Full implementation of Distributed Tracing with OpenTelemetry and Jaeger/Zipkin, advanced Metrics Collection with OpenTelemetry and Prometheus/Grafana, and Centralized Structured Logging with Serilog aggregated into a system like ELK stack or Seq.
* **Advanced Resilience Patterns:** Systematic implementation of patterns like Circuit Breakers, Retries with exponential backoff, Bulkheads, and Fallbacks (e.g., using Polly) for all inter-service HTTP calls and message consumers.
* **Service Discovery & Dynamic Configuration:** Integration of a dynamic service discovery mechanism (e.g., Consul, Kubernetes DNS) and dynamic updates to API Gateway (YARP) routing. Centralized configuration management using tools like HashiCorp Consul or Kubernetes ConfigMaps, and HashiCorp Vault for all sensitive secrets.

## 🏗 Project Structure

The solution is organized to support multiple independent microservices, shared libraries, and an API Gateway, with project folders located directly under a `src` directory at the solution root:

```text
SynopsisSI/
├── src/
│   ├── Gateways/
│   │   └── SynopsisSI.Gateway/              # YARP API Gateway Project
│   │       └── SynopsisSI.Gateway.csproj
│   │       └── Dockerfile
│   │       └── appsettings.json
│   │       └── Program.cs
│   │
│   ├── Services/                              # Contains all individual microservice projects
│   │   ├── ListingService/                  # Autonomous Listing Microservice
│   │   │   ├── ListingService.API/            # API Layer (ASP.NET Core Web API)
│   │   │   │   └── SynopsisSI.Services.ListingService.API.csproj
│   │   │   │   └── Dockerfile
│   │   │   │   └── Program.cs
│   │   │   │   └── Controllers/
│   │   │   │       └── ListingsController.cs
│   │   │   │   └── appsettings.json
│   │   │   ├── ListingService.Application/    # Application Logic, CQRS, Interfaces
│   │   │   │   └── SynopsisSI.Services.ListingService.Application.csproj
│   │   │   │   └── Features/
│   │   │   │   └── Interfaces/
│   │   │   ├── ListingService.Domain/       # Domain Entities & Logic
│   │   │   │   └── SynopsisSI.Services.ListingService.Domain.csproj
│   │   │   │   └── Entities/
│   │   │   │   └── ValueObjects/
│   │   │   └── ListingService.Infrastructure/ # Data Persistence, External Service Clients
│   │   │       └── SynopsisSI.Services.ListingService.Infrastructure.csproj
│   │   │       └── Persistence/
│   │   │       └── Services/
│   │   │
│   │   ├── OrderService/                    # Autonomous Order Microservice
│   │   │   ├── OrderService.API/
│   │   │   │   └── SynopsisSI.Services.OrderService.API.csproj
│   │   │   │   └── Dockerfile
│   │   │   │   └── Program.cs
│   │   │   │   └── Controllers/
│   │   │   │       └── OrdersController.cs
│   │   │   │   └── appsettings.json
│   │   │   ├── OrderService.Application/    # (Similar internal structure as ListingService.Application)
│   │   │   ├── OrderService.Domain/         # (Similar internal structure)
│   │   │   └── OrderService.Infrastructure/ # (Similar internal structure)
│   │   │
│   │   └── UserService/                     # Autonomous User Microservice
│   │       ├── UserService.API/
│   │       │   └── SynopsisSI.Services.UserService.API.csproj
│   │       │   └── Dockerfile
│   │       │   └── Program.cs
│   │       │   └── Controllers/
│   │       │       ├── UsersController.cs
│   │       │       └── AuthController.cs
│   │       │   └── appsettings.json
│   │       ├── UserService.Application/     # (Similar internal structure)
│   │       ├── UserService.Domain/          # (Similar internal structure)
│   │       └── UserService.Infrastructure/  # (Similar internal structure)
│   │
│   └── Shared/                                # Libraries shared across microservices
│       ├── SynopsisSI.Shared.Domain/          # Optional: Truly common domain primitives
│       │   └── SynopsisSI.Shared.Domain.csproj
│       ├── SynopsisSI.Shared.Events/          # DTOs for inter-service event messages
│       │   └── SynopsisSI.Shared.Events.csproj
│       └── SynopsisSI.Shared.Infrastructure/  # Optional: Common infra helpers
│           └── SynopsisSI.Shared.Infrastructure.csproj
│
├── tests/                                     # Test projects, ideally per service
│   ├── ListingService.UnitTests/              # Example
│   ├── OrderService.UnitTests/                # Example
│   ├── UserService.UnitTests/                 # Example
│   └── ...                                    # Integration tests, etc.
│
├── docker-compose.yml                         # Orchestrates all services and backing stores for local development
├── docker-compose.override.yml                # Optional: For local developer-specific overrides
├── SynopsisSI.sln                             # Main solution file
└── README.md                                  # This file

## 💾 Data Management & System Integration Highlights

The `SynopsisSI` platform employs a sophisticated data management and system integration strategy tailored for its microservice architecture:

* **Database per Service & Polyglot Persistence:** Each core microservice (`ListingService` - MongoDB, `OrderService` - MongoDB, `UserService` - PostgreSQL) owns and manages its own dedicated data store. This promotes autonomy and allows for the selection of the most appropriate database technology for each service's specific needs. `ListingService` and `OrderService` utilize **MongoDB** for its flexible schema capabilities, ideal for varied product details and complex order structures. In contrast, `UserService` leverages **PostgreSQL** for its relational strengths in managing structured user identity and credential data. This use of multiple database technologies exemplifies polyglot persistence.
* **CQRS Pattern:** Within each service, the Command Query Responsibility Segregation (CQRS) pattern is applied. This separates operations that change state (Commands, e.g., creating a listing, placing an order) from operations that read state (Queries, e.g., fetching listing details, searching for products). This separation allows for independent optimization of read and write paths and simplifies the models for each responsibility.
* **Event-Driven Architecture & Asynchronous Messaging:** Critical inter-service communication, such as notifying the `ListingService` when an order is placed by the `OrderService`, is handled asynchronously using an event-driven approach. **RabbitMQ** serves as the message broker, with **MassTransit** providing a .NET abstraction layer to simplify event publishing and consumption. This promotes loose coupling, as services don't need direct synchronous dependencies on each other, and enhances system resilience.
* **API Gateway (YARP):** The `SynopsisSI.Gateway` acts as the single, unified entry point for all external client requests. It is responsible for routing requests to the appropriate backend microservices based on path and other criteria. It also provides a centralized location for implementing cross-cutting concerns like initial authentication, SSL termination (conceptually), and rate limiting (planned).
* **Unit of Work Pattern:** Used within each service's infrastructure layer to manage transactions and data persistence operations atomically for its respective database, ensuring data integrity for write operations.
* **Saga Pattern (Conceptual for Future):** While initial eventing is point-to-point for simple workflows (like `OrderPlacedEvent` triggering a listing status update), more complex distributed transactions that span multiple services and require robust compensation logic (e.g., order placement involving payment, inventory, and notification services) would be managed using the Saga pattern. This is a planned area for future development as the system grows.

## 🚀 Scalability Principles Applied

The architecture of `SynopsisSI` is designed with scalability across multiple dimensions, often referred to by the Scale Cube model:

* **X-axis (Horizontal Duplication):** All API-based microservices (`SynopsisSI.Gateway`, `ListingService.API`, `OrderService.API`, `UserService.API`) are designed to be stateless (or manage state externally) and can therefore be scaled horizontally by running multiple container instances behind load balancers. Similarly, backing data stores like MongoDB (via replica sets), PostgreSQL (via read replicas and connection pooling), and Redis (via clustering) also support horizontal scaling.
* **Y-axis (Functional Decomposition):** This is the foundational principle of the microservice architecture itself. `SynopsisSI` is decomposed into distinct services (`ListingService`, `OrderService`, `UserService`, etc.) based on business capabilities or domains. This allows each function to be scaled, developed, and deployed independently. If any single microservice grows too large or complex, it can be further decomposed.
* **Z-axis (Data Partitioning/Sharding):** For services with extremely large datasets or very high write throughput requirements (e.g., potentially `ListingService` or `OrderService` at massive scale), their underlying databases (MongoDB and PostgreSQL) both offer sharding strategies. Sharding partitions data across multiple database servers or clusters, distributing the load and allowing for near-linear scalability of data storage and throughput. Redis Cluster also employs sharding for its distributed cache.

## 🚦 Getting Started

### Prerequisites

* .NET 9.0 SDK or later
* Docker Desktop (with Docker Compose enabled)
* An IDE like JetBrains Rider or Visual Studio 2022+
* Git

### Quick Start (Using Docker Compose - Recommended for Dev)

1.  **Clone the repository.**
2.  **Navigate to the root directory** of the cloned repository (where `docker-compose.yml` is located).
3.  **Ensure your JWT Secret Keys are set:**
  * Open the respective `appsettings.json` and `appsettings.Development.json` files for `UserService.API`, `ListingService.API`, and `OrderService.API` (under `src/Services/...`).
  * In the `JwtSettings:SecretKey` field, replace `"REPLACE_THIS_WITH_A_VERY_STRONG_AND_LONG_SECRET_KEY..."` with a strong, unique secret key (at least 32 bytes long). **Ensure the same secret key, issuer, and audience are used for validation in `ListingService` and `OrderService` as the ones used for signing in `UserService` if they are all validating tokens from `UserService` directly.**
  * For Docker Compose, these JWT settings are also passed as environment variables in the `docker-compose.yml` file for each service, which will override `appsettings.json` values during container runtime. Ensure these environment variables in `docker-compose.yml` are set with your chosen strong secret.
4.  **Start all services in detached mode:**
    ```bash
    docker-compose up --build -d
    ```
  * `--build` ensures images are built if Dockerfiles or source code changed.
  * Wait for all containers (especially `synopsis-postgres`, `synopsis-mongodb`, `synopsis-rabbitmq`, `synopsis-minio`) to report as healthy. You can check their status with `docker-compose ps`.

5.  **Apply EF Core Migrations for `UserService` (PostgreSQL):**
  * Once the `synopsis-postgres` container is running and healthy:
  * Open a new terminal and navigate to the `UserService.Infrastructure` project directory:
      ```bash
      cd src/Services/UserService/UserService.Infrastructure
      ```
  * Run the database update command. This command targets the PostgreSQL instance defined in your `UserService.API/appsettings.Development.json` connection string (which should point to `localhost:5432` if your Docker Compose setup maps that port for PostgreSQL and you intend to run migrations from your host against the containerized DB).
      ```bash
      dotnet ef database update -s ../UserService.API/ -c UserServiceDbContext -- --environment Development
      ```
    * `-s ../UserService.API/`: Specifies the startup project for configuration.
    * `-c UserServiceDbContext`: Specifies the DbContext.
    * `-- --environment Development`: Ensures development settings are used.
      This will create the necessary tables (like `identity.Users`) in your `synopsis_user_db` PostgreSQL database.

6.  **Verify MinIO Bucket (`ecommerce-bucket`):**
  * The `docker-compose.yml` for the `synopsis-minio` service includes:
      ```yaml
      environment:
        MINIO_DEFAULT_BUCKETS: ecommerce-bucket
      ```
  * This configuration instructs MinIO to **automatically create the `ecommerce-bucket`** when the MinIO container starts for the first time with an empty data volume.
  * You can verify this by navigating to the MinIO Console: `http://localhost:9001` (Login with `minioadmin` / `minio_password` as configured in `docker-compose.yml`) and checking the "Buckets" section.
  * If for some reason the bucket was not created (e.g., if you started MinIO with an existing data volume that didn't have this setting active initially), you can easily create it manually via the MinIO console:
    * Click "Buckets" on the left sidebar.
    * Click the "+ Create Bucket" button.
    * Enter `ecommerce-bucket` as the bucket name and click "Create Bucket".

### Access Points (Default Local Development via Docker Compose)

* **API Gateway:** `http://localhost:8080` (All API requests should go through here)
* **Swagger UI (Per Service - for Development/Debugging):**
  * To access individual service Swagger UIs, you can temporarily map their internal port 80 to a unique host port in `docker-compose.yml`. For example, for `listingservice.api`:
      ```yaml
      # In docker-compose.yml, under listingservice.api:
      # ports:
      #  - "5010:80" # Host:Container
      ```
    Then access `http://localhost:5010/swagger`. Remember to remove direct port mappings for microservices later if all external traffic should strictly go via the gateway.
* **RabbitMQ Management UI:** `http://localhost:15672` (Login: `user` / `password` as configured in `docker-compose.yml`)
* **MinIO Console:** `http://localhost:9001` (Login: `minioadmin` / `minio_password`)
* **pgAdmin (PostgreSQL UI):** `http://localhost:5050` (Login: e.g., `admin@synopsissi.com` / `adminpassword`. Register `synopsis-postgres` server: host `synopsis-postgres` (service name), port `5432`, user `useradmin`, pass `userpassword`, db `synopsis_user_db`.)
* **mongo-express (MongoDB UI):** `http://localhost:8081` (Login: e.g., `mexpressadmin` / `mexpresspassword`. Connect to `synopsis-mongodb` using `mongoadmin`/`mongopassword`.)

## 🐳 Essential Docker Compose Commands

(Located in the root directory of the `SynopsisSI` project)

* **View status of running services:**
    ```bash
    docker-compose ps
    ```
* **View logs for all services (and follow new logs):**
    ```bash
    docker-compose logs -f
    ```
* **View logs for a specific service:**
    ```bash
    docker-compose logs -f <service_name_in_compose_file>
    # Example: docker-compose logs -f listingservice.api
    ```
* **Stop all services:**
    ```bash
    docker-compose down
    ```
* **Stop services and remove named volumes (deletes all data associated with this compose project):**
    ```bash
    docker-compose down -v
    ```
* **Build (or rebuild) images for all services defined in `docker-compose.yml`:**
    ```bash
    docker-compose build
    ```
* **Rebuild a specific service and then start all services (detached mode), ensuring dependencies are met:**
    ```bash
    docker-compose build <service_name_in_compose_file>
    docker-compose up -d --no-deps <service_name_in_compose_file>
    # Or more simply, to rebuild all necessary and restart everything:
    # docker-compose up --build -d
    ```
* **Restart services:**
    ```bash
    docker-compose restart
    # Or for a specific service:
    # docker-compose restart <service_name_in_compose_file>
    ```

## 📖 API Documentation (via Swagger UI)

API documentation for each individual microservice is available via its Swagger UI. During development, to access a specific service's Swagger UI directly, you can temporarily map its internal port (usually 80) to a unique port on your host machine within the `docker-compose.yml` file (e.g., map `5010:80` for `listingservice.api` and then navigate to `http://localhost:5010/swagger`).

All client-facing API interactions should occur through the **API Gateway**, which is accessible at `http://localhost:8080`. The Gateway routes requests to the appropriate backend services based on path prefixes defined in its configuration (e.g., `/api/listings/*` routes to `ListingService`, `/api/orders/*` routes to `OrderService`, `/api/users/*` and `/api/auth/*` route to `UserService`).

### Key Endpoints (Accessed via API Gateway: `http://localhost:8080`)

#### Listings (`/api/listings`)

* **`POST /api/listings`**: Creates a new listing. (Requires Authentication: Bearer Token)
  * **Request Body Example:**
      ```json
      {
        "sellerId": "user-id-from-token-or-claims",
        "title": "Vintage Mechanical Keyboard",
        "description": "Classic clicky keyboard, fully restored.",
        "category": "Computer Peripherals",
        "price": 120.00,
        "currency": "USD",
        "condition": "Refurbished",
        "itemSpecifics": { "SwitchType": "Cherry MX Blue", "Layout": "ANSI" },
        "imageObjectKeys": ["keyboards/vintage_mech_01.jpg"],
        "locationLongitude": -122.084,
        "locationLatitude": 37.422,
        "tags": ["mechanical", "keyboard", "vintage", "retro"]
      }
      ```
* **`GET /api/listings/{id}`**: Retrieves a specific listing by its ID.
* **`GET /api/listings`**: Searches and filters listings. Supports query parameters such as `keyword`, `category`, `minPrice`, `maxPrice`, `condition`, `pageNumber`, `pageSize`, and `sortBy`.
* **`PUT /api/listings/{id}`**: Updates an existing listing. (Requires Authentication: Bearer Token; user must be the seller).
* **`DELETE /api/listings/{id}`**: Delists (soft delete) an item. (Requires Authentication: Bearer Token; user must be the seller).

#### Orders (`/api/orders`)

* **`POST /api/orders`**: Places a new order. (Requires Authentication: Bearer Token)
  * **Request Body Example:**
      ```json
      {
        // "buyerId": "user-id-from-token", // Controller will extract this from claims
        "items": [
          {
            "listingId": "valid-listing-id",
            "productTitleSnapshot": "Vintage Mechanical Keyboard",
            "priceAtPurchase": 120.00,
            "quantity": 1
          }
        ],
        "shippingAddress": {
          "street": "1600 Amphitheatre Parkway", "city": "Mountain View",
          "postalCode": "94043", "country": "USA"
        },
        "currency": "USD"
      }
      ```
* **`GET /api/orders/{id}`**: Retrieves order details by its ID. (Requires Authentication: Bearer Token; user must be the buyer or an involved party like the seller, or admin). *(Further implementation needed for query handler)*

#### Users & Authentication (`/api/users`, `/api/auth`)

* **`POST /api/users/register`**: Registers a new user.
  * **Request Body Example:**
      ```json
      { "username": "johndoe", "email": "john.doe@example.com", "password": "Password123!" }
      ```
* **`POST /api/auth/login`**: Logs in an existing user and returns a JWT.
  * **Request Body Example:**
      ```json
      { "email": "john.doe@example.com", "password": "Password123!" }
      ```
  * **Successful Response Body Example:**
      ```json
      {
        "isSuccess": true, "message": "Login successful",
        "userId": "generated-user-id", "username": "johndoe", "email": "john.doe@example.com",
        "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJnZW5lcmF0ZWQtdXNlci1pZCIsImVtYWlsIjoiam9obi5kb2VAZXhhbXBsZS5jb20iLCJuYW1lIjoiam9obmRvZSIsImp0aSI6ImE4YjJj...",
        "tokenExpiration": "YYYY-MM-DDTHH:MM:SSZ"
      }
      ```
* **`GET /api/users/{id}`**: Retrieves user profile information. (Requires Authentication: Bearer Token; specific user or admin). *(Further implementation needed for query handler)*

#### Reviews (`/api/reviews` - Planned)

* **`POST /api/reviews`**: Creates a review for a seller/listing associated with a completed order.

### Health Checks

The API Gateway is configured to monitor the health of downstream services. Individual services also expose their own health check endpoints:

* **API Gateway (General Status):** `http://localhost:8080/` (or a dedicated `/health` endpoint if added to the Gateway itself).
* **ListingService Health:** The Gateway checks `/health/listings` on `listingservice.api`.
* **OrderService Health:** The Gateway checks `/health/orders` on `orderservice.api`.
* **UserService Health:** The Gateway checks `/health/users` on `userservice.api`.

### Authentication

* Most write operations and endpoints that access user-specific data require a **JWT Bearer token**.
* A token is obtained by a client application after a successful login via the `/api/auth/login` endpoint (routed through the API Gateway to the `UserService`).
* The obtained token must be included in the `Authorization` header of subsequent requests to protected resources:
  `Authorization: Bearer {your_jwt_access_token}`
* Services like `ListingService` and `OrderService` are configured to validate these JWTs (checking signature, issuer, audience, and expiry) against the same settings used by `UserService` to issue them.

### Standard HTTP Response Codes

* **`200 OK`**: The request was successful. Data is typically returned in the response body for `GET` requests.
* **`201 Created`**: The request was successful, and a new resource was created. The `Location` header often points to the new resource, and the response body may contain the created resource or its ID.
* **`204 No Content`**: The request was successful, but there is no content to return (e.g., after a successful `DELETE` or a `PUT` that doesn't return the updated entity).
* **`400 Bad Request`**: The request was invalid due to a client-side error (e.g., missing required fields, malformed JSON, validation errors). The response body typically contains details about the specific error(s) in a problem details format.
* **`401 Unauthorized`**: Authentication is required and has failed, or has not yet been provided. The client should obtain valid credentials (e.g., log in to get a new token) and retry.
* **`403 Forbidden`**: The authenticated user does not have the necessary permissions to access the requested resource, even if their authentication token is valid.
* **`404 Not Found`**: The requested resource (e.g., a specific listing or order) could not be found on the server.
* **`409 Conflict`**: The request could not be completed due to a conflict with the current state of the resource. This can occur, for example, during an optimistic concurrency check if an item was updated by another process since it was last read.
* **`500 Internal Server Error`**: An unexpected error occurred on the server side. The client should not typically retry the exact same request without modification, as it indicates a server-side issue.

### General API Conventions

* **JSON for Request/Response Bodies:** All request and response bodies primarily use the `application/json` content type.
* **Error Responses:** Errors (4xx and 5xx status codes) should return a consistent JSON problem details response, as per RFC 7807, which ASP.NET Core facilitates. This typically includes `type`, `title`, `status`, `detail`, and `traceId`. For validation errors (400), an `errors` object detailing field-specific issues is common.
  *Example Validation Error (400):*
    ```json
    {
      "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
      "title": "One or more validation errors occurred.",
      "status": 400,
      "traceId": "00-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx-xxxxxxxxxxxxxxxx-00",
      "errors": {
        "Title": ["The Title field is required."],
        "Price": ["The field Price must be between 0.01 and 1000000.00."]
      }
    }
    ```
* **Pagination (for list endpoints like `GET /api/listings`):**
  * **Query Parameters:** Standardized parameters like `pageNumber` (default: 1) and `pageSize` (default: 10, with a configurable maximum like 100) are used.
  * **Response Structure (Example for `PagedListingsResultDto`):**
      ```json
      {
        "items": [ /* array of DTOs, e.g., ListingItemDto */ ],
        "pageNumber": 1,
        "pageSize": 10,
        "totalCount": 150,
        "totalPages": 15
      }
      ```
* **Sorting (for list endpoints):**
  * A `sortBy` query parameter can be used (e.g., `sortBy=price_asc`, `sortBy=createdAt_desc`) to specify the field and direction for sorting results. The backend handlers parse this to apply appropriate ordering to database queries.
* **Idempotency:** For critical write operations (Commands), especially those that might be retried by clients or message consumers, services should strive for idempotency where appropriate (e.g., using an idempotency key or checking if an operation was already performed).

