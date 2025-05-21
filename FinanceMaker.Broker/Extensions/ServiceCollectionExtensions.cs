using FinanceMaker.Broker.Models.Configuration;
using FinanceMaker.Broker.Services;
using FinanceMaker.Broker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceMaker.Broker.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBrokerServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure Polygon
        services.Configure<PolygonOptions>(configuration.GetSection("Polygon"));

        // Add caching
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();

        // Add HTTP client
        services.AddHttpClient<IMarketDataService, PolygonMarketDataService>();

        // Register services
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOrderExecutionService, OrderExecutionService>();
        services.AddHostedService<OrderProcessorBackgroundService>();

        return services;
    }
}
