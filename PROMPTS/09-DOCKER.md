# Prompt 9: Docker Containerization

## Overview
Create multi-stage Dockerfile for production-ready container deployment to AKS.

---

## Dockerfile (root directory)

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY FoodHub.sln ./
COPY src/FoodHub.Api/FoodHub.Api.csproj src/FoodHub.Api/
COPY src/FoodHub.Restaurant/FoodHub.Restaurant.Application/FoodHub.Restaurant.Application.csproj src/FoodHub.Restaurant/FoodHub.Restaurant.Application/
COPY src/FoodHub.Restaurant/FoodHub.Restaurant.Domain/FoodHub.Restaurant.Domain.csproj src/FoodHub.Restaurant/FoodHub.Restaurant.Domain/
COPY src/FoodHub.Restaurant/FoodHub.Restaurant.Infrastructure/FoodHub.Restaurant.Infrastructure.csproj src/FoodHub.Restaurant/FoodHub.Restaurant.Infrastructure/
COPY src/FoodHub.Menu/FoodHub.Menu.Application/FoodHub.Menu.Application.csproj src/FoodHub.Menu/FoodHub.Menu.Application/
COPY src/FoodHub.Menu/FoodHub.Menu.Domain/FoodHub.Menu.Domain.csproj src/FoodHub.Menu/FoodHub.Menu.Domain/
COPY src/FoodHub.Menu/FoodHub.Menu.Infrastructure/FoodHub.Menu.Infrastructure.csproj src/FoodHub.Menu/FoodHub.Menu.Infrastructure/
COPY src/FoodHub.User/FoodHub.User.Application/FoodHub.User.Application.csproj src/FoodHub.User/FoodHub.User.Application/
COPY src/FoodHub.User/FoodHub.User.Domain/FoodHub.User.Domain.csproj src/FoodHub.User/FoodHub.User.Domain/
COPY src/FoodHub.User/FoodHub.User.Infrastructure/FoodHub.User.Infrastructure.csproj src/FoodHub.User/FoodHub.User.Infrastructure/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY src/ src/

# Build and publish
WORKDIR /src/src/FoodHub.Api
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos \"\" appuser && chown -R appuser /app
USER appuser

# Copy published application
COPY --from=build /app/publish .

# Configure ASP.NET Core
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 8080

# Start application
ENTRYPOINT [\"dotnet\", \"FoodHub.Api.dll\"]
```

---

## .dockerignore (root directory)

```
**/bin/
**/obj/
**/.vs/
**/.vscode/
**/node_modules/
**/.git/
**/.gitignore
**/.dockerignore
**/Dockerfile
**/docker-compose*.yml
**/*.md
**/*.log
**/appsettings.Development.json
```

---

## Build & Test Locally

### Build Image:
```bash
docker build -t foodhub-api:latest .
```

### Run Container:
```bash
docker run -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e AZURE_CLIENT_ID=\"your-client-id\" \
  -e AZURE_CLIENT_SECRET=\"your-client-secret\" \
  -e AZURE_TENANT_ID=\"your-tenant-id\" \
  foodhub-api:latest
```

### Test Endpoint:
```bash
curl http://localhost:8080/graphql
```

---

## Architecture Considerations

### Multi-Stage Benefits:
1. **Build stage:** Full SDK (700MB+) - builds and publishes
2. **Runtime stage:** ASP.NET runtime only (200MB) - runs application
3. **Size reduction:** 70% smaller final image
4. **Security:** No build tools in production image

### Security Features:
- Non-root user (`appuser`)
- Minimal runtime dependencies
- No unnecessary files
- Production environment by default

### Port Configuration:
- Internal port: 8080 (non-privileged)
- Kubernetes exposes via Service (LoadBalancer)

---

## Success Criteria

- Image builds successfully
- Container runs and serves GraphQL endpoint
- Application logs visible in `docker logs`
- Size optimized (< 250MB)
- Non-root user execution
- Environment variables work correctly
