using FinanceMaker.Broker.Data;
using FinanceMaker.Broker.Interfaces;
using FinanceMaker.Broker.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FinanceMaker.Broker.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBrokerServices(this IServiceCollection services, IConfiguration configuration)
    {
        var dbSettings = configuration.GetSection("Database").Get<DatabaseSettings>() ?? new DatabaseSettings();

        // Configure database provider
        services.AddDbContext<BrokerDbContext>((serviceProvider, options) =>
        {
            switch (dbSettings.Provider)
            {
                case DatabaseProvider.Postgres:
                    options.UseNpgsql(dbSettings.ConnectionString);
                    break;
                case DatabaseProvider.Sqlite:
                default:
                    options.UseSqlite(dbSettings.ConnectionString);
                    break;
            }
        });

        services.AddScoped<IBrokerDbContext, BrokerDbContext>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IMarketDataService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<MarketDataService>>();
            return new MarketDataService(logger);
        });
        services.AddScoped<OrderProcessorService>();
        services.AddHostedService<OrderProcessorBackgroundService>();

        return services;
    }
}
