// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using RentalManager.Domain.Entities;

namespace RentalManager.Infrastructure.Services;

public interface IPropertyIndexingService
{
    Task IndexPropertyAsync(Property property);

    Task RemovePropertyAsync(Guid propertyId);

    Task ReindexAllPropertiesAsync(IEnumerable<Property> properties);
}

public class PropertyIndexingService : IPropertyIndexingService
{
    private readonly IElasticClient _client;
    private readonly ILogger<PropertyIndexingService> _logger;
    private const string IndexName = "properties";

    public PropertyIndexingService(
        IConfiguration configuration,
        ILogger<PropertyIndexingService> logger)
    {
        var elasticUri = configuration["Elasticsearch:Uri"] ?? "http://localhost:9200";
        var settings = new ConnectionSettings(new Uri(elasticUri))
            .DefaultIndex(IndexName);

        _client = new ElasticClient(settings);
        _logger = logger;
    }

    public async Task IndexPropertyAsync(Property property)
    {
        try
        {
            var document = new PropertySearchDocument
            {
                Id = property.Id.ToString(),
                OwnerId = property.OwnerId.ToString(),
                Street = property.Address.Street,
                Unit = property.Address.Unit,
                City = property.Address.City,
                State = property.Address.State,
                ZipCode = property.Address.ZipCode,
                Country = property.Address.Country,
                FullAddress = $"{property.Address.Street}, {property.Address.City}, {property.Address.State} {property.Address.ZipCode}",
                PropertyType = property.PropertyType.ToString(),
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                SquareFeet = property.SquareFeet,
                MonthlyRent = (double)property.MonthlyRent.Amount,
                RentCurrency = property.MonthlyRent.Currency,
                SecurityDeposit = (double)property.SecurityDeposit.Amount,
                SecurityDepositCurrency = property.SecurityDeposit.Currency,
                AvailableDate = property.AvailableDate,
                Status = property.Status.ToString(),
                Description = property.Description,
                Amenities = property.Amenities.ToList(),
                ApplicationFee = property.ApplicationFee?.Amount != null ? (double)property.ApplicationFee.Amount : null,
                ApplicationFeeCurrency = property.ApplicationFee?.Currency,
                CreatedAt = property.CreatedAt,
                UpdatedAt = property.UpdatedAt,
            };

            var response = await _client.IndexDocumentAsync(document);

            if (!response.IsValid)
            {
                _logger.LogError("Failed to index property {PropertyId}: {Error}", property.Id, response.ServerError);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing property {PropertyId}", property.Id);
        }
    }

    public async Task RemovePropertyAsync(Guid propertyId)
    {
        try
        {
            var response = await _client.DeleteAsync<PropertySearchDocument>(propertyId.ToString(), idx => idx.Index(IndexName));

            if (!response.IsValid)
            {
                _logger.LogError("Failed to remove property {PropertyId} from index: {Error}", propertyId, response.ServerError);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing property {PropertyId} from index", propertyId);
        }
    }

    public async Task ReindexAllPropertiesAsync(IEnumerable<Property> properties)
    {
        try
        {
            // Delete existing index
            await _client.Indices.DeleteAsync(IndexName);

            // Create new index with mappings
            await _client.Indices.CreateAsync(IndexName);

            // Bulk index properties
            var documents = properties.Select(p => new PropertySearchDocument
            {
                Id = p.Id.ToString(),
                OwnerId = p.OwnerId.ToString(),
                Street = p.Address.Street,
                Unit = p.Address.Unit,
                City = p.Address.City,
                State = p.Address.State,
                ZipCode = p.Address.ZipCode,
                Country = p.Address.Country,
                FullAddress = $"{p.Address.Street}, {p.Address.City}, {p.Address.State} {p.Address.ZipCode}",
                PropertyType = p.PropertyType.ToString(),
                Bedrooms = p.Bedrooms,
                Bathrooms = p.Bathrooms,
                SquareFeet = p.SquareFeet,
                MonthlyRent = (double)p.MonthlyRent.Amount,
                RentCurrency = p.MonthlyRent.Currency,
                SecurityDeposit = (double)p.SecurityDeposit.Amount,
                SecurityDepositCurrency = p.SecurityDeposit.Currency,
                AvailableDate = p.AvailableDate,
                Status = p.Status.ToString(),
                Description = p.Description,
                Amenities = p.Amenities.ToList(),
                ApplicationFee = p.ApplicationFee?.Amount != null ? (double)p.ApplicationFee.Amount : null,
                ApplicationFeeCurrency = p.ApplicationFee?.Currency,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
            }).ToList();

            var bulkResponse = await _client.BulkAsync(b => b
                .Index(IndexName)
                .IndexMany(documents));

            if (!bulkResponse.IsValid)
            {
                _logger.LogError("Failed to bulk index properties: {Error}", bulkResponse.ServerError);
            }

            _logger.LogInformation("Reindexed {Count} properties", documents.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reindexing properties");
        }
    }
}

public class PropertySearchDocument
{
    public string Id { get; set; } = string.Empty;

    public string OwnerId { get; set; } = string.Empty;

    public string Street { get; set; } = string.Empty;

    public string? Unit { get; set; }

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string ZipCode { get; set; } = string.Empty;

    public string? Country { get; set; }

    public string FullAddress { get; set; } = string.Empty;

    public string PropertyType { get; set; } = string.Empty;

    public int Bedrooms { get; set; }

    public int Bathrooms { get; set; }

    public decimal SquareFeet { get; set; }

    public double MonthlyRent { get; set; }

    public string RentCurrency { get; set; } = string.Empty;

    public double SecurityDeposit { get; set; }

    public string SecurityDepositCurrency { get; set; } = string.Empty;

    public DateTime AvailableDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public List<string> Amenities { get; set; } = new();

    public double? ApplicationFee { get; set; }

    public string? ApplicationFeeCurrency { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
