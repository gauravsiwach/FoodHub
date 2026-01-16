using FoodHub.Api.GraphQL.Mutations;
using FoodHub.Api.GraphQL.Queries;
using FoodHub.Restaurant.Application.Interfaces;
using FoodHub.Restaurant.Infrastructure.Persistence.Cosmos;
using FoodHub.Restaurant.Infrastructure.Persistence.Repositories;
using FoodHub.Menu.Application.Interfaces;
using FoodHub.Menu.Infrastructure.Persistence.Cosmos;
using Microsoft.Azure.Cosmos;
using FoodHub.Menu.Infrastructure;
using Serilog;
using Serilog.Events;
using HotChocolate.AspNetCore;


var builder = WebApplication.CreateBuilder(args);

// Bind options for both modules from the single top-level "Cosmos" section
var cosmosSection = builder.Configuration.GetSection("Cosmos");
builder.Services.Configure<FoodHub.Restaurant.Infrastructure.Persistence.Cosmos.CosmosOptions>(cosmosSection);
builder.Services.Configure<FoodHub.Menu.Infrastructure.Persistence.Cosmos.CosmosOptions>(cosmosSection);

// Build shared CosmosClient from top-level Cosmos credentials
var endpoint = cosmosSection.GetValue<string>("Endpoint") ?? throw new InvalidOperationException("Cosmos:Endpoint is not configured.");
var key = cosmosSection.GetValue<string>("Key") ?? throw new InvalidOperationException("Cosmos:Key is not configured.");

// Cosmos client (shared instance)
builder.Services.AddSingleton(new CosmosClient(endpoint, key));

// Contexts (module-specific)
builder.Services.AddSingleton<FoodHub.Restaurant.Infrastructure.Persistence.Cosmos.CosmosContext>();
builder.Services.AddSingleton<FoodHub.Menu.Infrastructure.Persistence.Cosmos.CosmosContext>();

// Repositories: register Restaurant and Menu repositories (single entries)
builder.Services.AddScoped<IRestaurantRepository, RestaurantRepository>();

// Menu repository (implementation in Menu.Infrastructure)
builder.Services.AddScoped<IMenuRepository, FoodHub.Menu.Infrastructure.Persistence.Repositories.MenuRepository>();

builder.Services
    .AddGraphQLServer()
    .AddQueryType<RestaurantQuery>()
    .AddMutationType<RestaurantMutation>();

// Serilog configuration
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.Debug()
    .CreateLogger();

// Expose Serilog's logger for code that requires Serilog-specific APIs
builder.Services.AddSingleton<Serilog.ILogger>(Log.Logger);

builder.Host.UseSerilog();

var app = builder.Build();

// Correlation ID Middleware
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();
    context.Response.Headers.Append("X-Correlation-ID", correlationId);

    using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
    {
        await next();
    }
});

app.MapGraphQL("/graphql");

// To enable the Banana Cake Pop IDE, add the appropriate HotChocolate tooling package
// and call `app.UseBananaCakePop()` here (optional).

app.Run();

