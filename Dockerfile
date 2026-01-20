# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file and project files for restore
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
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# Copy published application
COPY --from=build /app/publish .

# Configure ASP.NET Core
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Start application
ENTRYPOINT ["dotnet", "FoodHub.Api.dll"]