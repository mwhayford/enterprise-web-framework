// <copyright file="RedisTests.cs" company="Core">
// Copyright (c) Core. All rights reserved.
// </copyright>

namespace Core.IntegrationTests.Infrastructure;

using System;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using FluentAssertions;
using NUnit.Framework;
using StackExchange.Redis;
using Testcontainers.Redis;

/// <summary>
/// Integration tests for Redis caching operations.
/// </summary>
[TestFixture]
public class RedisTests
{
    private RedisContainer? _redisContainer;
    private IConnectionMultiplexer? _redis;
    private IDatabase? _database;

    /// <summary>
    /// Performs one-time setup before all tests.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        // Start Redis container
        _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithPortBinding(6381, 6379)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(6379))
            .Build();

        await _redisContainer.StartAsync();

        // Create Redis connection
        _redis = await ConnectionMultiplexer.ConnectAsync(_redisContainer.GetConnectionString());
        _database = _redis.GetDatabase();
    }

    /// <summary>
    /// Performs cleanup after each test.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [TearDown]
    public async Task TearDown()
    {
        // Flush database between tests
        if (_redis != null)
        {
            var endpoints = _redis.GetEndPoints();
            if (endpoints.Length > 0)
            {
                var server = _redis.GetServer(endpoints[0]);
                await server.FlushDatabaseAsync();
            }
        }
    }

    /// <summary>
    /// Performs one-time cleanup after all tests.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        _redis?.Dispose();

        if (_redisContainer != null)
        {
            await _redisContainer.StopAsync();
            await _redisContainer.DisposeAsync();
        }
    }

    /// <summary>
    /// Tests that a string value can be set and retrieved.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Redis_ShouldSetAndGetString()
    {
        // Arrange
        var key = "test:key";
        var value = "test value";

        // Act
        await _database!.StringSetAsync(key, value);
        var retrieved = await _database.StringGetAsync(key);

        // Assert
        retrieved.ToString().Should().Be(value);
    }

    /// <summary>
    /// Tests that a value can be set with expiration.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Redis_ShouldSetWithExpiration()
    {
        // Arrange
        var key = "test:expiring";
        var value = "will expire";
        var expiration = TimeSpan.FromSeconds(2);

        // Act
        await _database!.StringSetAsync(key, value, expiration);
        var immediate = await _database.StringGetAsync(key);

        await Task.Delay(2500); // Wait for expiration

        var afterExpiration = await _database.StringGetAsync(key);

        // Assert
        immediate.ToString().Should().Be(value);
        afterExpiration.IsNullOrEmpty.Should().BeTrue();
    }

    /// <summary>
    /// Tests that a key can be deleted.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Redis_ShouldDeleteKey()
    {
        // Arrange
        var key = "test:delete";
        var value = "to be deleted";
        await _database!.StringSetAsync(key, value);

        // Act
        var deleted = await _database.KeyDeleteAsync(key);
        var retrieved = await _database.StringGetAsync(key);

        // Assert
        deleted.Should().BeTrue();
        retrieved.IsNullOrEmpty.Should().BeTrue();
    }

    /// <summary>
    /// Tests that multiple values can be set using a hash.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Redis_ShouldHandleHashValues()
    {
        // Arrange
        var key = "test:hash";
        var hashEntries = new[]
        {
            new HashEntry("field1", "value1"),
            new HashEntry("field2", "value2"),
            new HashEntry("field3", "value3"),
        };

        // Act
        await _database!.HashSetAsync(key, hashEntries);
        var field1 = await _database.HashGetAsync(key, "field1");
        var field2 = await _database.HashGetAsync(key, "field2");
        var allFields = await _database.HashGetAllAsync(key);

        // Assert
        field1.ToString().Should().Be("value1");
        field2.ToString().Should().Be("value2");
        allFields.Should().HaveCount(3);
    }

    /// <summary>
    /// Tests that a list can be used as a queue.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Redis_ShouldHandleListAsQueue()
    {
        // Arrange
        var key = "test:queue";

        // Act - Push items
        await _database!.ListRightPushAsync(key, "item1");
        await _database.ListRightPushAsync(key, "item2");
        await _database.ListRightPushAsync(key, "item3");

        // Act - Pop items
        var item1 = await _database.ListLeftPopAsync(key);
        var item2 = await _database.ListLeftPopAsync(key);
        var length = await _database.ListLengthAsync(key);

        // Assert
        item1.ToString().Should().Be("item1");
        item2.ToString().Should().Be("item2");
        length.Should().Be(1); // item3 still in queue
    }

    /// <summary>
    /// Tests that a set can store unique values.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Redis_ShouldHandleSets()
    {
        // Arrange
        var key = "test:set";

        // Act
        await _database!.SetAddAsync(key, "value1");
        await _database.SetAddAsync(key, "value2");
        await _database.SetAddAsync(key, "value1"); // Duplicate

        var members = await _database.SetMembersAsync(key);
        var contains = await _database.SetContainsAsync(key, "value1");

        // Assert
        members.Should().HaveCount(2); // Duplicates removed
        contains.Should().BeTrue();
    }

    /// <summary>
    /// Tests that a counter can be incremented atomically.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Redis_ShouldIncrementCounter()
    {
        // Arrange
        var key = "test:counter";

        // Act
        var value1 = await _database!.StringIncrementAsync(key);
        var value2 = await _database.StringIncrementAsync(key);
        var value3 = await _database.StringIncrementAsync(key, 5);

        // Assert
        value1.Should().Be(1);
        value2.Should().Be(2);
        value3.Should().Be(7);
    }
}

