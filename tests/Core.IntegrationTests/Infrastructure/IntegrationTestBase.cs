// <copyright file="IntegrationTestBase.cs" company="Core">
// Copyright (c) Core. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using Core.Infrastructure.Persistence;
using DotNet.Testcontainers.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using NUnit.Framework;
using Respawn;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace Core.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for integration tests that provides common infrastructure setup.
/// </summary>
[TestFixture]
public abstract class IntegrationTestBase
{
    private PostgreSqlContainer? _postgresContainer;
    private RedisContainer? _redisContainer;
    private Respawner? _respawner;

    /// <summary>
    /// Gets the service provider for dependency injection.
    /// </summary>
    protected IServiceProvider? ServiceProvider { get; private set; }

    /// <summary>
    /// Gets the database context.
    /// </summary>
    protected ApplicationDbContext? DbContext { get; private set; }

    /// <summary>
    /// Performs one-time setup before all tests in the fixture.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        // Start PostgreSQL container
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .WithPortBinding(5434, 5432)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .Build();

        await _postgresContainer.StartAsync();

        // Start Redis container
        _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithPortBinding(6381, 6379)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(6379))
            .Build();

        await _redisContainer.StartAsync();

        // Setup services
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();

        // Get DbContext and apply migrations
        DbContext = ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await DbContext.Database.MigrateAsync();

        // Initialize Respawner for database cleanup between tests
        await using var connection = new NpgsqlConnection(_postgresContainer.GetConnectionString());
        await connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(
            connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = new[] { "public" },
                TablesToIgnore = new Respawn.Graph.Table[] { "__EFMigrationsHistory" },
            });
    }

    /// <summary>
    /// Performs setup before each test.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [SetUp]
    public async Task SetUp()
    {
        if (_respawner != null && _postgresContainer != null)
        {
            await using var connection = new NpgsqlConnection(_postgresContainer.GetConnectionString());
            await connection.OpenAsync();
            await _respawner.ResetAsync(connection);
        }
    }

    /// <summary>
    /// Performs one-time cleanup after all tests in the fixture.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (DbContext != null)
        {
            await DbContext.DisposeAsync();
        }

        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        if (_postgresContainer != null)
        {
            await _postgresContainer.StopAsync();
            await _postgresContainer.DisposeAsync();
        }

        if (_redisContainer != null)
        {
            await _redisContainer.StopAsync();
            await _redisContainer.DisposeAsync();
        }
    }

    /// <summary>
    /// Gets the PostgreSQL connection string.
    /// </summary>
    /// <returns>The connection string.</returns>
    protected string GetPostgresConnectionString()
    {
        return _postgresContainer?.GetConnectionString() ?? string.Empty;
    }

    /// <summary>
    /// Gets the Redis connection string.
    /// </summary>
    /// <returns>The connection string.</returns>
    protected string GetRedisConnectionString()
    {
        return _redisContainer?.GetConnectionString() ?? string.Empty;
    }

    /// <summary>
    /// Configures services for dependency injection.
    /// Override this method to add custom services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    protected virtual void ConfigureServices(IServiceCollection services)
    {
        // Register ApplicationDbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(GetPostgresConnectionString()));
    }
}
