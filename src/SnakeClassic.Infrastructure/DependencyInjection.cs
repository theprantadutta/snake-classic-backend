using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Infrastructure.Persistence;
using SnakeClassic.Infrastructure.Services;
using SnakeClassic.Infrastructure.Services.BackgroundJobs;

namespace SnakeClassic.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database connection string from environment or config
        var connectionString = BuildConnectionString(configuration);

        // Entity Framework Core with PostgreSQL
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "public");
                npgsqlOptions.EnableRetryOnFailure(3);
            });
        });

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        // Hangfire with PostgreSQL
        services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180);
            config.UseSimpleAssemblyNameTypeSerializer();
            config.UseRecommendedSerializerSettings();
            config.UsePostgreSqlStorage(options =>
            {
                options.UseNpgsqlConnection(connectionString);
            }, new PostgreSqlStorageOptions
            {
                SchemaName = configuration["Hangfire:SchemaName"] ?? "snake_classic_hangfire",
                QueuePollInterval = TimeSpan.FromSeconds(15),
                PrepareSchemaIfNecessary = true
            });
        });

        services.AddHangfireServer();

        // Firebase services
        services.AddSingleton<IFirebaseAuthService, FirebaseAuthService>();
        services.AddSingleton<IFirebaseMessagingService, FirebaseMessagingService>();

        // JWT service
        services.AddSingleton<IJwtService, JwtService>();

        // DateTime service
        services.AddSingleton<IDateTimeService, DateTimeService>();

        // Background job services
        services.AddScoped<INotificationJobService, NotificationJobService>();
        services.AddScoped<ITournamentManagementJobService, TournamentManagementJobService>();

        return services;
    }

    private static string BuildConnectionString(IConfiguration configuration)
    {
        // Try to get full connection string first
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(connectionString))
        {
            return connectionString;
        }

        // Build from environment variables
        var host = Environment.GetEnvironmentVariable("DATABASE_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("DATABASE_PORT") ?? "5432";
        var database = Environment.GetEnvironmentVariable("DATABASE_NAME") ?? "snake_classic";
        var username = Environment.GetEnvironmentVariable("DATABASE_USERNAME") ?? "postgres";
        var password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "";

        return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
    }
}
