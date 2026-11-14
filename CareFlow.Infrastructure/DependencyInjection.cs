using CareFlow.Application.Abstractions;
using CareFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CareFlow.Infrastructure;

public static class CareFlowDatabase
{
    public static string DefaultConnectionString
    {
        get
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CareFlow");
            Directory.CreateDirectory(folder);
            return $"Data Source={Path.Combine(folder, "careflow.db")}";
        }
    }
}

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        // Transient: each repository gets its own DbContext (no stale shared state)
        services.AddDbContext<CareFlowDbContext>(
            o => o.UseSqlite(connectionString), ServiceLifetime.Transient);
        services.AddTransient<IBookingRepository, EfBookingRepository>();
        services.AddSingleton<IClock, SystemClock>();
        return services;
    }
}