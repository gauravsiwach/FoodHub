# Prompt 1: Project Setup and Architecture

## Context
Create a production-ready .NET 9 food delivery platform using **Modular Monolith** architecture with **Clean Architecture** principles. The application will be deployed to Azure Kubernetes Service (AKS) with full CI/CD automation.

## Requirements

### Project Structure
Create a solution named `FoodHub` with the following structure:

```
FoodHub/
├── FoodHub.sln
├── README.md
├── ARCHITECTURE.md
├── Dockerfile
├── .dockerignore
├── .gitignore
├── src/
│   ├── FoodHub.Api/                    # Presentation Layer (GraphQL)
│   ├── FoodHub.Restaurant/             # Restaurant Module
│   │   ├── FoodHub.Restaurant.Domain/
│   │   ├── FoodHub.Restaurant.Application/
│   │   └── FoodHub.Restaurant.Infrastructure/
│   ├── FoodHub.Menu/                   # Menu Module
│   │   ├── FoodHub.Menu.Domain/
│   │   ├── FoodHub.Menu.Application/
│   │   └── FoodHub.Menu.Infrastructure/
│   └── FoodHub.User/                   # User Module
│       ├── FoodHub.User.Domain/
│       ├── FoodHub.User.Application/
│       └── FoodHub.User.Infrastructure/
├── k8s/
│   ├── local/                          # Local Kubernetes manifests
│   │   ├── deployment.yaml
│   │   └── service.yaml
│   └── prod/                           # Production Kubernetes manifests
│       ├── deployment.yaml
│       └── service.yaml
└── .github/
    └── workflows/
        └── build-deploy.yml            # CI/CD pipeline
```

### Technology Stack

**Framework & Runtime:**
- .NET 9 (latest)
- ASP.NET Core

**API Layer:**
- GraphQL with HotChocolate 15.1.11
- HotChocolate.AspNetCore.Authorization

**Databases:**
- Azure Cosmos DB (Restaurant & Menu modules)
- SQL Server with Entity Framework Core 9.0.1 (User module)

**Configuration:**
- Azure Key Vault integration with DefaultAzureCredential
- appsettings.json hierarchy

**Logging:**
- Serilog 10.0.0
- Structured logging with correlation IDs
- Console and Debug sinks

**Containerization:**
- Docker multi-stage builds
- .NET SDK 9.0 (build)
- .NET ASP.NET Runtime 9.0 (runtime)

**Cloud Platform:**
- Azure Kubernetes Service (AKS)
- Azure Container Registry (ACR)
- Azure Key Vault

**CI/CD:**
- GitHub Actions
- OIDC Authentication (no secrets)

### Solution File (FoodHub.sln)

Create a solution with the following projects:

1. **FoodHub.Api** - ASP.NET Core Web API (net9.0)
2. **FoodHub.Restaurant.Domain** - Class Library (net9.0)
3. **FoodHub.Restaurant.Application** - Class Library (net9.0)
4. **FoodHub.Restaurant.Infrastructure** - Class Library (net9.0)
5. **FoodHub.Menu.Domain** - Class Library (net9.0)
6. **FoodHub.Menu.Application** - Class Library (net9.0)
7. **FoodHub.Menu.Infrastructure** - Class Library (net9.0)
8. **FoodHub.User.Domain** - Class Library (net9.0)
9. **FoodHub.User.Application** - Class Library (net9.0)
10. **FoodHub.User.Infrastructure** - Class Library (net9.0)

### Project Dependencies

**FoodHub.Api depends on:**
- FoodHub.Restaurant.Application
- FoodHub.Restaurant.Infrastructure
- FoodHub.Menu.Application
- FoodHub.Menu.Infrastructure
- FoodHub.User.Application
- FoodHub.User.Infrastructure

**Application projects depend on:**
- Their respective Domain project only

**Infrastructure projects depend on:**
- Their respective Domain project
- Their respective Application project

### Core Packages (FoodHub.Api.csproj)

```xml
<PackageReference Include="HotChocolate.AspNetCore" Version="15.1.11" />
<PackageReference Include="HotChocolate.AspNetCore.Authorization" Version="15.1.11" />
<PackageReference Include="HotChocolate.Data" Version="15.1.11" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.1" />
<PackageReference Include="Microsoft.Azure.Cosmos" Version="3.56.0" />
<PackageReference Include="Azure.Extensions.AspNetCore.Configuration.Secrets" Version="1.3.2" />
<PackageReference Include="Google.Apis.Auth" Version="1.70.0" />
<PackageReference Include="Serilog.AspNetCore" Version="10.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="6.1.1" />
<PackageReference Include="Serilog.Sinks.Debug" Version="3.0.0" />
```

### Clean Architecture Principles

**Layer Responsibilities:**
1. **Domain Layer** - Pure business logic, entities, value objects, enums
2. **Application Layer** - Use cases (Commands/Queries), DTOs, Repository interfaces
3. **Infrastructure Layer** - Database implementations, external services
4. **Presentation Layer (API)** - GraphQL resolvers, middleware, authentication

**Dependency Flow:**
```
API → Application → Domain
      ↑
Infrastructure
```

### Module Communication Rules

1. **Restaurant Module** exposes `IRestaurantReadRepository` (read-only)
2. **Menu Module** consumes `IRestaurantReadRepository` for validation
3. **User Module** is standalone (no cross-module dependencies)
4. All modules are independent with clear boundaries

### Initial Configuration (appsettings.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" }
    ]
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqlLocalDb;Database=FoodHubDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "KeyVault": {
    "Endpoint": "https://your-keyvault.vault.azure.net/"
  },
  "Cosmos": {
    "Endpoint": "https://your-cosmos.documents.azure.com:443/",
    "DatabaseName": "FoodHubDb",
    "Containers": {
      "Restaurant": {
        "Name": "Restaurants"
      },
      "Menu": {
        "Name": "Menus"
      }
    }
  },
  "GoogleAuth": {
    "ClientId": "your-google-client-id",
    "Aud": "your-google-aud"
  },
  "Jwt": {
    "Issuer": "FoodHub",
    "Audience": "FoodHub",
    "ExpiryMinutes": 60
  }
}
```

### Expected Output

1. Complete solution structure with all projects
2. Project references configured correctly
3. NuGet packages installed
4. appsettings.json with placeholder values
5. Empty Program.cs file in FoodHub.Api
6. README.md documenting the architecture
7. ARCHITECTURE.md with detailed layer descriptions

### Success Criteria

- Solution builds successfully
- All project dependencies are correct
- Clean Architecture boundaries are enforced
- No circular dependencies exist
- Projects use .NET 9 target framework
