```markdown
# FoodHub API

FoodHub is a modern backend service built on .NET 9, designed as a Modular Monolith with Clean Architecture principles. It serves as the API for a food delivery and restaurant management platform, starting with the `Restaurant` module.

The architecture is intentionally designed for high maintainability, clear separation of concerns, and future-readiness to be decomposed into microservices.

## 🚀 Tech Stack

- **Framework**: .NET 9
- **API**: GraphQL (Hot Chocolate)
- **Database**: Azure Cosmos DB (SQL API)
- **Architecture**: Modular Monolith, Clean Architecture
- **Logging**: Serilog

## 🏛️ Architecture Overview

The solution follows Clean Architecture, enforcing a strict separation of concerns between its layers. All dependencies flow inwards, towards the central Domain project.

- **FoodHub.Api**: The presentation layer. Exposes a GraphQL endpoint and contains no business logic.
- **FoodHub.Restaurant.Application**: The application layer. Orchestrates use cases (Commands/Queries) and defines repository interfaces.
- **FoodHub.Restaurant.Domain**: The core of the module. Contains pure, framework-independent business entities and logic.
- **FoodHub.Restaurant.Infrastructure**: The infrastructure layer. Implements the data access logic using Azure Cosmos DB.

```ascii
+------------------+      +--------------------------+      +------------------------+      +-------------------------------+
|   Presentation   |----->|       Application        |----->|         Domain         |<-----|        Infrastructure         |
|  (FoodHub.Api)   |      | (Restaurant.Application) |      |   (Restaurant.Domain)  |      | (Restaurant.Infrastructure)   |
+------------------+      +--------------------------+      +------------------------+      +-------------------------------+
```

## 📂 Folder Structure

The `src` directory is organized by feature modules, each containing its own Domain, Application, and Infrastructure projects.

```
/src
├── FoodHub.Api/                  # Host project: GraphQL endpoint, DI, middleware
│
└── FoodHub.Restaurant/           # "Restaurant" business module
    ├── FoodHub.Restaurant.Domain/        # Entities, Value Objects, business rules
    ├── FoodHub.Restaurant.Application/   # Commands, Queries, DTOs, repository interfaces
    └── FoodHub.Restaurant.Infrastructure/  # Cosmos DB repository implementation, data models
```

## ⚙️ How to Run Locally

### Prerequisites

- .NET 9 SDK
- An Azure Cosmos DB account (SQL API)

### 1. Clone the Repository

```bash
git clone <your-repo-url>
cd FoodHub
```

### 2. Configure Your Connection

Open `src/FoodHub.Api/appsettings.Development.json` and update the `Cosmos` section with your Azure Cosmos DB credentials.

```json
{
  "Cosmos": {
    "Endpoint": "YOUR_COSMOS_DB_ENDPOINT_URI",
    "Key": "YOUR_COSMOS_DB_PRIMARY_KEY",
    "Database": "FoodHub",
    "Containers": {
      "Restaurants": "Restaurants"
    }
  }
}
```

### 3. Run the Application

Navigate to the API project directory and run the application.

```bash
cd src/FoodHub.Api
dotnet run
```

The API will be available at `https://localhost:5001`.

## ⚡ GraphQL Endpoint

The GraphQL endpoint is hosted at `/graphql`. When you run the application and navigate to `https://localhost:5001/graphql`, you can use the built-in Banana Cake Pop IDE to explore the schema and execute operations.

### Sample Query

```graphql
query GetRestaurant {
  restaurantById(id: "your-restaurant-guid") {
    id
    name
    city
    cuisine
  }
}
```

### Sample Mutation

```graphql
mutation CreateRestaurant {
  createRestaurant(input: {
    name: "The Golden Spoon",
    cuisine: "Italian",
    city: "New York"
  })
}
```

## 📝 Logging & Observability

- **Structured Logging**: All logs are written to the console in a structured JSON format via Serilog.
- **Correlation ID**: Every HTTP request is assigned a unique `X-Correlation-ID` header. This ID is attached to all log events generated during that request, allowing you to trace a single operation's flow through all layers of the application.
- **Usage**: When reporting an issue or debugging, provide the `X-Correlation-ID` from the response headers to quickly locate all relevant logs.

## 🌱 Future Scalability

This project is designed as a **Modular Monolith**, which means it can be incrementally scaled and decomposed into microservices with minimal friction. Each business module (like `FoodHub.Restaurant`) is self-contained and communicates with the API layer via well-defined contracts, making it a prime candidate to be extracted into its own independent service when the need arises.
```