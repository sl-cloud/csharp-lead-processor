using LeadProcessor.Domain.Services;
using LeadProcessor.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LeadProcessor.Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for configuring infrastructure services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Infrastructure layer services to the dependency injection container.
    /// This includes date/time providers and other infrastructure services.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Register date/time provider
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        return services;
    }
}

