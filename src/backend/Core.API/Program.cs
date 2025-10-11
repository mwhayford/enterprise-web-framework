using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using System.Text;
using Serilog;
using FluentValidation;
using Core.Infrastructure.Persistence;
using Core.Infrastructure.Identity;
using Core.Application.Interfaces;
using Core.Infrastructure.Services;
using Core.Infrastructure.ExternalServices;
using Core.Application.Mappings;
using Stripe;
using Nest;
using Confluent.Kafka;
using Hangfire;
using Hangfire.PostgreSql;
using Core.Infrastructure.Services;
using Core.API.Filters;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Debug()
    .CreateLogger();

builder.Host.UseSerilog();

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
builder.Services.AddMediatR(cfg => {
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Google:ClientId"]!;
    options.ClientSecret = builder.Configuration["Google:ClientSecret"]!;
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
        SecurityProtocol = Enum.Parse<SecurityProtocol>(settings.SecurityProtocol, true)
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
        SecurityProtocol = Enum.Parse<SecurityProtocol>(settings.SecurityProtocol, true)
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
    .UsePostgreSqlStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

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
builder.Services.AddHostedService<KafkaEventBus>();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Core API", 
        Version = "v1",
        Description = "Enterprise Web Application Framework API"
    });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
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
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

app.MapControllers();

// Add health check endpoint
app.MapGet("/health", () => new { Status = "Healthy", Timestamp = DateTime.UtcNow });

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}

// Configure recurring jobs
using (var scope = app.Services.CreateScope())
{
    var recurringJobsService = scope.ServiceProvider.GetRequiredService<RecurringJobsService>();
    recurringJobsService.ConfigureRecurringJobs();
}

app.Run();