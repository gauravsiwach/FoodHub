# Prompt 8: Serilog Structured Logging
## Package
`xml
<PackageReference Include=\"Serilog.AspNetCore\" Version=\"10.0.0\" />
<PackageReference Include=\"Serilog.Sinks.Console\" Version=\"6.1.1\" />
<PackageReference Include=\"Serilog.Sinks.Debug\" Version=\"3.0.0\" />
`
## Configuration (Program.cs)
`csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override(\"Microsoft\", LogEventLevel.Warning)
    .MinimumLevel.Override(\"Microsoft.AspNetCore\", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: \"[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}\")
    .WriteTo.Debug()
    .CreateLogger();
builder.Services.AddSingleton<Serilog.ILogger>(Log.Logger);
builder.Host.UseSerilog();
`
## Correlation ID Middleware
`csharp
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers[\"X-Correlation-ID\"].FirstOrDefault() 
        ?? Guid.NewGuid().ToString();
    context.Response.Headers.Append(\"X-Correlation-ID\", correlationId);
    using (Serilog.Context.LogContext.PushProperty(\"CorrelationId\", correlationId))
    {
        await next();
    }
});
`
## Usage in Resolvers
`csharp
logger.ForContext<RestaurantQuery>().Information(
    \"Begin: GetRestaurantById query for {RestaurantId}\", id);
`
## Success Criteria
- Structured logging to console
- Correlation IDs in all logs
- Context enrichment works
- Log levels properly configured
