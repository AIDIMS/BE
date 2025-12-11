using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AIDIMS.Application.Interfaces;
using AIDIMS.Application.UseCases;
using AIDIMS.Application.Events;
using AIDIMS.Application.Events.Handlers;
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

        // Register Event System
        var channel = Channel.CreateUnbounded<object>(new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = false
        });

        services.AddSingleton(channel.Reader);
        services.AddSingleton(channel.Writer);
        services.AddScoped<IEventPublisher, EventPublisher>();

        // Register Event Handlers
        services.AddScoped<IEventHandler<Domain.Events.DicomUploadedEvent>, DicomUploadedEventHandler>();

        // Register Background Service để xử lý events
        services.AddHostedService<EventProcessorBackgroundService>();

        // Register services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IPatientVisitService, PatientVisitService>();
        services.AddScoped<IImagingOrderService, ImagingOrderService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDicomService, DicomService>();
        services.AddScoped<IAiAnalysisService, AiAnalysisService>();
        services.AddScoped<IImageAnnotationService, ImageAnnotationService>();
        services.AddScoped<IDiagnosisService, DiagnosisService>();

        // Add other services here

        return services;
    }
}
