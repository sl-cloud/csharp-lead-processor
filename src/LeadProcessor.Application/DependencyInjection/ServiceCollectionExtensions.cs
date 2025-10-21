using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace LeadProcessor.Application.DependencyInjection;

/// <summary>
/// Extension methods for configuring application services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Application layer services to the dependency injection container.
    /// This includes MediatR, FluentValidation, and all command handlers and validators.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Register MediatR with all handlers from this assembly
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);
        });

        // Register FluentValidation validators from this assembly
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}

