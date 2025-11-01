# Metro Area Property Seeding

## Overview

The RentalManager system now supports seeding properties across all major US metropolitan areas, covering all 50 states. This enables comprehensive testing with realistic property data distributed across the entire United States.

## Capabilities

### Scale
- **~150 metro areas** across all 50 US states
- **1,000 properties per metro area** (configurable)
- **Total: ~150,000 properties** (at default 1,000 per metro)
- **Batch processing**: 100 properties per batch for optimal performance
- **Progress logging**: Updates every 500 properties per metro area

### Coverage
Every US state is represented with at least one major metropolitan area:
- Major metros (multiple per state for large states like CA, TX, NY, FL)
- Mid-size metros (2-3 per state for most states)
- Small metros (1-2 per state for smaller states)

### Data Quality
- **Metro-specific rent multipliers**: Based on actual market rates (e.g., San Francisco: 2.8x, New York: 2.5x, average markets: 1.0x)
- **Realistic zip codes**: Uses metro area-specific zip code prefixes
- **Geographic distribution**: Properties distributed across cities within each metro area
- **Property variety**: Mix of property types, sizes, amenities, and statuses

## Usage

### Via Admin API

#### Seed 1,000 properties per metro area (default):
```bash
curl -X POST "http://localhost:5111/api/admin/seed-properties?byMetro=true&propertiesPerMetro=1000" \
  -H "Authorization: Bearer <your-jwt-token>"
```

#### Seed custom number per metro:
```bash
curl -X POST "http://localhost:5111/api/admin/seed-properties?byMetro=true&propertiesPerMetro=500" \
  -H "Authorization: Bearer <your-jwt-token>"
```

#### Traditional seeding (random distribution):
```bash
curl -X POST "http://localhost:5111/api/admin/seed-properties?count=10000" \
  -H "Authorization: Bearer <your-jwt-token>"
```

### Via PowerShell Script

Update `scripts/post-deploy-seed.ps1` to include metro area seeding option:
```powershell
.\scripts\post-deploy-seed.ps1 -PropertyCount 150000 -ByMetro
```

## Metro Areas Included

The system includes major metropolitan areas such as:

- **California**: Los Angeles, San Francisco, San Diego, San Jose, Sacramento, Fresno, Riverside
- **New York**: New York City, Buffalo, Rochester, Albany
- **Texas**: Dallas, Houston, Austin, San Antonio, El Paso
- **Florida**: Miami, Tampa, Orlando, Jacksonville, Tallahassee
- **Illinois**: Chicago, Peoria, Rockford
- **And 45+ more states** with their major metros

Each metro area includes:
- Metro name (e.g., "Los Angeles-Long Beach-Anaheim")
- Primary city
- State name and code
- Rent multiplier (market-based)
- Zip code prefix for realistic addresses

## Performance

### Seeding Time Estimates
- **1,000 properties per metro**: ~150 metro areas × ~10 seconds per metro = ~25 minutes total
- **Batch processing**: 100 properties per database transaction (optimized for performance)
- **Memory efficient**: Properties generated and saved in batches to avoid memory issues

### Database Considerations
- Large-scale seeding (150,000+ properties) requires adequate database resources
- PostgreSQL connection pooling handles concurrent operations efficiently
- Indexes on city, state, and property type ensure fast search queries

## Example Metro Areas

| State | Metro Area | City | Rent Multiplier |
|-------|-----------|------|----------------|
| CA | Los Angeles-Long Beach-Anaheim | Los Angeles | 2.20x |
| CA | San Francisco-Oakland-Berkeley | San Francisco | 2.80x |
| NY | New York-Newark-Jersey City | New York | 2.50x |
| TX | Dallas-Fort Worth-Arlington | Dallas | 1.30x |
| TX | Austin-Round Rock-Georgetown | Austin | 1.40x |
| FL | Miami-Fort Lauderdale-Pompano Beach | Miami | 1.60x |
| IL | Chicago-Naperville-Elgin | Chicago | 1.60x |
| WA | Seattle-Tacoma-Bellevue | Seattle | 1.90x |
| ... | ... | ... | ... |

## Technical Details

### Files Created
- `MetroArea.cs`: Data model for metro area information
- `MetroAreaData.cs`: Comprehensive list of ~150 metro areas with market data

### Methods Added
- `PropertySeeder.SeedPropertiesByMetroAreaAsync()`: Seeds properties distributed by metro area
- `PropertySeeder.GeneratePropertyForMetro()`: Generates property with metro-specific data
- `PropertySeeder.GenerateZipCodeForMetro()`: Creates realistic zip codes per metro

### API Enhancements
- `AdminController.SeedProperties()`: Added `byMetro` and `propertiesPerMetro` parameters
- Supports up to 5,000 properties per metro (configurable)
- Maximum total properties: 150,000+ (150 metros × 1,000 default)

## Recommendations

1. **Start Small**: Test with `propertiesPerMetro=100` first to verify functionality
2. **Monitor Progress**: Check backend logs for seeding progress
3. **Database Size**: Ensure adequate disk space for 150,000+ properties (~500MB-1GB)
4. **Search Performance**: Properties are indexed, but very large datasets may require additional optimization

## Future Enhancements

Potential improvements:
- Metro area-specific property type distributions (e.g., more apartments in NYC, more houses in suburbs)
- Seasonal availability variations
- Neighborhood-level granularity
- Property image URLs by metro area

