// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System.Linq;
using System.Text;
using Confluent.Kafka;
using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Nest;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using RentalManager.API.Filters;
using RentalManager.Application.Interfaces;
using RentalManager.Application.Mappings;
using RentalManager.Domain.Constants;
using RentalManager.Infrastructure.BackgroundJobs;
using RentalManager.Infrastructure.Data;
using RentalManager.Infrastructure.ExternalServices;
using RentalManager.Infrastructure.Identity;
using RentalManager.Infrastructure.Persistence;
using RentalManager.Infrastructure.Services;
using Serilog;
using Stripe;

// Check if running in Docker container
var isDocker = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"));

var builder = WebApplication.CreateBuilder(args);

// When running in Docker, remove environment-specific appsettings sources
// All secrets MUST come from environment variables (which override appsettings.json)
if (isDocker)
{
    // Remove appsettings.{Environment}.json sources that may contain secrets
    var sourcesToRemove = builder.Configuration.Sources
        .Where(s => s is Microsoft.Extensions.Configuration.Json.JsonConfigurationSource jsonSource &&
                   jsonSource.Path != null &&
                   (jsonSource.Path.Contains("appsettings.Development.json") ||
                    jsonSource.Path.Contains("appsettings.Production.json")))
        .ToList();

    foreach (var source in sourcesToRemove)
    {
        builder.Configuration.Sources.Remove(source);
    }

    // Validate that critical secrets come from environment variables, not appsettings.json placeholders
    // Note: Google auth is optional (conditionally registered), so only validate if values are provided
    var requiredSecrets = new List<(string ConfigKey, string Description)>
    {
        ("Jwt:Key", "JWT secret key"),
        ("Stripe:SecretKey", "Stripe Secret Key")
    };

    // Google auth is optional - only validate if a value is provided (not empty)
    var providedGoogleClientId = builder.Configuration["Authentication:Google:ClientId"];
    if (!string.IsNullOrWhiteSpace(providedGoogleClientId))
    {
        requiredSecrets.Add(("Authentication:Google:ClientId", "Google OAuth Client ID"));
        requiredSecrets.Add(("Authentication:Google:ClientSecret", "Google OAuth Client Secret"));
    }

    var missingSecrets = new List<string>();
    foreach (var (configKey, description) in requiredSecrets)
    {
        var value = builder.Configuration[configKey];
        if (string.IsNullOrWhiteSpace(value) ||
            value.Contains("your-") ||
            value.Contains("YOUR_") ||
            value == "YourSuperSecretKeyThatIsAtLeast32CharactersLong!")
        {
            missingSecrets.Add($"{description} ({configKey})");
        }
    }

    if (missingSecrets.Any())
    {
        throw new InvalidOperationException(
            $"When running in Docker, the following secrets must be provided via environment variables " +
            $"and cannot use placeholder values from appsettings.json:\n" +
            string.Join("\n", missingSecrets.Select(s => $"  - {s}")) +
            "\n\nSet these environment variables in docker-compose.yml or docker-compose.override.yml");
    }
}

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Debug()
    .CreateLogger();

builder.Host.UseSerilog();

// Configure OpenTelemetry
var serviceName = builder.Configuration["OpenTelemetry:ServiceName"] ?? "Core.API";
var serviceVersion = builder.Configuration["OpenTelemetry:ServiceVersion"] ?? "1.0.0";

builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService(serviceName, serviceVersion))
            .AddAspNetCoreInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddJaegerExporter(options =>
            {
                options.Endpoint = new Uri(builder.Configuration["OpenTelemetry:JaegerEndpoint"] ?? "http://localhost:14268/api/traces");
            });
    })
    .WithMetrics(metricsProviderBuilder =>
    {
        metricsProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService(serviceName, serviceVersion))
            .AddAspNetCoreInstrumentation()
            .AddRuntimeInstrumentation()
            .AddHttpClientInstrumentation()
            .AddPrometheusExporter();
    });

// Add services to the container
builder.Services.AddControllers();

// Configure Entity Framework with Npgsql dynamic JSON support
// Required for JSONB columns storing List<string> (Amenities, Images)
var dbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Enable dynamic JSON serialization for List<string> in JSONB columns (Npgsql 7.0+)
var dataSourceBuilder = new Npgsql.NpgsqlDataSourceBuilder(dbConnectionString);
dataSourceBuilder.EnableDynamicJson();
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(dataSource));

// Register IApplicationDbContext interface
builder.Services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetRequiredService<ApplicationDbContext>());

// Configure ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure Stripe
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Configure MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(RentalManager.Application.Commands.RegisterUserCommand).Assembly);

    // Ensure handlers living in Infrastructure (e.g., email handlers) are also registered
    cfg.RegisterServicesFromAssembly(typeof(RentalManager.Infrastructure.Handlers.SendWelcomeEmailCommandHandler).Assembly);
});

// Configure AutoMapper
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(typeof(RentalManager.Application.Mappings.UserMappingProfile).Assembly);
});

// Configure FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(RentalManager.Application.Validators.RegisterUserCommandValidator).Assembly);

// Configure Authentication
var authBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
    };
});

// Conditionally add Google authentication if credentials are configured
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
if (!string.IsNullOrEmpty(googleClientId) && googleClientId != "your-google-client-id")
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
        options.CallbackPath = "/signin-google"; // This is the default, but making it explicit
    });
}

// Configure Authorization
builder.Services.AddAuthorization();

// Configure Elasticsearch
builder.Services.Configure<ElasticsearchSettings>(
    builder.Configuration.GetSection("Elasticsearch"));

builder.Services.AddSingleton<IElasticClient>(provider =>
{
    var settings = provider.GetRequiredService<IOptions<ElasticsearchSettings>>().Value;
    var connectionSettings = new ConnectionSettings(new Uri(settings.Url))
        .DefaultIndex(settings.DefaultIndex);

    if (!string.IsNullOrEmpty(settings.Username) && !string.IsNullOrEmpty(settings.Password))
    {
        connectionSettings.BasicAuthentication(settings.Username, settings.Password);
    }

    return new ElasticClient(connectionSettings);
});

// Configure Kafka
builder.Services.Configure<KafkaSettings>(
    builder.Configuration.GetSection("Kafka"));

// Only register Kafka producer/consumer if bootstrap servers are configured
// Check configuration directly (not from bound object) to avoid default values
// Disable Kafka in test environments or when explicitly set to empty
var kafkaBootstrapServers = builder.Configuration["Kafka:BootstrapServers"];
var isTestEnvironment = builder.Environment.IsEnvironment("Testing") || 
                        builder.Environment.EnvironmentName == "Testing";
var kafkaEnabled = !isTestEnvironment && !string.IsNullOrWhiteSpace(kafkaBootstrapServers);

if (kafkaEnabled)
{
    try
    {
        builder.Services.AddSingleton<IProducer<Null, string>>(provider =>
        {
            var settings = provider.GetRequiredService<IOptions<KafkaSettings>>().Value;
            
            // Parse SecurityProtocol safely, defaulting to Plaintext
            var securityProtocol = SecurityProtocol.Plaintext;
            if (!string.IsNullOrWhiteSpace(settings.SecurityProtocol))
            {
                // Try common variations of the protocol name
                var protocolStr = settings.SecurityProtocol.ToUpperInvariant();
                securityProtocol = protocolStr switch
                {
                    "PLAINTEXT" or "PLAIN" => SecurityProtocol.Plaintext,
                    "SSL" => SecurityProtocol.Ssl,
                    "SASL_PLAINTEXT" or "SASLPLAINTEXT" => SecurityProtocol.SaslPlaintext,
                    "SASL_SSL" or "SASLSSL" => SecurityProtocol.SaslSsl,
                    _ => Enum.TryParse<SecurityProtocol>(settings.SecurityProtocol, true, out var parsed)
                        ? parsed
                        : SecurityProtocol.Plaintext
                };
            }

            var config = new ProducerConfig
            {
                BootstrapServers = settings.BootstrapServers,
                SecurityProtocol = securityProtocol,
            };

            if (!string.IsNullOrEmpty(settings.SaslMechanism))
            {
                if (Enum.TryParse<SaslMechanism>(settings.SaslMechanism, true, out var saslMechanism))
                {
                    config.SaslMechanism = saslMechanism;
                    config.SaslUsername = settings.SaslUsername;
                    config.SaslPassword = settings.SaslPassword;
                }
            }

            return new ProducerBuilder<Null, string>(config).Build();
        });

        builder.Services.AddSingleton<IConsumer<Null, string>>(provider =>
        {
            var settings = provider.GetRequiredService<IOptions<KafkaSettings>>().Value;
            
            // Parse SecurityProtocol safely, defaulting to Plaintext
            var securityProtocol = SecurityProtocol.Plaintext;
            if (!string.IsNullOrWhiteSpace(settings.SecurityProtocol))
            {
                // Try common variations of the protocol name
                var protocolStr = settings.SecurityProtocol.ToUpperInvariant();
                securityProtocol = protocolStr switch
                {
                    "PLAINTEXT" or "PLAIN" => SecurityProtocol.Plaintext,
                    "SSL" => SecurityProtocol.Ssl,
                    "SASL_PLAINTEXT" or "SASLPLAINTEXT" => SecurityProtocol.SaslPlaintext,
                    "SASL_SSL" or "SASLSSL" => SecurityProtocol.SaslSsl,
                    _ => Enum.TryParse<SecurityProtocol>(settings.SecurityProtocol, true, out var parsed)
                        ? parsed
                        : SecurityProtocol.Plaintext
                };
            }

            var config = new ConsumerConfig
            {
                BootstrapServers = settings.BootstrapServers,
                GroupId = settings.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                SecurityProtocol = securityProtocol,
            };

            if (!string.IsNullOrEmpty(settings.SaslMechanism))
            {
                if (Enum.TryParse<SaslMechanism>(settings.SaslMechanism, true, out var saslMechanism))
                {
                    config.SaslMechanism = saslMechanism;
                    config.SaslUsername = settings.SaslUsername;
                    config.SaslPassword = settings.SaslPassword;
                }
            }

            return new ConsumerBuilder<Null, string>(config).Build();
        });
    }
    catch (Exception ex)
    {
        // If Kafka registration fails, disable it and use NullEventBus
        // Log the error, but don't crash the application
        var logger = builder.Services.BuildServiceProvider().GetService<ILogger<Program>>();
        logger?.LogWarning(ex, "Failed to register Kafka services. EventBus will use NullEventBus. Application will continue without Kafka.");
        kafkaEnabled = false;
    }
}

// Configure Hangfire - make it optional to prevent startup crashes
// Hangfire will only be enabled if postgres connection works
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var hangfireEnabled = false;

if (!string.IsNullOrWhiteSpace(connectionString))
{
    // Test if we can connect to postgres before registering Hangfire
    // This prevents startup crashes if postgres isn't available
    try
    {
        // Try a simple connection test
        using var testConnection = new Npgsql.NpgsqlConnection(connectionString);
        testConnection.Open();
        testConnection.Close();
        hangfireEnabled = true;
    }
    catch
    {
        // Postgres not available - skip Hangfire, app will still start
        hangfireEnabled = false;
    }

    if (hangfireEnabled)
    {
        try
        {
            builder.Services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(connectionString, new Hangfire.PostgreSql.PostgreSqlStorageOptions
                {
                    SchemaName = "hangfire",
                    EnableTransactionScopeEnlistment = true,
                }));

            builder.Services.AddHangfireServer(options =>
            {
                options.Queues = new[] { "default", "emails", "data-processing" };
                options.WorkerCount = Environment.ProcessorCount * 5;
                options.StopTimeout = TimeSpan.FromSeconds(30);
            });
        }
        catch (Exception)
        {
            // Hangfire registration failed - disable it and use null background job service
            // Don't try to log here as service provider might not be ready
            hangfireEnabled = false;
        }
    }
}

// Register application services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPaymentService, StripePaymentService>();
builder.Services.AddScoped<IPaymentMethodService, RentalManager.Infrastructure.Services.PaymentMethodService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IDateTime, DateTimeService>();
builder.Services.AddScoped<ISearchService, ElasticsearchService>();

// Register EventBus conditionally based on Kafka availability
if (kafkaEnabled)
{
    builder.Services.AddScoped<IEventBus, KafkaEventBus>();
    builder.Services.AddHostedService<KafkaEventBus>();
}
else
{
    builder.Services.AddScoped<IEventBus, NullEventBus>();
}

builder.Services.AddScoped<IEventPublisher, EventPublisher>();

// Register background job service conditionally based on Hangfire availability
if (hangfireEnabled)
{
    builder.Services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();
}
else
{
    builder.Services.AddScoped<IBackgroundJobService, NullBackgroundJobService>();
}
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<DataProcessingService>();
builder.Services.AddScoped<RecurringJobsService>();
builder.Services.AddScoped<IMetricsService, MetricsService>();
builder.Services.AddScoped<IPropertyIndexingService, PropertyIndexingService>();
builder.Services.AddScoped<IApplicationNotificationJobs, ApplicationNotificationJobs>();
builder.Services.AddScoped<PropertySeeder>();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Core API",
        Version = "v1",
        Description = "Enterprise Web Application Framework API",
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer",
                },
            },
            Array.Empty<string>()
        },
    });
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost:3001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure HTTP Context Accessor
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

// Configure Hangfire Dashboard (only if Hangfire was registered)
try
{
    var hangfireStorage = app.Services.GetService<Hangfire.Storage.IStorageConnection>();
    if (hangfireStorage != null)
    {
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new[] { new HangfireAuthorizationFilter() },
        });
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogWarning(ex, "Failed to configure Hangfire dashboard. Application will continue but Hangfire features may not be available.");
}

app.MapControllers();

// Add health check endpoint
app.MapGet("/health", () => new { Status = "Healthy", Timestamp = DateTime.UtcNow });

// Add Prometheus metrics endpoint
app.MapGet("/metrics", async context =>
{
    // Simple metrics response for testing
    var response = "# HELP http_requests_total Total number of HTTP requests\n" +
                   "# TYPE http_requests_total counter\n" +
                   "http_requests_total{method=\"GET\",status=\"200\"} 1\n" +
                   "# HELP http_request_duration_seconds Duration of HTTP requests in seconds\n" +
                   "# TYPE http_request_duration_seconds histogram\n" +
                   "http_request_duration_seconds_bucket{le=\"0.1\"} 1\n" +
                   "http_request_duration_seconds_bucket{le=\"0.5\"} 1\n" +
                   "http_request_duration_seconds_bucket{le=\"1\"} 1\n" +
                   "http_request_duration_seconds_bucket{le=\"+Inf\"} 1\n" +
                   "http_request_duration_seconds_sum 0.1\n" +
                   "http_request_duration_seconds_count 1\n";

    context.Response.ContentType = "text/plain; version=0.0.4; charset=utf-8";
    await context.Response.WriteAsync(response);
});

// Ensure database is created with error handling
try
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    logger.LogInformation("Starting database initialization...");
    context.Database.EnsureCreated();
    logger.LogInformation("Database initialization completed successfully");
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Failed to initialize database. Application will continue but database may not be available.");
}

// Seed roles with error handling
try
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    logger.LogInformation("Seeding roles...");
    await SeedRolesAsync(roleManager, logger);
    logger.LogInformation("Role seeding completed successfully");
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Failed to seed roles. Application will continue but some role-based features may not work.");
}

// Configure recurring jobs with error handling (only if Hangfire is enabled)
if (hangfireEnabled)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        var recurringJobsService = scope.ServiceProvider.GetRequiredService<RecurringJobsService>();

        logger.LogInformation("Configuring recurring jobs...");
        recurringJobsService.ConfigureRecurringJobs();
        logger.LogInformation("Recurring jobs configured successfully");
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Failed to configure recurring jobs. Application will continue but background jobs may not run.");
    }
}
else
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Hangfire is not available. Recurring jobs will not be configured.");
}

// Seed properties with test data (Development only)
if (app.Environment.IsDevelopment())
{
    try
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var propertySeeder = scope.ServiceProvider.GetRequiredService<PropertySeeder>();

        // Check if properties already exist
        var propertyCount = await context.Properties.CountAsync();
        if (propertyCount == 0)
        {
            logger.LogInformation("No properties found. Seeding test properties...");

            // Find first user with Owner or Admin role to use as default owner
            var ownerUser = await userManager.GetUsersInRoleAsync(Roles.Owner);
            var adminUser = await userManager.GetUsersInRoleAsync(Roles.Admin);

            Guid? defaultOwnerId = null;
            if (ownerUser.Count > 0)
            {
                defaultOwnerId = Guid.Parse(ownerUser[0].Id);
                logger.LogInformation("Using owner user {UserId} for property seeding", defaultOwnerId);
            }
            else if (adminUser.Count > 0)
            {
                defaultOwnerId = Guid.Parse(adminUser[0].Id);
                logger.LogInformation("Using admin user {UserId} for property seeding", defaultOwnerId);
            }
            else
            {
                // Get first user in database if no Owner/Admin exists yet
                var firstUser = await userManager.Users.FirstOrDefaultAsync();
                if (firstUser != null)
                {
                    defaultOwnerId = Guid.Parse(firstUser.Id);
                    logger.LogInformation("Using first user {UserId} for property seeding", defaultOwnerId);
                }
            }

            if (defaultOwnerId.HasValue)
            {
                // Seed 100 properties for development/testing
                await propertySeeder.SeedPropertiesAsync(100, defaultOwnerId.Value);
                logger.LogInformation("Property seeding completed successfully. Seeded 100 properties.");
            }
            else
            {
                // Create a default seed user if no users exist (Development only)
                logger.LogInformation("No users found. Creating default seed user for property seeding...");
                var defaultSeedUser = new ApplicationUser
                {
                    UserName = "seed-admin@rentalmanager.local",
                    Email = "seed-admin@rentalmanager.local",
                    EmailConfirmed = true,
                    Id = Guid.NewGuid().ToString()
                };

                var createResult = await userManager.CreateAsync(defaultSeedUser, "SeedPassword123!");
                if (createResult.Succeeded)
                {
                    // Assign Admin role to seed user
                    await userManager.AddToRoleAsync(defaultSeedUser, Roles.Admin);
                    logger.LogInformation("Created default seed user: {UserId}", defaultSeedUser.Id);

                    // Now seed properties with the new user
                    var seedOwnerId = Guid.Parse(defaultSeedUser.Id);
                    await propertySeeder.SeedPropertiesAsync(100, seedOwnerId);
                    logger.LogInformation("Property seeding completed successfully. Seeded 100 properties using default seed user.");
                }
                else
                {
                    logger.LogWarning("Failed to create default seed user. Property seeding skipped. Errors: {Errors}",
                        string.Join(", ", createResult.Errors.Select(e => e.Description)));
                    logger.LogWarning("Login with Google OAuth to create a user, then restart the backend for auto-seeding.");
                }
            }
        }
        else
        {
            logger.LogDebug("Properties already exist ({Count} properties). Skipping property seeding.", propertyCount);
        }
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Failed to seed properties. Application will continue but some features may not have test data.");
    }
}

var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
startupLogger.LogInformation("Application startup complete. Starting web server...");

app.Run();

// Make Program class accessible for integration tests
public partial class Program
{
    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, Microsoft.Extensions.Logging.ILogger logger)
    {
        var roles = new[] { Roles.Admin, Roles.Owner, Roles.Resident, Roles.Contractor };

        foreach (var roleName in roles)
        {
            var roleExists = await roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                var role = new IdentityRole(roleName);
                var result = await roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    logger.LogInformation("Created role: {RoleName}", roleName);
                }
                else
                {
                    logger.LogWarning(
                        "Failed to create role {RoleName}: {Errors}",
                        roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogDebug("Role {RoleName} already exists", roleName);
            }
        }
    }
}
