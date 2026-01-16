# High-Level Design (HLD): FoodHub

## 1. Architecture Overview

FoodHub employs a **Modular Monolith** architecture using **Clean Architecture** principles. This design provides a robust foundation for the current application while strategically preparing for a future evolution into a microservices-based system.

The core tenets are:
- **Modularity**: The system is partitioned into vertical business capabilities (e.g., `FoodHub.Restaurant`). Each module is self-contained, encompassing its own domain, application, and infrastructure logic.
- **Strict Separation of Concerns**: Each layer has a single, well-defined responsibility, enforcing a one-way dependency flow towards the core business logic (Domain).
- **Dependency Inversion**: Abstractions (interfaces) defined in the Application layer are implemented by the Infrastructure layer. This decouples business logic from concrete external concerns like databases or frameworks.

This approach yields high cohesion within modules and low coupling between them, making the system easier to maintain, test, and eventually decompose.

## 2. Component Diagram

The diagram below illustrates the layers and their dependencies within a single business module (`Restaurant`). All dependencies point inwards.

```ascii
+---------------------------------------------------------------------------------+
|                                     CLIENT                                      |
| (Web Browser, Mobile App, etc. via GraphQL)                                     |
+---------------------------------------------------------------------------------+
          |
          v
+---------------------------------------------------------------------------------+
| FoodHub.Api (Presentation Layer)                                                |
|---------------------------------------------------------------------------------|
| - GraphQL Endpoint (/graphql)                                                   |
| - Queries & Mutations (API Contracts)                                           |
| - Logging & Correlation ID Middleware                                           |
| - **NO BUSINESS LOGIC**                                                         |
|                                                                                 |
|   Dependencies: -> FoodHub.Restaurant.Application                             |
+---------------------------------------------------------------------------------+
          |
          v
+---------------------------------------------------------------------------------+
| FoodHub.Restaurant.Application (Application Layer)                              |
|---------------------------------------------------------------------------------|
| - Use Cases (Commands & Queries)                                                |
| - DTOs (Data Transfer Objects)                                                  |
| - Repository Interfaces (e.g., IRestaurantRepository)                           |
| - Orchestrates data flow between API and Infrastructure                         |
|                                                                                 |
|   Dependencies: -> FoodHub.Restaurant.Domain                                  |
+---------------------------------------------------------------------------------+
          |
          v
+---------------------------------------------------------------------------------+
| FoodHub.Restaurant.Domain (Domain Layer)                                        |
|---------------------------------------------------------------------------------|
| - Entities (e.g., Restaurant) & Aggregates                                      |
| - Value Objects (e.g., RestaurantName)                                          |
| - Core business rules and logic                                                 |
| - **Completely pure, no external dependencies**                                 |
+---------------------------------------------------------------------------------+
     ^
     | (Domain Entities returned)
     |
+---------------------------------------------------------------------------------+
| FoodHub.Restaurant.Infrastructure (Infrastructure Layer)                        |
|---------------------------------------------------------------------------------|
| - Implements IRestaurantRepository                                              |
| - Cosmos DB Client Logic & Queries                                              |
| - Document Models (e.g., RestaurantDocument)                                    |
| - Mapping: Domain Entity <--> Document Model                                    |
|                                                                                 |
|   Dependencies: -> FoodHub.Restaurant.Domain                                  |
|   External Systems: -> [Azure Cosmos DB]                                        |
+---------------------------------------------------------------------------------+
```

## 3. Layer Responsibilities

- **Presentation (FoodHub.Api)**: The system's entry point. Its sole responsibility is to handle HTTP requests, deserialize them into simple commands or queries, and pass them to the Application layer. It is completely unaware of business logic.
- **Application (FoodHub.Restaurant.Application)**: The orchestrator. It contains the application's use cases (e.g., `CreateRestaurantCommand`). It knows *what* to do but not *how*. It defines the abstractions (interfaces) needed to achieve its goals and maps domain entities to DTOs for the presentation layer.
- **Domain (FoodHub.Restaurant.Domain)**: The heart of the business. It contains the core business logic, entities, and rules. It is completely isolated and has no dependencies on any external framework or database, ensuring its purity and longevity.
- **Infrastructure (FoodHub.Restaurant.Infrastructure)**: The implementation layer. It provides concrete implementations for the interfaces defined in the Application layer, handling all interactions with external systems like Azure Cosmos DB. It is responsible for data persistence and retrieval, translating between domain entities and persistence models.

## 4. Execution Flow (Example: `createRestaurant` Mutation)

1.  **Client**: Sends a GraphQL mutation to the `/graphql` endpoint.
2.  **API Layer (`FoodHub.Api`)**:
    - The Correlation ID middleware intercepts the request and injects a `CorrelationId` into the `LogContext`.
    - Hot Chocolate maps the mutation to the `RestaurantMutation` class.
    - The `CreateRestaurant` method is invoked, receiving a `CreateRestaurantDto`.
    - It logs the request entry: `Begin: CreateRestaurant mutation...`.
    - It calls the Application layer's `CreateRestaurantCommand`.
3.  **Application Layer (`FoodHub.Restaurant.Application`)**:
    - `CreateRestaurantCommand.ExecuteAsync` is called with the DTO.
    - It validates the request and creates a `Restaurant` domain entity.
    - It calls the `IRestaurantRepository.AddAsync()` method, passing the domain entity.
4.  **Infrastructure Layer (`FoodHub.Restaurant.Infrastructure`)**:
    - The `RestaurantRepository.AddAsync()` implementation is executed.
    - It logs the database call intention.
    - It maps the `Restaurant` domain entity to a `RestaurantDocument` persistence model.
    - It uses the Cosmos DB client to save the `RestaurantDocument`.
5.  **Return Flow**: The `Id` of the newly created document is returned up the stack.
6.  **API Layer (`FoodHub.Api`)**:
    - Receives the `Id` from the Application layer.
    - Logs the successful completion: `Success: Created restaurant...`.
    - Serializes the result and sends the GraphQL response to the client.

## 5. Data Flow & Mapping

Data integrity and domain isolation are maintained by transforming data models at layer boundaries. **Domain entities are never exposed outside the module.**

- **API -> Application**: `CreateRestaurantDto` (Plain C# Object)
- **Application -> Domain**: A new `Restaurant` (Domain Entity) is instantiated from DTO data.
- **Application -> Infrastructure**: `Restaurant` (Domain Entity) is passed to the repository interface.
- **Inside Infrastructure**: `Restaurant` is mapped to `RestaurantDocument` for persistence.
- **Infrastructure -> Application**: `Restaurant` (Domain Entity) is returned from the repository.
- **Application -> API**: `Restaurant` (Domain Entity) is mapped to `RestaurantDto` before being returned.

## 6. Logging Strategy

Logging is treated as a critical cross-cutting concern, configured for diagnostics and observability.

- **Provider**: Serilog, configured centrally in `FoodHub.Api`.
- **Consumption**: Standard `ILogger<T>` is used via dependency injection in all layers.
- **Correlation**: A middleware in `FoodHub.Api` generates a unique `CorrelationId` for every HTTP request. This ID is pushed to Serilog's `LogContext` and is automatically attached to every log entry generated during that request's lifecycle, enabling distributed tracing across layers.
- **Boundaries**: Logging is intentionally limited to the boundaries of each layer to reduce noise and focus on interactions:
    - **API**: Logs the start and end of a request/mutation.
    - **Application**: Logs the invocation of a use case.
    - **Infrastructure**: Logs calls to external systems (e.g., "Calling Cosmos DB to retrieve document...").

## 7. Cosmos DB Design

The database design prioritizes cost-effectiveness and alignment with Domain-Driven Design (DDD) principles.

- **Account/Database**: A single Cosmos DB account and a single database are used to simplify management and reduce overhead.
- **Containers**: Each Aggregate Root from the domain (e.g., `Restaurant`) gets its own container. This provides a good balance of isolation and query performance without the cost of a database per module.
- **Partition Keys**: The partition key for each container is chosen carefully to ensure even distribution of reads and writes (e.g., `/city` for the `Restaurants` container).
- **Document Models**: The domain entities are **not** persisted directly. Instead, each container has a corresponding document model (e.g., `RestaurantDocument`) in the Infrastructure project. This model includes persistence-specific properties like `id`, `_etag`, and the partition key property.
- **Repositories**: The Infrastructure layer contains repositories that handle the mapping between domain entities and these document models, completely hiding the persistence mechanism from the rest of the application.

## 8. Microservice Readiness

The Modular Monolith design is a strategic choice that enables a straightforward, incremental transition to microservices.

- **High Cohesion**: All logic related to a business capability (e.g., "Restaurant Management") is located within a single module (`FoodHub.Restaurant`).
- **Low Coupling**: Modules communicate only through the well-defined interfaces and DTOs exposed by their Application layers. There is no direct dependency between modules at the Infrastructure or Domain level.
- **Decomposition Path**: To extract the `Restaurant` module into a microservice:
    1.  Create a new microservice solution.
    2.  Move the `FoodHub.Restaurant.Domain`, `FoodHub.Restaurant.Application`, and `FoodHub.Restaurant.Infrastructure` projects into it.
    3.  Add a new, lightweight API project (e.g., using REST or keeping GraphQL) to the new solution that replaces `FoodHub.Api`'s role for that specific module.
    4.  Update the original monolith's API to call the new microservice via an HTTP client instead of direct method invocation.

This process requires minimal to no changes to the core business and domain logic, which is the primary benefit of this architectural approach.
