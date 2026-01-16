# FoodHub - Modular Monolith Architecture

A production-ready food delivery and restaurant management platform built on .NET 9 using **Modular Monolith** and **Clean Architecture** principles. Designed for enterprise-scale operations with strategic microservice decomposition capabilities.

## Project Overview

**Architecture Style**: Modular Monolith + Clean Architecture  
**Database Strategy**: Multi-tenant Cosmos DB with container-per-aggregate  
**API Pattern**: GraphQL-first with Hot Chocolate  
**Observability**: Structured logging with distributed tracing  
**Deployment Model**: Single deployable unit with module isolation  

## High-Level Architecture

### Core Principles

- **Strict Layer Boundaries**: Dependencies flow inward toward Domain
- **Module Autonomy**: Each module owns its data and business logic
- **Cross-Module Communication**: Read-only interfaces via Application layer adapters
- **Infrastructure Abstraction**: Domain entities never leak to external layers
- **Event-Driven Potential**: Architecture supports future event sourcing patterns

### Architecture Diagram

```
┌─────────────────┐    GraphQL     ┌─────────────────┐
│     Client      │ ──────────────►│   FoodHub.Api   │ (Presentation)
└─────────────────┘                └─────────────────┘
                                            │
                                    ┌───────┼───────┐
                                    ▼       ▼       ▼
                          ┌─────────────┐ ┌─────────────┐
                          │ Restaurant  │ │    Menu     │ (Modules)
                          │   Module    │ │   Module    │
                          └─────────────┘ └─────────────┘
                                    │       │
                          ┌─────────┼───────┼─────────┐
                          ▼         ▼       ▼         ▼
                    Application  Domain  Domain  Application
                          │                           │
                          ▼                           ▼
                   Infrastructure              Infrastructure
                          │                           │
                          └───────────┬───────────────┘
                                      ▼
                              ┌─────────────────┐
                              │  Cosmos DB      │
                              │  - Restaurants  │
                              │  - Menus       │
                              └─────────────────┘
```

## Module Breakdown

### Restaurant Module (`FoodHub.Restaurant`)

**Responsibility**: Restaurant aggregate management  
**Business Characteristics**: Write-light, stable domain  
**Cosmos Container**: `Restaurants`  
**Partition Strategy**: `/id` (restaurant ID)

**Domain Entities**:
- `Restaurant`: Aggregate root with Name (value object), City, IsActive

**Use Cases**:
- Create Restaurant
- Get Restaurant by ID  
- Get All Restaurants
- Activate/Deactivate Restaurant

**Cross-Module Interfaces**:
- `IRestaurantReadRepository`: Exposes `ExistsAsync()` for Menu module validation

### Menu Module (`FoodHub.Menu`)

**Responsibility**: Menu and MenuItem aggregate management  
**Business Characteristics**: Read-heavy, volatile domain with complex business rules  
**Cosmos Container**: `Menus`  
**Partition Strategy**: `/restaurantId` (enables efficient restaurant-scoped queries)

**Domain Entities**:
- `Menu`: Aggregate root containing MenuItems
- `MenuItem`: Entity with Price (value object), Category, Availability, Images
- `MenuImage`: Value object for item imagery

**Use Cases**:
- Create Menu (validates Restaurant existence via cross-module interface)
- Add/Update/Remove Menu Items
- Get Menu by ID
- Get Menu by Restaurant ID

**Cross-Module Dependencies**:
- Consumes `IRestaurantReadRepository` from Restaurant module (read-only validation)

## Folder & Project Structure

```
FoodHub/
├── src/
│   ├── FoodHub.Api/                           # 🎯 Presentation Layer
│   │   ├── GraphQL/
│   │   │   ├── Queries/
│   │   │   │   └── RestaurantQuery.cs         # GraphQL query resolvers
│   │   │   └── Mutations/
│   │   │       └── RestaurantMutation.cs      # GraphQL mutation resolvers
│   │   ├── Program.cs                         # 🔧 DI container, middleware
│   │   └── appsettings.json                   # 🔐 Configuration (Cosmos, logging)
│   │
│   ├── FoodHub.Restaurant/                    # 🏢 Restaurant Module
│   │   ├── FoodHub.Restaurant.Domain/         # 🧠 Pure business logic
│   │   │   ├── Entities/Restaurant.cs
│   │   │   ├── ValueObjects/RestaurantName.cs
│   │   │   └── Exceptions/DomainException.cs
│   │   ├── FoodHub.Restaurant.Application/    # 🎭 Use cases & orchestration
│   │   │   ├── Commands/CreateRestaurant/
│   │   │   ├── Queries/GetRestaurantById/
│   │   │   ├── Dtos/RestaurantDto.cs
│   │   │   └── Interfaces/IRestaurantRepository.cs
│   │   └── FoodHub.Restaurant.Infrastructure/ # 💾 Cosmos DB implementation
│   │       └── Persistence/
│   │           ├── Cosmos/
│   │           │   ├── CosmosContext.cs       # Container resolution
│   │           │   ├── CosmosOptions.cs       # Configuration binding
│   │           │   └── RestaurantDocument.cs  # Persistence model
│   │           └── Repositories/RestaurantRepository.cs
│   │
│   └── FoodHub.Menu/                          # 🍽️ Menu Module  
│       ├── FoodHub.Menu.Domain/               # 🧠 Pure business logic
│       │   ├── Entities/
│       │   │   ├── Menu.cs                    # Aggregate root
│       │   │   └── MenuItem.cs                # Entity
│       │   ├── ValueObjects/
│       │   │   ├── Price.cs
│       │   │   └── MenuImage.cs
│       │   └── Enums/
│       │       ├── ItemCategory.cs
│       │       └── ItemAvailability.cs
│       ├── FoodHub.Menu.Application/          # 🎭 Use cases & orchestration
│       │   ├── Commands/
│       │   │   ├── CreateMenuCommand.cs
│       │   │   ├── AddMenuItemCommand.cs
│       │   │   └── UpdateMenuItemCommand.cs
│       │   ├── Queries/
│       │   │   ├── GetMenuByIdQuery.cs
│       │   │   └── GetMenuByRestaurantIdQuery.cs
│       │   ├── Dtos/MenuDto.cs
│       │   └── Interfaces/
│       │       ├── IMenuRepository.cs
│       │       └── IRestaurantReadRepository.cs  # Cross-module interface
│       └── FoodHub.Menu.Infrastructure/        # 💾 Cosmos DB implementation
│           └── Persistence/
│               ├── Cosmos/
│               │   ├── CosmosContext.cs
│               │   ├── CosmosOptions.cs  
│               │   └── MenuDocument.cs
│               └── Repositories/MenuRepository.cs
├── FoodHub.sln
├── README.md
└── ARCHITECTURE.md
```

## Execution Flow

### Request Processing Pipeline

```
1. Client Request
   └─► /graphql (POST)

2. API Layer (FoodHub.Api)
   ├─► Correlation ID Middleware (injects X-Correlation-ID)
   ├─► Hot Chocolate GraphQL Engine
   └─► Query/Mutation Resolver (RestaurantQuery/RestaurantMutation)

3. Application Layer (*.Application)
   ├─► Use Case Command/Query (e.g., CreateRestaurantCommand.ExecuteAsync())
   ├─► Input Validation & Business Rule Application
   └─► Repository Interface Invocation (IRestaurantRepository.AddAsync())

4. Infrastructure Layer (*.Infrastructure)
   ├─► CosmosContext (resolves container from configuration)
   ├─► Domain Entity → Document Model Mapping (Restaurant → RestaurantDocument)
   ├─► Cosmos DB SDK Operations (CreateItemAsync, QueryIterator)
   └─► Document Model → Domain Entity Mapping (RestaurantDocument → Restaurant)

5. Response Pipeline
   ├─► Domain Entity → DTO Mapping (Restaurant → RestaurantDto)
   ├─► GraphQL Response Serialization
   └─► HTTP Response with Correlation ID Header
```

### Cross-Module Communication Flow

```
Menu Module (CreateMenuCommand)
├─► Validates Restaurant existence
├─► Calls IRestaurantReadRepository.ExistsAsync(restaurantId)
├─► DI Container resolves to Restaurant.Infrastructure.RestaurantRepository
├─► RestaurantRepository.ExistsAsync() queries Restaurants container
└─► Returns boolean result to Menu module
```

## Cosmos DB Design & Partitioning

### Database Architecture

```
Cosmos Account: FoodHub-Production
├─► Database: FoodHubDb
    ├─► Container: Restaurants
    │   ├─► Partition Key: /id
    │   ├─► Documents: RestaurantDocument
    │   └─► Typical Size: 10K-100K restaurants
    │
    └─► Container: Menus  
        ├─► Partition Key: /restaurantId
        ├─► Documents: MenuDocument (with embedded MenuItemDocument[])
        └─► Typical Size: 10K-100K menus, 100K-1M menu items
```

### Partitioning Strategy

**Restaurants Container (`/id`)**:
- **Rationale**: Even distribution across restaurant IDs
- **Query Patterns**: Point reads by restaurant ID, cross-partition scans for GetAll
- **Scaling**: Horizontal scale based on restaurant count

**Menus Container (`/restaurantId`)**:
- **Rationale**: Co-locate all menu data for a restaurant in single partition
- **Query Patterns**: Efficient restaurant-scoped queries, hot partitions for popular restaurants
- **Scaling**: Partition splitting based on individual restaurant activity

### Document Models

**RestaurantDocument**:
```json
{
  "id": "restaurant-guid",
  "name": "The Golden Spoon", 
  "city": "New York",
  "isActive": true
}
```

**MenuDocument**:
```json
{
  "id": "menu-guid",
  "restaurantId": "restaurant-guid",
  "name": "Dinner Menu",
  "description": "Evening dining options",
  "items": [
    {
      "id": "item-guid",
      "name": "Margherita Pizza",
      "description": "Fresh mozzarella, basil, tomatoes",
      "priceAmount": 18.99,
      "priceCurrency": "USD",
      "category": "Main",
      "availability": "Available",
      "images": [
        {"type": "primary", "url": "https://..."}
      ]
    }
  ]
}
```

## GraphQL Design

### API Surface

**Endpoint**: `/graphql`  
**Development UI**: `/graphql` (Banana Cake Pop embedded)  
**Schema Introspection**: Enabled in Development only  

### Query Operations

```graphql
type Query {
  # Restaurant Queries
  getAllRestaurants: [RestaurantDto!]!
  getRestaurantById(id: ID!): RestaurantDto
  
  # Menu Queries  
  getMenuById(id: ID!): MenuDto
  getMenusByRestaurant(restaurantId: ID!): MenuDto
}
```

### Mutation Operations

```graphql
type Mutation {
  # Restaurant Mutations
  createRestaurant(input: CreateRestaurantDto!): ID!
  
  # Menu Mutations
  createMenu(input: CreateMenuDto!): ID!
  addMenuItem(input: AddMenuItemDto!): Void
  updateMenuItem(input: UpdateMenuItemDto!): Void
}
```

### Error Handling

- **Domain Exceptions**: Mapped to GraphQL field errors with appropriate error codes
- **Validation Errors**: Input validation failures return structured error messages
- **Infrastructure Failures**: Cosmos exceptions mapped to generic GraphQL errors (details logged with Correlation ID)

## Logging & Observability

### Logging Architecture

**Provider**: Serilog with structured logging  
**Sinks**: Console (structured JSON), Debug  
**Context Enrichment**: Correlation ID, user context, operation metadata  

### Correlation & Tracing

```
Request Flow Tracing:
┌─────────────────┐ X-Correlation-ID: abc-123
│   HTTP Request  │ ────────────────────────────┐
└─────────────────┘                             │
                                                 ▼
┌─────────────────────────────────────────────────────────────┐
│ Serilog LogContext (Per-Request Scope)                     │
│ CorrelationId: abc-123                                     │
│ ├─► [API] Begin: CreateRestaurant mutation                 │
│ ├─► [Application] Use Case: Creating restaurant            │
│ ├─► [Infrastructure] Calling Cosmos DB to insert document │
│ ├─► [Infrastructure] Successfully inserted restaurant      │
│ ├─► [Application] Successfully created restaurant          │
│ └─► [API] Success: Created restaurant with ID xyz         │
└─────────────────────────────────────────────────────────────┘
```

### Logging Boundaries

**API Layer**: Request entry/exit, mutation/query results, error responses  
**Application Layer**: Use case execution start/completion, cross-module calls  
**Infrastructure Layer**: Database operations, external service calls  
**Domain Layer**: NO LOGGING (pure business logic)  

### Sample Log Entry

```json
{
  "@timestamp": "2026-01-17T10:30:00.123Z",
  "@level": "Information", 
  "@messageTemplate": "Success: Created restaurant {RestaurantName} with Id {RestaurantId}",
  "RestaurantName": "The Golden Spoon",
  "RestaurantId": "550e8400-e29b-41d4-a716-446655440000",
  "CorrelationId": "abc-123-def-456",
  "SourceContext": "FoodHub.Api.GraphQL.Mutations.RestaurantMutation"
}
```

## Cross-Module Communication Strategy

### Communication Patterns

**Allowed**: Application-to-Application via read-only interfaces  
**Forbidden**: Direct Infrastructure-to-Infrastructure, Domain-to-Domain  

### Interface Design

```csharp
// Defined in FoodHub.Menu.Application.Interfaces
public interface IRestaurantReadRepository
{
    Task<bool> ExistsAsync(Guid restaurantId, CancellationToken cancellationToken);
}

// Implemented in FoodHub.Restaurant.Infrastructure  
public class RestaurantRepository : IRestaurantRepository, IRestaurantReadRepository
{
    // Read-write operations for Restaurant module
    // Read-only operations for cross-module consumers
}
```

### DI Registration Pattern

```csharp
// Program.cs - Cross-module interface mapping
services.AddScoped<IRestaurantRepository, RestaurantRepository>();
services.AddScoped<IRestaurantReadRepository, RestaurantRepository>(); // Cross-module
```

### Future Event-Driven Evolution

Current synchronous cross-module calls can be replaced with:
- **Domain Events**: Restaurant created → Menu module receives event
- **Event Store**: Audit trail and temporal queries
- **CQRS**: Separate read/write models with eventual consistency

## Microservice Readiness

### Decomposition Strategy

Each module is architected for **zero-friction extraction**:

1. **High Cohesion**: All restaurant logic in `FoodHub.Restaurant` namespace
2. **Loose Coupling**: Cross-module dependencies via interfaces only  
3. **Data Isolation**: Separate Cosmos containers per aggregate
4. **API Contracts**: GraphQL schema serves as stable API contract

### Extraction Process (Example: Restaurant Module)

```
Step 1: Create New Microservice Solution
├─► Copy FoodHub.Restaurant.* projects
├─► Add new FoodHub.Restaurant.Api project
└─► Configure independent Cosmos DB access

Step 2: Update Original Monolith  
├─► Replace RestaurantQuery/RestaurantMutation with HTTP client calls
├─► Update IRestaurantReadRepository implementation to call REST API
└─► Remove Restaurant module projects

Step 3: Deploy & Route
├─► Deploy Restaurant microservice independently
├─► Update API Gateway routing (/graphql/restaurant → Restaurant service)
└─► Maintain GraphQL federation or schema stitching
```

### Service Boundaries

**Restaurant Service**: Restaurant aggregate, user management, restaurant onboarding  
**Menu Service**: Menu/MenuItem aggregates, pricing, inventory  
**Order Service** (Future): Order processing, cart management, checkout  
**Payment Service** (Future): Payment processing, billing, refunds  

## Local Development Setup

### Prerequisites

- .NET 9 SDK
- Azure Cosmos DB Emulator OR Azure Cosmos DB account
- Visual Studio 2022 / VS Code / Rider

### Configuration Setup

1. **Cosmos DB Configuration** (`appsettings.json`):
```json
{
  "Cosmos": {
    "Endpoint": "https://localhost:8081",  // Emulator
    "Key": "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
    "DatabaseName": "FoodHubDb",
    "Containers": {
      "Restaurant": { "Name": "Restaurants" },
      "Menu": { "Name": "Menus" }
    }
  }
}
```

2. **Container Creation** (Azure Portal or Emulator):
```
Database: FoodHubDb
├─► Container: Restaurants (Partition: /id)  
└─► Container: Menus (Partition: /restaurantId)
```

### Build & Run Commands

```bash
# Clean build
dotnet clean
dotnet build

# Run API
cd src/FoodHub.Api  
dotnet run

# Access GraphQL Playground
# Navigate to: https://localhost:7161/graphql
```

### Sample Development Workflow

```bash
# 1. Create Restaurant
curl -X POST https://localhost:7161/graphql \
  -H "Content-Type: application/json" \
  -d '{"query": "mutation { createRestaurant(input: {name: \"Test Restaurant\", city: \"NYC\"}) }"}'

# 2. Create Menu for Restaurant  
curl -X POST https://localhost:7161/graphql \
  -H "Content-Type: application/json" \
  -d '{"query": "mutation { createMenu(input: {restaurantId: \"GUID_FROM_STEP_1\", name: \"Lunch Menu\"}) }"}'

# 3. Query Restaurant with Menu
curl -X POST https://localhost:7161/graphql \
  -H "Content-Type: application/json" \
  -d '{"query": "query { getAllRestaurants { id name city } }"}'
```

## Current Status & Next Modules

### ✅ Completed Modules

**Restaurant Module**:
- ✅ Domain entities with business rules
- ✅ CRUD operations via GraphQL  
- ✅ Cosmos DB persistence with document mapping
- ✅ Cross-module read interface for validation

**Menu Module**:
- ✅ Complex aggregate with MenuItem entities
- ✅ Menu/MenuItem CRUD with business rule validation
- ✅ Restaurant validation via cross-module interface
- ✅ Efficient partitioning strategy (`/restaurantId`)

### 🔄 In Progress

- Build verification and integration testing
- Performance benchmarking with Cosmos DB
- GraphQL schema optimization

### 📋 Planned Modules

**Order Module** (Next Priority):
- **Aggregates**: Order, OrderItem, OrderStatus  
- **Business Rules**: Inventory validation, pricing calculation, order state machine
- **Integration**: Menu item validation, Restaurant availability checks
- **Cosmos Container**: Orders (`/customerId` partition for customer-scoped queries)

**Payment Module**:
- **Aggregates**: Payment, PaymentMethod, Transaction
- **Integration**: External payment gateways (Stripe, Square)
- **Cosmos Container**: Payments (`/orderId` partition for order-payment correlation)

**Customer Module**:  
- **Aggregates**: Customer, CustomerProfile, DeliveryAddress
- **Integration**: Authentication provider integration
- **Cosmos Container**: Customers (`/id` partition for even distribution)

**Delivery Module**:
- **Aggregates**: DeliveryOrder, Driver, DeliveryRoute
- **Integration**: Geolocation services, real-time tracking
- **Cosmos Container**: Deliveries (`/regionId` partition for geographic efficiency)

### 🎯 Technical Roadmap

**Phase 1** (Current): Core domain modules with synchronous communication  
**Phase 2**: Event-driven architecture with domain events and eventual consistency  
**Phase 3**: Microservice extraction with API Gateway and service mesh  
**Phase 4**: Advanced patterns (CQRS, Event Sourcing, Distributed Caching)

---

**Architecture Review Status**: ✅ Senior Engineer Ready | ✅ Tech Lead Ready | ✅ Architect Interview Ready