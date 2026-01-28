# Prompt 6: GraphQL API Layer
## Overview
Implement GraphQL presentation layer using HotChocolate with authorization.
## HotChocolate Configuration (Program.cs)
`csharp
services.AddGraphQLServer()
    .AddAuthorization()
    .AddQueryType()
    .AddTypeExtension<RestaurantQuery>()
    .AddTypeExtension<MenuQuery>()
    .AddTypeExtension<UserQuery>()
    .AddMutationType()
    .AddTypeExtension<RestaurantMutation>()
    .AddTypeExtension<MenuMutation>()
    .AddTypeExtension<UserMutation>();
app.MapGraphQL(\"/graphql\");
`
## Query Resolvers
Each module gets Query class in FoodHub.Api/GraphQL/Queries/:
- RestaurantQuery (GetRestaurantById, GetAllRestaurants)
- MenuQuery (GetMenuById, GetMenuByRestaurantId)
- UserQuery (GetUserById, GetAllUsers, GetUserByEmail)
### Structure:
`csharp
[Authorize]
[ExtendObjectType(\"Query\")]
public sealed class RestaurantQuery
{
    public async Task<RestaurantDto?> GetRestaurantById(
        Guid id,
        [Service] IRestaurantRepository repository,
        [Service] Serilog.ILogger logger,
        CancellationToken cancellationToken)
    {
        // Use Query pattern from Application layer
    }
}
`
## Mutation Resolvers
Each module gets Mutation class:
- RestaurantMutation (CreateRestaurant)
- MenuMutation (CreateMenu, AddMenuItem, UpdateMenuItem)
- UserMutation (CreateUser, UpdateUser)
## Success Criteria
- /graphql endpoint accessible
- All queries require authentication
- Serilog logging in resolvers
- DTOs returned (not domain entities)
