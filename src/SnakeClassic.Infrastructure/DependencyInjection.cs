using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
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

        // Create NpgsqlDataSource with dynamic JSON enabled for JSONB columns
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        // Entity Framework Core with PostgreSQL
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(dataSource, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "public");
                // Retry on transient failures (connection drops, network issues, etc.)
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null); // Uses Npgsql's default transient error codes
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

        // Multiplayer services
        services.AddScoped<IMatchmakingService, MatchmakingService>();

        return services;
    }

    private static string BuildConnectionString(IConfiguration configuration)
    {
        // Try to get full connection string first
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(connectionString))
        {
            // Append connection resiliency settings if not already present
            return AppendConnectionResiliencySettings(connectionString);
        }

        // Build from environment variables
        var host = Environment.GetEnvironmentVariable("DATABASE_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("DATABASE_PORT") ?? "5432";
        var database = Environment.GetEnvironmentVariable("DATABASE_NAME") ?? "snake_classic";
        var username = Environment.GetEnvironmentVariable("DATABASE_USERNAME") ?? "postgres";
        var password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "";

        var baseConnectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";
        return AppendConnectionResiliencySettings(baseConnectionString);
    }

    /// <summary>
    /// Appends connection resiliency settings to prevent stale connection errors.
    /// These settings help with:
    /// - Keepalive: Periodic packets to prevent server from closing idle connections
    /// - Connection Lifetime: Max time a connection can be reused before being recycled
    /// - Connection Idle Lifetime: Max time a connection can sit idle in pool
    /// - Pooling settings: Control how connections are managed
    /// </summary>
    private static string AppendConnectionResiliencySettings(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            // Send keepalive packets every 30 seconds to keep connections alive
            KeepAlive = 30,

            // Max time (seconds) a connection can be reused before being closed and recreated
            // Prevents issues with connections that become stale after long periods
            ConnectionLifetime = 300, // 5 minutes

            // Max time (seconds) a connection can sit idle in the pool before being pruned
            ConnectionIdleLifetime = 60, // 1 minute

            // How often (seconds) to check for and remove idle connections
            ConnectionPruningInterval = 10,

            // Connection pool size
            MinPoolSize = 1,
            MaxPoolSize = 20,

            // Timeout for acquiring a connection from pool (seconds)
            Timeout = 30,

            // Command timeout (seconds)
            CommandTimeout = 60,

            // Enable connection pooling
            Pooling = true,
        };

        return builder.ConnectionString;
    }
}
