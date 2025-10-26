# API Versioning Strategy

## Overview

This document outlines the API versioning strategy for the RentalManager API. It covers how versions are managed, how changes are classified, and the lifecycle of API versions.

## Versioning Approach

RentalManager uses **URL path versioning** for API endpoints:

```
https://api.rentalmanager.com/v1/properties
https://api.rentalmanager.com/v2/properties
```

### Current Version

**Current API Version**: v1.0

All endpoints are currently under the `/api` prefix (implicitly v1). Future versions will use explicit versioning.

## Version Lifecycle

### Supported Versions

- **Current Version (v1.0)**: Actively maintained and enhanced
- **Previous Versions**: Supported for 12 months after deprecation
- **Deprecated Versions**: Receiving security updates only

### Version Support Timeline

```
v1.0 (Current)
  │
  ├─ Active: 0-12 months (new features, bug fixes)
  │
  ├─ Deprecated: 12-24 months (security updates only)
  │
  └─ Unsupported: 24+ months (no updates)
```

## Change Classification

### Non-Breaking Changes

Changes that maintain backward compatibility:

1. **New Endpoints**: Adding new API endpoints
2. **New Optional Parameters**: Adding optional query/path parameters
3. **New Response Fields**: Adding fields to existing responses
4. **New HTTP Methods**: Adding new HTTP methods to existing resources
5. **Enhanced Functionality**: Improving existing behavior without changing contracts

**Example:**
```json
// v1.0 - Original
GET /api/properties/123

// v1.1 - Adding optional filter parameter (non-breaking)
GET /api/properties/123?includeAmenities=true
```

### Breaking Changes

Changes that affect backward compatibility:

1. **Removing Endpoints**: Removing or retiring API endpoints
2. **Changing HTTP Methods**: Changing HTTP verbs (GET to POST)
3. **Removing Response Fields**: Removing or renaming response fields
4. **Changing Parameter Types**: Changing data types or formats
5. **Changing URL Structure**: Modifying resource URLs
6. **Changing Authentication**: Modifying authentication requirements
7. **Changing Status Codes**: Changing HTTP status codes for operations

**Example:**
```json
// v1.0 - Original
GET /api/properties/123
Response: { "id": 123, "address": "123 Main St" }

// v2.0 - Breaking change (removed field)
GET /api/v2/properties/123
Response: { "id": 123, "propertyAddress": "123 Main St" }
```

## Versioning Rules

### URL Structure

```
https://api.rentalmanager.com/v{major}/resource
```

- **Major Version**: Required for breaking changes
- **Minor Version**: Not exposed in URL (implied in API docs)
- **Patch Version**: Internal only (not exposed in URL)

### Request Headers

Optional header for API version preference:

```
Accept: application/json; version=1.0
```

### Version Negotiation

The API supports multiple methods for version negotiation:

1. **URL Path** (primary): `/v1/properties`
2. **Accept Header**: `Accept: application/json; version=1.0`
3. **Custom Header**: `X-API-Version: 1.0`

## Migration Guides

When introducing breaking changes in a new version, provide a migration guide:

### Migration Guide Template

```markdown
# Migration Guide: v1.0 → v2.0

## Overview
Migration guide for upgrading from API v1.0 to v2.0

## Breaking Changes

### 1. Property Endpoint
**v1.0:**
GET /api/properties/123
Response: { "id": 123, "address": "..." }

**v2.0:**
GET /api/v2/properties/123
Response: { "id": 123, "propertyAddress": "..." }

**Migration Steps:**
1. Update endpoint URL to `/api/v2/properties/{id}`
2. Replace `address` field with `propertyAddress` in your code
3. Update response parsing logic

### 2. Updated Field Names
All snake_case fields are now camelCase.

**Before:** `created_at`
**After:** `createdAt`

## Timeline
- v1.0 deprecation date: January 1, 2026
- v2.0 release date: January 1, 2026
- v1.0 end of life: January 1, 2027
```

## Version Rollout

### Deprecation Process

1. **Announcement**: 6 months before deprecation
   - Blog post
   - API documentation updates
   - Email notifications to API consumers

2. **Deprecation**: Mark as deprecated in documentation
   - Add deprecation warnings in responses
   - Log usage for monitoring

3. **Support Period**: 12 months of security updates
   - No new features
   - Security fixes only
   - Bug fixes for critical issues only

4. **End of Life**: After support period
   - API endpoints return 410 Gone
   - Services may be terminated

### Deprecation Headers

Deprecated APIs include headers:

```
Warning: 299 - "Deprecated API. Use v2 instead."
Deprecation: true
Sunset: "Wed, 01 Jan 2027 00:00:00 GMT"
```

## Versioning Best Practices

### For API Developers

1. **Avoid Breaking Changes**: Design APIs to be extensible
2. **Additive Changes**: Add new fields/parameters instead of modifying existing ones
3. **Documentation**: Update API docs for each change
4. **Testing**: Maintain tests for all supported versions
5. **Migration Tools**: Provide tools to ease migration

### For API Consumers

1. **Version Pinning**: Use specific API versions in production
2. **Monitor Deprecations**: Watch for deprecation warnings
3. **Plan Migrations**: Start migration planning before EOL
4. **Test Updates**: Test new versions in staging before production
5. **Stay Updated**: Subscribe to API changelog notifications

## Version Implementation

### Backend Implementation

```csharp
// Controllers
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/properties")]
public class PropertiesController : ControllerBase
{
    [HttpGet]
    public IActionResult GetProperties()
    {
        // Implementation
    }
}
```

### Version Mapping

```csharp
// Program.cs
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-API-Version"),
        new AcceptHeaderApiVersionReader("Accept")
    );
});
```

## Testing Across Versions

### Version Testing Strategy

1. **Unit Tests**: Test each version independently
2. **Integration Tests**: Test version negotiation
3. **Compatibility Tests**: Test backward compatibility
4. **Migration Tests**: Test migration paths

### Example Test

```csharp
[Test]
public async Task ShouldSupportBothV1AndV2()
{
    // Test v1
    var v1Response = await client.GetAsync("/api/v1/properties/123");
    Assert.AreEqual(200, v1Response.StatusCode);
    
    // Test v2
    var v2Response = await client.GetAsync("/api/v2/properties/123");
    Assert.AreEqual(200, v2Response.StatusCode);
}
```

## Changelog

### Changelog Format

Follow [Keep a Changelog](https://keepachangelog.com/) format:

```markdown
## [2.0.0] - 2025-06-01

### Added
- New property search endpoint
- Filtering by amenities

### Changed
- Property address field renamed to `propertyAddress`

### Deprecated
- /api/v1/properties endpoint (use v2)

### Removed
- Support for Python 3.6

### Fixed
- Fixed pagination issue
```

## Versioning Checklist

### When Creating a New Version

- [ ] Document all breaking changes
- [ ] Create migration guide
- [ ] Update API documentation
- [ ] Add deprecation headers to old version
- [ ] Update version negotiation logic
- [ ] Add integration tests for new version
- [ ] Update client libraries
- [ ] Announce deprecation timeline

### When Introducing Breaking Changes

- [ ] Consider if change is truly necessary
- [ ] Explore non-breaking alternatives
- [ ] Create new version for breaking changes
- [ ] Maintain backward compatibility
- [ ] Provide migration path
- [ ] Communicate changes to consumers

## Resources

- [API Versioning Best Practices](https://restfulapi.net/versioning/)
- [RESTful API Design Guide](https://restfulapi.net/)
- [Semantic Versioning](https://semver.org/)
- [ASP.NET Core API Versioning](https://github.com/dotnet/aspnet-api-versioning)

## Support

For questions about API versioning:

- API Documentation: [docs/api-documentation.md](api-documentation.md)
- Open an issue: [GitHub Issues](https://github.com/your-org/RentalManager/issues)
- Contact: api-support@rentalmanager.com

