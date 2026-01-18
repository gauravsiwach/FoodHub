using FoodHub.User.Application.Interfaces;
using FoodHub.User.Infrastructure.Sql;
using FoodHub.User.Infrastructure.Sql.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FoodHub.User.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUserModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<UserDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(UserDbContext).Assembly.FullName)));

        // Register Repository
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}