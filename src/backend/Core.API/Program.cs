// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System.Text;
using Confluent.Kafka;
using Core.API.Filters;
using Core.Application.Interfaces;
using Core.Application.Mappings;
using Core.Infrastructure.ExternalServices;
using Core.Infrastructure.Identity;
using Core.Infrastructure.Persistence;
using Core.Infrastructure.Services;
using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Nest;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

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

// Configure Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
    cfg.RegisterServicesFromAssembly(typeof(Core.Application.Commands.RegisterUserCommand).Assembly);
});

// Configure AutoMapper
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(typeof(Core.Application.Mappings.UserMappingProfile).Assembly);
});

// Configure FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Core.Application.Validators.RegisterUserCommandValidator).Assembly);

// Configure Authentication
builder.Services.AddAuthentication(options =>
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
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    options.CallbackPath = "/signin-google"; // This is the default, but making it explicit
});

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

builder.Services.AddSingleton<IProducer<Null, string>>(provider =>
{
    var settings = provider.GetRequiredService<IOptions<KafkaSettings>>().Value;
    var config = new ProducerConfig
    {
        BootstrapServers = settings.BootstrapServers,
        SecurityProtocol = Enum.Parse<SecurityProtocol>(settings.SecurityProtocol, true),
    };

    if (!string.IsNullOrEmpty(settings.SaslMechanism))
    {
        config.SaslMechanism = Enum.Parse<SaslMechanism>(settings.SaslMechanism, true);
        config.SaslUsername = settings.SaslUsername;
        config.SaslPassword = settings.SaslPassword;
    }

    return new ProducerBuilder<Null, string>(config).Build();
});

builder.Services.AddSingleton<IConsumer<Null, string>>(provider =>
{
    var settings = provider.GetRequiredService<IOptions<KafkaSettings>>().Value;
    var config = new ConsumerConfig
    {
        BootstrapServers = settings.BootstrapServers,
        GroupId = settings.GroupId,
        AutoOffsetReset = AutoOffsetReset.Earliest,
        EnableAutoCommit = false,
        SecurityProtocol = Enum.Parse<SecurityProtocol>(settings.SecurityProtocol, true),
    };

    if (!string.IsNullOrEmpty(settings.SaslMechanism))
    {
        config.SaslMechanism = Enum.Parse<SaslMechanism>(settings.SaslMechanism, true);
        config.SaslUsername = settings.SaslUsername;
        config.SaslPassword = settings.SaslPassword;
    }

    return new ConsumerBuilder<Null, string>(config).Build();
});

// Configure Hangfire
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options =>
    {
        options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")!);
    }));

builder.Services.AddHangfireServer(options =>
{
    options.Queues = new[] { "default", "emails", "data-processing" };
    options.WorkerCount = Environment.ProcessorCount * 5;
});

// Register application services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPaymentService, StripePaymentService>();
builder.Services.AddScoped<IPaymentMethodService, Core.Infrastructure.Services.PaymentMethodService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IDateTime, DateTimeService>();
builder.Services.AddScoped<ISearchService, ElasticsearchService>();
builder.Services.AddScoped<IEventBus, KafkaEventBus>();
builder.Services.AddScoped<IEventPublisher, EventPublisher>();
builder.Services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<DataProcessingService>();
builder.Services.AddScoped<RecurringJobsService>();
builder.Services.AddScoped<IMetricsService, MetricsService>();
builder.Services.AddHostedService<KafkaEventBus>();

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
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
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

// Configure Hangfire Dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() },
});

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

// Configure recurring jobs with error handling
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

var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
startupLogger.LogInformation("Application startup complete. Starting web server...");

app.Run();
