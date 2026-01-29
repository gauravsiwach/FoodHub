# FoodHub Application Reconstruction Guide

## 📋 Overview
This directory contains **11 sequential prompts** to recreate the entire FoodHub modular monolith application from scratch. Each prompt is self-contained with complete code examples, configurations, and success criteria.

---

## 🎯 Application Summary

**FoodHub** is a production-ready food delivery platform built with:
- **Architecture:** Modular Monolith + Clean Architecture
- **Technology:** .NET 9, GraphQL, Azure Cosmos DB, SQL Server
- **Authentication:** Google OAuth → JWT (two-token system)
- **Infrastructure:** Docker, Kubernetes (AKS), GitHub Actions CI/CD
- **Security:** Azure Key Vault, OIDC authentication (zero secrets)

---

## 📚 Prompt Sequence

### Phase 1: Foundation (Prompts 1-4)
Build the core modular monolith architecture with three business modules.

#### **[01-PROJECT-SETUP.md](01-PROJECT-SETUP.md)** 
**Duration:** 30 minutes  
**What You'll Build:**
- Complete solution structure with 10 projects
- Clean Architecture layer boundaries
- NuGet package configuration
- appsettings.json with placeholders

**Key Concepts:** Modular Monolith, Clean Architecture, Dependency Inversion

---

#### **[02-RESTAURANT-MODULE.md](02-RESTAURANT-MODULE.md)**
**Duration:** 45 minutes  
**What You'll Build:**
- Restaurant Domain (Entity, Value Object, Exception)
- Restaurant Application (Commands, Queries, DTOs, Repository Interface)
- Restaurant Infrastructure (Cosmos DB implementation)

**Key Concepts:** DDD, Aggregate Root, Repository Pattern, Cosmos DB Partitioning

---

#### **[03-MENU-MODULE.md](03-MENU-MODULE.md)**
**Duration:** 60 minutes  
**What You'll Build:**
- Menu Domain (Menu + MenuItem entities, Price/MenuImage value objects)
- Menu Application (CRUD operations, cross-module validation)
- Menu Infrastructure (Cosmos DB with nested documents)
- Cross-module interface implementation

**Key Concepts:** Aggregate Composition, Cross-Module Communication, Value Objects

---

#### **[04-USER-MODULE.md](04-USER-MODULE.md)**
**Duration:** 45 minutes  
**What You'll Build:**
- User Domain (User entity, Email value object)
- User Application (User management operations)
- User Infrastructure (SQL Server + Entity Framework Core)
- DbContext with indexes and constraints

**Key Concepts:** Entity Framework Core, Code-First Migrations, Value Object Validation

---

### Phase 2: Security & Integration (Prompts 5-8)
Add authentication, API layer, and cloud integrations.

#### **[05-AUTHENTICATION.md](05-AUTHENTICATION.md)**
**Duration:** 45 minutes  
**What You'll Build:**
- Google OAuth token validator
- JWT token generator with claims
- Google Auth Controller (POST /auth/google)
- JWT authentication middleware
- [Authorize] attributes on GraphQL endpoints

**Key Concepts:** OAuth 2.0, JWT, Claims-Based Auth, Token Exchange

---

#### **[06-GRAPHQL-API.md](06-GRAPHQL-API.md)**
**Duration:** 30 minutes  
**What You'll Build:**
- HotChocolate GraphQL server
- Query resolvers (Restaurant, Menu, User)
- Mutation resolvers with logging
- Authorization integration

**Key Concepts:** GraphQL, Type Extensions, Dependency Injection in Resolvers

---

#### **[07-KEYVAULT.md](07-KEYVAULT.md)**
**Duration:** 20 minutes  
**What You'll Build:**
- Azure Key Vault configuration
- DefaultAzureCredential setup
- Environment-based configuration hierarchy
- Secret management strategy

**Key Concepts:** Azure Key Vault, DefaultAzureCredential, Secret Management

---

#### **[08-LOGGING.md](08-LOGGING.md)**
**Duration:** 20 minutes  
**What You'll Build:**
- Serilog structured logging
- Correlation ID middleware
- Log context enrichment
- Console and Debug sinks

**Key Concepts:** Structured Logging, Correlation IDs, Log Enrichment

---

### Phase 3: DevOps & Deployment (Prompts 9-11)
Containerize and automate deployment to Azure Kubernetes Service.

#### **[09-DOCKER.md](09-DOCKER.md)**
**Duration:** 30 minutes  
**What You'll Build:**
- Multi-stage Dockerfile (SDK → Runtime)
- .dockerignore file
- Non-root user configuration
- Environment variable setup

**Key Concepts:** Multi-Stage Builds, Container Security, Image Optimization

---

#### **[10-KUBERNETES.md](10-KUBERNETES.md)**
**Duration:** 40 minutes  
**What You'll Build:**
- Local Kubernetes manifests (Docker Desktop)
- Production AKS manifests
- Deployment with health probes
- Service configuration (NodePort vs LoadBalancer)

**Key Concepts:** Kubernetes Deployments, Services, Health Probes, Resource Limits

---

#### **[11-CICD-PIPELINE.md](11-CICD-PIPELINE.md)**
**Duration:** 60 minutes  
**What You'll Build:**
- GitHub Actions workflow
- OIDC authentication to Azure (no secrets)
- Docker build with platform targeting (linux/amd64)
- Automated deployment to AKS
- Azure infrastructure setup scripts

**Key Concepts:** CI/CD, OIDC, GitHub Actions, Azure Container Registry, AKS

---

### Phase 4: Quality Assurance (Prompt 12)
Implement comprehensive testing for business logic and application workflows.

#### **[12-UNIT-TESTING.md](12-UNIT-TESTING.md)**
**Duration:** 90 minutes  
**What You'll Build:**
- Unit tests for all Domain entities and value objects
- Application layer tests with mocked dependencies
- Test projects for Restaurant, Menu, and User modules
- xUnit + FluentAssertions + NSubstitute configuration
- Cross-module validation tests

**Key Concepts:** Unit Testing, Mocking, AAA Pattern, Test-Driven Development, FluentAssertions

---

## 🏗️ Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                        GitHub Actions                           │
│  (CI/CD Pipeline with OIDC - No Secrets)                        │
└────────────────────┬────────────────────────────────────────────┘
                     │ Push to main
                     ▼
┌─────────────────────────────────────────────────────────────────┐
│              Azure Container Registry (ACR)                     │
│  foodhubacr.azurecr.io/foodhub-api:latest                      │
└────────────────────┬────────────────────────────────────────────┘
                     │ Pull image
                     ▼
┌─────────────────────────────────────────────────────────────────┐
│              Azure Kubernetes Service (AKS)                     │
│  ┌──────────────────────────────────────────────────┐           │
│  │           LoadBalancer Service (Port 80)         │           │
│  └──────────────┬───────────────────────────────────┘           │
│                 ▼                                                │
│  ┌──────────────────────────────────────────────────┐           │
│  │        Pods (foodhub-api containers)             │           │
│  │  - Health Probes (/graphql)                      │           │
│  │  - Resource Limits (256Mi-512Mi)                 │           │
│  │  - Non-root user execution                       │           │
│  └──────────────┬───────────────────────────────────┘           │
└─────────────────┼───────────────────────────────────────────────┘
                  │
                  ├──────────► Azure Cosmos DB
                  │             (Restaurant & Menu)
                  │
                  ├──────────► SQL Server
                  │             (User Module)
                  │
                  └──────────► Azure Key Vault
                                (Secrets & Configuration)
```

---

## 🔑 Key Features

### Security
- ✅ Google OAuth identity verification
- ✅ JWT-based API authorization
- ✅ Azure Key Vault for secrets
- ✅ OIDC authentication (zero secrets in GitHub)
- ✅ Non-root container execution

### Architecture
- ✅ Modular Monolith (3 loosely-coupled modules)
- ✅ Clean Architecture (Domain, Application, Infrastructure, API)
- ✅ DDD patterns (Aggregates, Value Objects, Repository)
- ✅ Cross-module communication via interfaces

### Database
- ✅ Azure Cosmos DB (Restaurant, Menu)
- ✅ SQL Server + EF Core (User)
- ✅ Partition strategies for performance
- ✅ Code-First migrations

### DevOps
- ✅ Docker multi-stage builds
- ✅ Kubernetes deployment automation
- ✅ GitHub Actions CI/CD
- ✅ Platform targeting (linux/amd64)
- ✅ Health checks and resource limits

### Observability
- ✅ Serilog structured logging
- ✅ Correlation ID tracking
- ✅ GraphQL endpoint monitoring

---

## 📊 Technology Stack

| Layer | Technology | Version |
|-------|-----------|---------|
| **Framework** | .NET | 9.0 |
| **API** | HotChocolate GraphQL | 15.1.11 |
| **Auth** | Google.Apis.Auth + JWT | 1.70.0 |
| **Database (NoSQL)** | Azure Cosmos DB | 3.56.0 |
| **Database (SQL)** | SQL Server + EF Core | 9.0.1 |
| **Configuration** | Azure Key Vault | 1.3.2 |
| **Logging** | Serilog | 10.0.0 |
| **Container** | Docker | - |
| **Orchestration** | Kubernetes (AKS) | - |
| **CI/CD** | GitHub Actions | - |
| **Testing** | xUnit | 2.9.3 |
| **Assertions** | FluentAssertions | 6.12.2 |
| **Mocking** | NSubstitute | 5.3.0 |

---

## 🎓 Learning Outcomes

After completing all prompts, you will have hands-on experience with:

### Architecture Patterns
- Modular Monolith design
- Clean Architecture implementation
- Domain-Driven Design principles
- Cross-module communication strategies

### Backend Development
- .NET 9 modern features
- GraphQL API design with HotChocolate
- Multi-database architecture (NoSQL + SQL)
- Repository and CQRS patterns

### Cloud & DevOps
- Azure Cosmos DB partitioning and querying
- Azure Key Vault secret management
- Docker containerization best practices
- Kubernetes deployment and configuration
- GitHub Actions CI/CD with OIDC

### Security
- OAuth 2.0 + JWT authentication flow
- Claims-based authorization
- Secure secret management
- Container security (non-root execution)

### Testing & Quality
- Unit testing with xUnit
- Mocking dependencies with NSubstitute
- Readable assertions with FluentAssertions
- Test-driven development practices
- Domain and Application layer test coverage

---

## 📝 Usage Instructions

### Prerequisites
- .NET 9 SDK
- Docker Desktop
- Azure subscription
- GitHub account
- Visual Studio Code or Visual Studio

### Execution Order
1. **Complete prompts sequentially** (1→11)
2. **Test after each phase** before moving forward
3. **Keep notes** of any deviations or custom configurations
4. **Commit frequently** to track progress

### Time Estimate
- **Phase 1 (Foundation):** 3-4 hours
- **Phase 2 (Integration):** 2 hours
- **Phase 3 (DevOps):** 2-3 hours
- **Phase 4 (Testing):** 1.5 hours
- **Total:** 8.5-10.5 hours for complete implementation

---

## ✅ Final Success Criteria

Your completed application will:
- ✅ Build and run locally with all modules functioning
- ✅ Authenticate users via Google OAuth
- ✅ Serve GraphQL API with authorization
- ✅ Persist data to Cosmos DB and SQL Server
- ✅ Retrieve secrets from Azure Key Vault
- ✅ Run in Docker container
- ✅ Deploy to local Kubernetes
- ✅ Automatically deploy to AKS on git push
- ✅ Be accessible via LoadBalancer external IP
- ✅ Have >80% unit test coverage for business logic

---

## 🔗 Additional Resources

- [Official Documentation](../README.md)
- [Architecture Overview](../ARCHITECTURE.md)
- [Authentication Flow](../AUTHENTICATION_FLOW.md)

---

## 🤝 Support

If you encounter issues:
1. Check the **Success Criteria** section in each prompt
2. Review the **Expected Output** for validation
3. Verify all prerequisites are met
4. Ensure configurations match your Azure environment

---

**Happy Building! 🚀**
