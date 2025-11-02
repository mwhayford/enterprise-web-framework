// Copyright (c) RentalManager. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace RentalManager.Infrastructure.Data;

/// <summary>
/// Comprehensive list of major US metropolitan areas covering all 50 states.
/// </summary>
public static class MetroAreaData
{
    /// <summary>
    /// Gets all major metropolitan areas in the United States, covering all 50 states.
    /// Organized by state, with rent multipliers based on market data.
    /// </summary>
    public static List<MetroArea> GetAllMetroAreas()
    {
        return new List<MetroArea>
        {
            // Alabama
            new("Birmingham-Hoover", "Birmingham", "Alabama", "AL", 0.75m, 35200),
            new("Huntsville", "Huntsville", "Alabama", "AL", 0.80m, 35800),
            new("Mobile", "Mobile", "Alabama", "AL", 0.70m, 36600),

            // Alaska
            new("Anchorage", "Anchorage", "Alaska", "AK", 1.20m, 99500),

            // Arizona
            new("Phoenix-Mesa-Chandler", "Phoenix", "Arizona", "AZ", 1.10m, 85000),
            new("Tucson", "Tucson", "Arizona", "AZ", 0.85m, 85700),

            // Arkansas
            new("Little Rock-North Little Rock-Conway", "Little Rock", "Arkansas", "AR", 0.70m, 72200),
            new("Fayetteville-Springdale-Rogers", "Fayetteville", "Arkansas", "AR", 0.80m, 72700),

            // California
            new("Los Angeles-Long Beach-Anaheim", "Los Angeles", "California", "CA", 2.20m, 90000),
            new("San Francisco-Oakland-Berkeley", "San Francisco", "California", "CA", 2.80m, 94100),
            new("San Diego-Chula Vista-Carlsbad", "San Diego", "California", "CA", 2.00m, 92100),
            new("San Jose-Sunnyvale-Santa Clara", "San Jose", "California", "CA", 2.50m, 95100),
            new("Sacramento-Roseville-Folsom", "Sacramento", "California", "CA", 1.40m, 95800),
            new("Fresno", "Fresno", "California", "CA", 0.90m, 93700),
            new("Riverside-San Bernardino-Ontario", "Riverside", "California", "CA", 1.30m, 92500),

            // Colorado
            new("Denver-Aurora-Lakewood", "Denver", "Colorado", "CO", 1.50m, 80200),
            new("Colorado Springs", "Colorado Springs", "Colorado", "CO", 1.10m, 80900),

            // Connecticut
            new("Hartford-East Hartford-Middletown", "Hartford", "Connecticut", "CT", 1.20m, 6100),
            new("New Haven-Milford", "New Haven", "Connecticut", "CT", 1.15m, 6500),
            new("Bridgeport-Stamford-Norwalk", "Bridgeport", "Connecticut", "CT", 1.80m, 6600),

            // Delaware
            new("Wilmington", "Wilmington", "Delaware", "DE", 1.10m, 19800),

            // Florida
            new("Miami-Fort Lauderdale-Pompano Beach", "Miami", "Florida", "FL", 1.60m, 33100),
            new("Tampa-St. Petersburg-Clearwater", "Tampa", "Florida", "FL", 1.20m, 33600),
            new("Orlando-Kissimmee-Sanford", "Orlando", "Florida", "FL", 1.15m, 32800),
            new("Jacksonville", "Jacksonville", "Florida", "FL", 0.95m, 32200),
            new("Tallahassee", "Tallahassee", "Florida", "FL", 0.85m, 32300),

            // Georgia
            new("Atlanta-Sandy Springs-Alpharetta", "Atlanta", "Georgia", "GA", 1.25m, 30300),
            new("Savannah", "Savannah", "Georgia", "GA", 0.90m, 31400),
            new("Augusta-Richmond County", "Augusta", "Georgia", "GA", 0.75m, 30900),

            // Hawaii
            new("Urban Honolulu", "Honolulu", "Hawaii", "HI", 2.10m, 96800),

            // Idaho
            new("Boise City", "Boise", "Idaho", "ID", 1.15m, 83700),
            new("Idaho Falls", "Idaho Falls", "Idaho", "ID", 0.85m, 83400),

            // Illinois
            new("Chicago-Naperville-Elgin", "Chicago", "Illinois", "IL", 1.60m, 60600),
            new("Peoria", "Peoria", "Illinois", "IL", 0.75m, 61600),
            new("Rockford", "Rockford", "Illinois", "IL", 0.70m, 61100),

            // Indiana
            new("Indianapolis-Carmel-Anderson", "Indianapolis", "Indiana", "IN", 0.95m, 46200),
            new("Fort Wayne", "Fort Wayne", "Indiana", "IN", 0.75m, 46800),
            new("Evansville", "Evansville", "Indiana", "IN", 0.70m, 47700),

            // Iowa
            new("Des Moines-West Des Moines", "Des Moines", "Iowa", "IA", 0.85m, 50300),
            new("Cedar Rapids", "Cedar Rapids", "Iowa", "IA", 0.80m, 52400),

            // Kansas
            new("Wichita", "Wichita", "Kansas", "KS", 0.75m, 67200),
            new("Kansas City", "Kansas City", "Kansas", "KS", 0.90m, 66100),

            // Kentucky
            new("Louisville/Jefferson County", "Louisville", "Kentucky", "KY", 0.85m, 40200),
            new("Lexington-Fayette", "Lexington", "Kentucky", "KY", 0.80m, 40500),

            // Louisiana
            new("New Orleans-Metairie", "New Orleans", "Louisiana", "LA", 0.95m, 70100),
            new("Baton Rouge", "Baton Rouge", "Louisiana", "LA", 0.85m, 70800),
            new("Shreveport-Bossier City", "Shreveport", "Louisiana", "LA", 0.75m, 71100),

            // Maine
            new("Portland-South Portland", "Portland", "Maine", "ME", 1.20m, 4100),

            // Maryland
            new("Baltimore-Columbia-Towson", "Baltimore", "Maryland", "MD", 1.30m, 21200),
            new("Frederick-Gaithersburg-Rockville", "Frederick", "Maryland", "MD", 1.60m, 21700),

            // Massachusetts
            new("Boston-Cambridge-Newton", "Boston", "Massachusetts", "MA", 2.00m, 2100),
            new("Worcester", "Worcester", "Massachusetts", "MA", 1.10m, 1600),
            new("Springfield", "Springfield", "Massachusetts", "MA", 0.95m, 1100),

            // Michigan
            new("Detroit-Warren-Dearborn", "Detroit", "Michigan", "MI", 1.00m, 48200),
            new("Grand Rapids-Kentwood", "Grand Rapids", "Michigan", "MI", 0.90m, 49500),
            new("Ann Arbor", "Ann Arbor", "Michigan", "MI", 1.30m, 48100),

            // Minnesota
            new("Minneapolis-St. Paul-Bloomington", "Minneapolis", "Minnesota", "MN", 1.30m, 55400),
            new("Duluth", "Duluth", "Minnesota", "MN", 0.80m, 55800),

            // Mississippi
            new("Jackson", "Jackson", "Mississippi", "MS", 0.75m, 39200),
            new("Gulfport-Biloxi", "Gulfport", "Mississippi", "MS", 0.70m, 39500),

            // Missouri
            new("Kansas City", "Kansas City", "Missouri", "MO", 0.95m, 64100),
            new("St. Louis", "St. Louis", "Missouri", "MO", 0.90m, 63100),
            new("Springfield", "Springfield", "Missouri", "MO", 0.75m, 65800),

            // Montana
            new("Billings", "Billings", "Montana", "MT", 0.85m, 59100),
            new("Missoula", "Missoula", "Montana", "MT", 0.90m, 59800),

            // Nebraska
            new("Omaha-Council Bluffs", "Omaha", "Nebraska", "NE", 0.90m, 68100),
            new("Lincoln", "Lincoln", "Nebraska", "NE", 0.85m, 68500),

            // Nevada
            new("Las Vegas-Henderson-Paradise", "Las Vegas", "Nevada", "NV", 1.20m, 89100),
            new("Reno", "Reno", "Nevada", "NV", 1.15m, 89500),

            // New Hampshire
            new("Manchester-Nashua", "Manchester", "New Hampshire", "NH", 1.25m, 3100),

            // New Jersey
            new("Newark-Jersey City", "Newark", "New Jersey", "NJ", 1.60m, 7100),
            new("Trenton-Princeton", "Trenton", "New Jersey", "NJ", 1.40m, 8600),

            // New Mexico
            new("Albuquerque", "Albuquerque", "New Mexico", "NM", 0.90m, 87100),
            new("Santa Fe", "Santa Fe", "New Mexico", "NM", 1.10m, 87500),

            // New York
            new("New York-Newark-Jersey City", "New York", "New York", "NY", 2.50m, 10000),
            new("Buffalo-Cheektowaga", "Buffalo", "New York", "NY", 0.90m, 14200),
            new("Rochester", "Rochester", "New York", "NY", 0.85m, 14600),
            new("Albany-Schenectady-Troy", "Albany", "New York", "NY", 1.00m, 12200),

            // North Carolina
            new("Charlotte-Concord-Gastonia", "Charlotte", "North Carolina", "NC", 1.15m, 28200),
            new("Raleigh-Cary", "Raleigh", "North Carolina", "NC", 1.20m, 27600),
            new("Greensboro-High Point", "Greensboro", "North Carolina", "NC", 0.80m, 27400),
            new("Winston-Salem", "Winston-Salem", "North Carolina", "NC", 0.75m, 27100),

            // North Dakota
            new("Fargo", "Fargo", "North Dakota", "ND", 0.90m, 58100),
            new("Bismarck", "Bismarck", "North Dakota", "ND", 0.85m, 58500),

            // Ohio
            new("Columbus", "Columbus", "Ohio", "OH", 1.00m, 43200),
            new("Cleveland-Elyria", "Cleveland", "Ohio", "OH", 0.85m, 44100),
            new("Cincinnati", "Cincinnati", "Ohio", "OH", 0.90m, 45200),
            new("Toledo", "Toledo", "Ohio", "OH", 0.75m, 43600),

            // Oklahoma
            new("Oklahoma City", "Oklahoma City", "Oklahoma", "OK", 0.80m, 73100),
            new("Tulsa", "Tulsa", "Oklahoma", "OK", 0.75m, 74100),

            // Oregon
            new("Portland-Vancouver-Hillsboro", "Portland", "Oregon", "OR", 1.50m, 97200),
            new("Eugene-Springfield", "Eugene", "Oregon", "OR", 1.10m, 97400),

            // Pennsylvania
            new("Philadelphia-Camden-Wilmington", "Philadelphia", "Pennsylvania", "PA", 1.40m, 19100),
            new("Pittsburgh", "Pittsburgh", "Pennsylvania", "PA", 0.95m, 15200),
            new("Allentown-Bethlehem-Easton", "Allentown", "Pennsylvania", "PA", 1.00m, 18100),

            // Rhode Island
            new("Providence-Warwick", "Providence", "Rhode Island", "RI", 1.20m, 2900),

            // South Carolina
            new("Charleston-North Charleston", "Charleston", "South Carolina", "SC", 1.15m, 29400),
            new("Columbia", "Columbia", "South Carolina", "SC", 0.80m, 29200),
            new("Greenville-Anderson", "Greenville", "South Carolina", "SC", 0.85m, 29600),

            // South Dakota
            new("Sioux Falls", "Sioux Falls", "South Dakota", "SD", 0.85m, 57100),
            new("Rapid City", "Rapid City", "South Dakota", "SD", 0.80m, 57700),

            // Tennessee
            new("Nashville-Davidson--Murfreesboro--Franklin", "Nashville", "Tennessee", "TN", 1.20m, 37200),
            new("Memphis", "Memphis", "Tennessee", "TN", 0.85m, 38100),
            new("Knoxville", "Knoxville", "Tennessee", "TN", 0.80m, 37900),

            // Texas
            new("Dallas-Fort Worth-Arlington", "Dallas", "Texas", "TX", 1.30m, 75200),
            new("Houston-The Woodlands-Sugar Land", "Houston", "Texas", "TX", 1.15m, 77000),
            new("Austin-Round Rock-Georgetown", "Austin", "Texas", "TX", 1.40m, 78700),
            new("San Antonio-New Braunfels", "San Antonio", "Texas", "TX", 0.95m, 78200),
            new("El Paso", "El Paso", "Texas", "TX", 0.80m, 79900),

            // Utah
            new("Salt Lake City", "Salt Lake City", "Utah", "UT", 1.25m, 84100),
            new("Provo-Orem", "Provo", "Utah", "UT", 1.10m, 84600),

            // Vermont
            new("Burlington-South Burlington", "Burlington", "Vermont", "VT", 1.15m, 5400),

            // Virginia
            new("Virginia Beach-Norfolk-Newport News", "Virginia Beach", "Virginia", "VA", 1.10m, 23450),
            new("Richmond", "Richmond", "Virginia", "VA", 1.05m, 23200),
            new("Arlington-Alexandria", "Arlington", "Virginia", "VA", 1.80m, 22200),

            // Washington
            new("Seattle-Tacoma-Bellevue", "Seattle", "Washington", "WA", 1.90m, 98100),
            new("Spokane-Spokane Valley", "Spokane", "Washington", "WA", 0.90m, 99200),

            // West Virginia
            new("Charleston", "Charleston", "West Virginia", "WV", 0.75m, 25300),
            new("Huntington-Ashland", "Huntington", "West Virginia", "WV", 0.70m, 25700),

            // Wisconsin
            new("Milwaukee-Waukesha", "Milwaukee", "Wisconsin", "WI", 1.00m, 53200),
            new("Madison", "Madison", "Wisconsin", "WI", 1.15m, 53700),
            new("Green Bay", "Green Bay", "Wisconsin", "WI", 0.80m, 54300),

            // Wyoming
            new("Cheyenne", "Cheyenne", "Wyoming", "WY", 0.85m, 82000),
            new("Casper", "Casper", "Wyoming", "WY", 0.80m, 82600),
        };
    }

    /// <summary>
    /// Gets the count of total metro areas.
    /// </summary>
    public static int GetMetroAreaCount() => GetAllMetroAreas().Count;

    /// <summary>
    /// Gets metro areas grouped by state code.
    /// </summary>
    public static Dictionary<string, List<MetroArea>> GetMetroAreasByState()
    {
        return GetAllMetroAreas()
            .GroupBy(m => m.StateCode)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}