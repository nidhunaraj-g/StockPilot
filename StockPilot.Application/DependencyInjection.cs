using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace StockPilot.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // Register MediatR handlers.
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(assembly));

        // Register FluentValidation validators.
        services.AddValidatorsFromAssembly(assembly);

        // Register AutoMapper profiles.
        services.AddAutoMapper(
            configuration => { },
            assembly);

        return services;
    }
}
