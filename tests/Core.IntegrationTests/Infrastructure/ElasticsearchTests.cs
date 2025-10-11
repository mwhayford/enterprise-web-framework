// <copyright file="ElasticsearchTests.cs" company="Core">
// Copyright (c) Core. All rights reserved.
// </copyright>

namespace Core.IntegrationTests.Infrastructure;

using System.Threading.Tasks;
using Core.Application.DTOs;
using Core.Infrastructure.Services;
using DotNet.Testcontainers.Builders;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using NUnit.Framework;
using Testcontainers.Elasticsearch;

/// <summary>
/// Integration tests for Elasticsearch operations.
/// </summary>
[TestFixture]
public class ElasticsearchTests
{
    private ElasticsearchContainer? _elasticsearchContainer;
    private ElasticsearchService? _elasticsearchService;

    /// <summary>
    /// Performs one-time setup before all tests.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        // Start Elasticsearch container
        _elasticsearchContainer = new ElasticsearchBuilder()
            .WithImage("elasticsearch:8.11.3")
            .WithEnvironment("discovery.type", "single-node")
            .WithEnvironment("xpack.security.enabled", "false")
            .WithPortBinding(9200, 9200)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(9200)))
            .Build();

        await _elasticsearchContainer.StartAsync();

        // Create Elasticsearch client
        var settings = new ConnectionSettings(new System.Uri(_elasticsearchContainer.GetConnectionString()))
            .DefaultIndex("test");

        var client = new ElasticClient(settings);

        // Create service
        _elasticsearchService = new ElasticsearchService(client);
    }

    /// <summary>
    /// Performs cleanup after each test.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [TearDown]
    public async Task TearDown()
    {
        // Clean up indices
        if (_elasticsearchService != null)
        {
            try
            {
                await _elasticsearchService.DeleteIndexAsync("test");
            }
            catch
            {
                // Ignore errors during cleanup
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
        if (_elasticsearchContainer != null)
        {
            await _elasticsearchContainer.StopAsync();
            await _elasticsearchContainer.DisposeAsync();
        }
    }

    /// <summary>
    /// Tests that a document can be indexed in Elasticsearch.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Elasticsearch_ShouldIndexDocument()
    {
        // Arrange
        var document = new { Id = "1", Name = "Test Document", Content = "This is a test" };

        // Act
        await _elasticsearchService!.IndexDocumentAsync("test", "1", document);

        // Give Elasticsearch time to index
        await Task.Delay(1000);

        // Assert
        var result = await _elasticsearchService.SearchAsync<object>("test", "Test Document", 0, 10);
        result.Should().NotBeNull();
        result.TotalCount.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Tests that documents can be searched.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Elasticsearch_ShouldSearchDocuments()
    {
        // Arrange
        var doc1 = new { Id = "1", Name = "Test One", Content = "Content One" };
        var doc2 = new { Id = "2", Name = "Test Two", Content = "Content Two" };
        var doc3 = new { Id = "3", Name = "Different", Content = "Other Content" };

        await _elasticsearchService!.IndexDocumentAsync("test", "1", doc1);
        await _elasticsearchService.IndexDocumentAsync("test", "2", doc2);
        await _elasticsearchService.IndexDocumentAsync("test", "3", doc3);

        // Give Elasticsearch time to index
        await Task.Delay(1000);

        // Act
        var result = await _elasticsearchService.SearchAsync<object>("test", "Test", 0, 10);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
    }

    /// <summary>
    /// Tests that an index can be created and deleted.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Elasticsearch_ShouldCreateAndDeleteIndex()
    {
        // Arrange
        var indexName = "testindex";

        // Act - Create
        var createResult = await _elasticsearchService!.CreateIndexAsync(indexName);

        // Assert - Created
        createResult.Should().BeTrue();

        // Act - Delete
        var deleteResult = await _elasticsearchService.DeleteIndexAsync(indexName);

        // Assert - Deleted
        deleteResult.Should().BeTrue();
    }

    /// <summary>
    /// Tests that pagination works correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task Elasticsearch_ShouldPaginateResults()
    {
        // Arrange
        for (int i = 0; i < 5; i++)
        {
            var doc = new { Id = i.ToString(), Name = $"Document {i}", Content = "searchable" };
            await _elasticsearchService!.IndexDocumentAsync("test", i.ToString(), doc);
        }

        // Give Elasticsearch time to index
        await Task.Delay(1000);

        // Act - Page 1
        var page1 = await _elasticsearchService!.SearchAsync<object>("test", "searchable", 0, 2);

        // Act - Page 2
        var page2 = await _elasticsearchService.SearchAsync<object>("test", "searchable", 1, 2);

        // Assert
        page1.TotalCount.Should().Be(5);
        page1.Results.Should().HaveCount(2);
        page2.Results.Should().HaveCount(2);
        page1.CurrentPage.Should().Be(0);
        page2.CurrentPage.Should().Be(1);
    }
}

