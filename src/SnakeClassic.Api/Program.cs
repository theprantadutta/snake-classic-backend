using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Hangfire;
using Hangfire.Dashboard;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using SnakeClassic.Api.Hubs;
using SnakeClassic.Api.Services;
using SnakeClassic.Application;
using SnakeClassic.Application.Common.Interfaces;
using SnakeClassic.Application.Features.Achievements.Commands.SeedAchievements;
using SnakeClassic.Infrastructure;
using SnakeClassic.Infrastructure.Services.BackgroundJobs;

// Load .env file if it exists (check multiple locations)
var possibleEnvPaths = new[]
{
    Path.Combine(Directory.GetCurrentDirectory(), ".env"),
    Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env"),
    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".env"),
};

var envFilePath = possibleEnvPaths.FirstOrDefault(File.Exists);
if (envFilePath != null)
{
    foreach (var line in File.ReadAllLines(envFilePath))
    {
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            continue;

        var parts = line.Split('=', 2);
        if (parts.Length == 2)
        {
            Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
        }
    }
}

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/snake-classic-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting Snake Classic API...");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog
    builder.Host.UseSerilog();

    // Add Application and Infrastructure layers
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // Add HttpContextAccessor for CurrentUserService
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

    // Configure JSON serialization (snake_case for frontend compatibility)
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
            options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

    // Configure CORS
    var allowedOrigins = builder.Configuration["AllowedOrigins"]
        ?? Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")
        ?? "*";

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            if (allowedOrigins == "*")
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            }
            else
            {
                policy.WithOrigins(allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            }
        });
    });

    // Configure JWT Authentication
    var jwtSecretKey = builder.Configuration["JwtSettings:SecretKey"]
        ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
        ?? throw new InvalidOperationException("JWT_SECRET_KEY is not configured");

    var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "SnakeClassicApi";
    var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "SnakeClassicApp";

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // Support SignalR authentication via query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

    builder.Services.AddAuthorization();

    // Add SignalR
    builder.Services.AddSignalR()
        .AddJsonProtocol(options =>
        {
            options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
            options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));
        });

    // Configure OpenAPI with Scalar
    builder.Services.AddOpenApi(options =>
    {
        options.ShouldInclude = operation => operation.HttpMethod != null;
    });

    var app = builder.Build();

    // Seed achievements on startup if needed
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        var hasAchievements = await context.Achievements.AnyAsync();

        if (!hasAchievements)
        {
            Log.Information("Seeding achievements...");
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var result = await mediator.Send(new SeedAchievementsCommand());
            Log.Information("Seeded {Count} achievements", result.Value?.Total ?? 0);
        }
    }

    // Configure the HTTP request pipeline

    // Global exception handler
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
            if (error != null)
            {
                Log.Error(error.Error, "Unhandled exception");

                var response = new
                {
                    error = "An unexpected error occurred",
                    message = app.Environment.IsDevelopment() ? error.Error.Message : "Internal server error"
                };

                await context.Response.WriteAsJsonAsync(response);
            }
        });
    });

    // Request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "{RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

    app.UseHttpsRedirection();
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();

    // OpenAPI and Scalar UI (only in Development)
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.WithTheme(ScalarTheme.Mars).WithTitle("Snake Classic API");
        });
    }

    // Hangfire Dashboard (available in all environments with auth in production)
    var hangfireUsername = Environment.GetEnvironmentVariable("HANGFIRE_USERNAME") ?? "admin";
    var hangfirePassword = Environment.GetEnvironmentVariable("HANGFIRE_PASSWORD") ?? "admin";

    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new HangfireAuthorizationFilter(
            app.Environment.IsDevelopment(),
            hangfireUsername,
            hangfirePassword) }
    });

    // Configure Hangfire recurring jobs (matching APScheduler from Python)
    ConfigureRecurringJobs();

    // Map controllers and SignalR hub
    app.MapControllers();
    app.MapHub<GameHub>("/hubs/game");

    // Health check endpoint
    app.MapGet("/health", () => Results.Ok(new
    {
        status = "healthy",
        timestamp = DateTime.UtcNow,
        version = "1.0.0"
    }));

    // Root endpoint
    app.MapGet("/", (IWebHostEnvironment env) => env.IsDevelopment()
        ? Results.Redirect("/scalar/v1")
        : Results.Ok(new { name = "Snake Classic API", version = "1.0.0", status = "running" }));

    var port = Environment.GetEnvironmentVariable("API_PORT") ?? "8393";
    app.Urls.Add($"http://0.0.0.0:{port}");

    Log.Information("Snake Classic API started on port {Port}", port);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Configure recurring background jobs (equivalent to Python APScheduler)
static void ConfigureRecurringJobs()
{
    // Daily challenge reminder - runs at 9:00 AM every day
    RecurringJob.AddOrUpdate<INotificationJobService>(
        "daily-challenge-reminder",
        service => service.SendDailyChallengeReminder(),
        "0 9 * * *", // Cron: minute=0, hour=9, every day
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

    // Weekly leaderboard update - runs every Sunday at 6:00 PM
    RecurringJob.AddOrUpdate<INotificationJobService>(
        "weekly-leaderboard-update",
        service => service.SendWeeklyLeaderboardUpdate(),
        "0 18 * * 0", // Cron: minute=0, hour=18, day_of_week=0 (Sunday)
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

    // Retention campaign - runs at 2:00 PM every day
    RecurringJob.AddOrUpdate<INotificationJobService>(
        "retention-campaign",
        service => service.SendRetentionNotifications(),
        "0 14 * * *", // Cron: minute=0, hour=14, every day
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

    Log.Information("Hangfire recurring jobs configured successfully");
}

// Hangfire authorization filter for dashboard with Basic Auth
public class HangfireAuthorizationFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    private readonly bool _isDevelopment;
    private readonly string _username;
    private readonly string _password;

    public HangfireAuthorizationFilter(bool isDevelopment, string username, string password)
    {
        _isDevelopment = isDevelopment;
        _username = username;
        _password = password;
    }

    public bool Authorize(Hangfire.Dashboard.DashboardContext context)
    {
        // In development, allow all access without auth
        if (_isDevelopment)
            return true;

        // In production, require Basic Auth
        var httpContext = context.GetHttpContext();
        var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic "))
        {
            SetUnauthorizedResponse(httpContext);
            return false;
        }

        try
        {
            var encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
            var credentials = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
            var parts = credentials.Split(':', 2);

            if (parts.Length == 2 && parts[0] == _username && parts[1] == _password)
                return true;
        }
        catch
        {
            // Invalid base64 or other error
        }

        SetUnauthorizedResponse(httpContext);
        return false;
    }

    private static void SetUnauthorizedResponse(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = 401;
        httpContext.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Hangfire Dashboard\"";
    }
}
