# Complete Application Prompts for FoodHub Modular Monolith

This directory contains sequential prompts to recreate the entire FoodHub application from scratch.

## What You'll Build: FoodHub Application

**FoodHub** is a production-ready **food delivery platform** that connects restaurants with customers through a modern GraphQL API. Think of it as the backend for applications like UberEats, DoorDash, or Grubhub.

### Application Capabilities

**For Restaurants:**
- Create and manage restaurant profiles (name, location, contact info)
- Build digital menus with categories and pricing
- Add menu items with descriptions, images, and availability status
- Update prices and item availability in real-time
- Manage multiple menus per restaurant (breakfast, lunch, dinner)

**For Users:**
- Sign in securely using Google accounts
- Browse restaurant listings
- View detailed menus with prices and images
- Access personalized user profiles
- Secure authentication for all operations

**For Administrators:**
- Manage restaurant approvals
- Monitor system operations through structured logs
- Track user activity with correlation IDs
- Scale deployment across cloud infrastructure

### Real-World Architecture

This isn't a toy application - it's built with enterprise patterns:

- **Modular Monolith Design**: Three business modules (Restaurant, Menu, User) that could be extracted into microservices later
- **Multi-Database Strategy**: NoSQL (Cosmos DB) for high-read restaurant/menu data, SQL Server for transactional user data
- **Cloud-Native**: Runs on Azure Kubernetes Service with automatic scaling and health monitoring
- **Security-First**: Google OAuth identity verification, JWT authorization, Azure Key Vault secret management
- **DevOps Ready**: Automated CI/CD pipeline deploys to production on every git push

### Why This Project Matters

By building FoodHub, you'll gain hands-on experience with:
- **Modern .NET development** using .NET 9 and GraphQL APIs
- **Domain-Driven Design** with proper aggregate boundaries and value objects
- **Cloud architecture** patterns used by Fortune 500 companies
- **Production deployment** with Docker, Kubernetes, and Azure
- **Zero-downtime deployments** through automated pipelines

This is the kind of system used by real food delivery platforms serving millions of users.

## Prompt Sequence

### Phase 1: Foundation
1. **[01-PROJECT-SETUP.md](./01-PROJECT-SETUP.md)** - Solution structure, Clean Architecture setup, technology stack
2. **[02-RESTAURANT-MODULE.md](./02-RESTAURANT-MODULE.md)** - Restaurant module (Domain, Application, Infrastructure)
3. **[03-MENU-MODULE.md](./03-MENU-MODULE.md)** - Menu module with cross-module dependencies
4. **[04-USER-MODULE.md](./04-USER-MODULE.md)** - User module with SQL Server & Entity Framework

### Phase 2: Security & Integration
5. **[05-AUTHENTICATION.md](./05-AUTHENTICATION.md)** - Google OAuth + JWT authentication system
6. **[06-GRAPHQL-API.md](./06-GRAPHQL-API.md)** - GraphQL API layer with HotChocolate
7. **[07-KEYVAULT.md](./07-KEYVAULT.md)** - Azure Key Vault integration with DefaultAzureCredential
8. **[08-LOGGING.md](./08-LOGGING.md)** - Serilog structured logging with correlation IDs

### Phase 3: DevOps & Deployment
9. **[09-DOCKER.md](./09-DOCKER.md)** - Docker containerization with multi-stage builds
10. **[10-KUBERNETES.md](./10-KUBERNETES.md)** - Kubernetes manifests for local and production
11. **[11-CICD-PIPELINE.md](./11-CICD-PIPELINE.md)** - GitHub Actions CI/CD with OIDC authentication

### Phase 4: Quality Assurance
12. **[12-UNIT-TESTING.md](./12-UNIT-TESTING.md)** - Unit tests for Domain and Application layers with xUnit, FluentAssertions, and NSubstitute

## How to Use

1. **Start with Phase 1** - Build the foundation
2. **Complete each prompt sequentially** - Each builds on the previous
3. **Test after each phase** - Verify functionality before moving forward
4. **Phase 2 adds security** - Authentication and API layer
5. **Phase 3 enables deployment** - Containerization and automation

## Architecture Highlights

- **Pattern:** Modular Monolith + Clean Architecture
- **Modules:** Restaurant, Menu, User (loosely coupled)
- **API:** GraphQL with authorization
- **Authentication:** Google OAuth → FoodHub JWT (two-token system)
- **Databases:** Cosmos DB (Restaurant/Menu), SQL Server (User)
- **Configuration:** Azure Key Vault with local fallback
- **Deployment:** Docker → AKS via GitHub Actions

## Technology Stack

- .NET 9
- GraphQL (HotChocolate 15.1.11)
- Azure Cosmos DB
- SQL Server + Entity Framework Core 9.0.1
- Azure Key Vault
- Serilog
- Docker & Kubernetes
- GitHub Actions with OIDC
- xUnit + FluentAssertions + NSubstitute (Testing)

## Success Criteria

After completing all prompts, you will have:
- ✅ Production-ready modular monolith
- ✅ Complete authentication system
- ✅ GraphQL API with security
- ✅ Multi-database architecture
- ✅ Azure cloud integration
- ✅ Automated CI/CD pipeline
- ✅ Kubernetes deployment manifests
- ✅ Comprehensive unit test coverage
