using Microsoft.Extensions.DependencyInjection;
using AIDIMS.Application.Interfaces;
using AIDIMS.Application.UseCases;
using FluentValidation;
using System.Reflection;

namespace AIDIMS.Application;

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
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IPatientVisitService, PatientVisitService>();
        services.AddScoped<IImagingOrderService, ImagingOrderService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDicomService, DicomService>();
        services.AddScoped<IAiAnalysisService, AiAnalysisService>();

        // Add other services here

        return services;
    }
}
