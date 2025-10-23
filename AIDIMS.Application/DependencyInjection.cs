using Microsoft.Extensions.DependencyInjection;
using AIDIMS.Application.Interfaces;
using AIDIMS.Application.UseCases;
using FluentValidation;
using System.Reflection;

namespace AIDIMS.Application;

/// <summary>
/// Dependency injection configuration for Application layer
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register AutoMapper
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // Register FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Register services
        services.AddScoped<IUserService, UserService>();

        // Add other services here

        return services;
    }
}
