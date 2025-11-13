using AIDIMS.Application.Interfaces;
using AIDIMS.Domain.Interfaces;
using AIDIMS.Infrastructure.Data;
using AIDIMS.Infrastructure.Repositories;
using AIDIMS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AIDIMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add DbContext with PostgreSQL
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Register repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register authentication services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        // Add other infrastructure services here
        services.AddHttpContextAccessor();
        return services;
    }
}
