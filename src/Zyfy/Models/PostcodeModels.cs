namespace Zyfy.Models;

/// <summary>Full enriched response for a single UK postcode lookup.</summary>
public sealed record PostcodeResult
{
    /// <summary>Formatted with space (e.g. "SW1A 2AA").</summary>
    public string Postcode { get; init; } = string.Empty;
    public string OutwardCode { get; init; } = string.Empty;
    /// <summary>Null for outward-code-only queries.</summary>
    public string? InwardCode { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public int? Eastings { get; init; }
    public int? Northings { get; init; }
    public string? Country { get; init; }
    /// <summary>England only.</summary>
    public string? Region { get; init; }
    public string? AdminDistrict { get; init; }
    /// <summary>Not all areas have a county.</summary>
    public string? AdminCounty { get; init; }
    public string? AdminWard { get; init; }
    public string? Parish { get; init; }
    public string? ParliamentaryConstituency { get; init; }
    public string? NhsTrust { get; init; }
    /// <summary>Lower Super Output Area code.</summary>
    public string? Lsoa { get; init; }
    /// <summary>Middle Super Output Area code.</summary>
    public string? Msoa { get; init; }
    /// <summary>ONS rural/urban classification.</summary>
    public string? RuralUrbanClassification { get; init; }
    public PostcodeSummary? Summary { get; init; }
    public PostcodeSignals? Signals { get; init; }
    public PostcodeScores? Scores { get; init; }
    /// <summary>Populated for Starter tier and above.</summary>
    public PostcodePercentiles? Percentiles { get; init; }
    /// <summary>ONS geography codes for this postcode.</summary>
    public GeographyCodes GeographyCodes { get; init; } = new GeographyCodes();
    /// <summary>Distance in metres from the query point. Populated for /nearest and /within responses.</summary>
    public double? QueryPointDistanceMetres { get; init; }
    public PostcodeSources Sources { get; init; } = new PostcodeSources();
    public string SchemaVersion { get; init; } = string.Empty;
    /// <summary>ISO 8601 datetime when the underlying data was last refreshed.</summary>
    public string DataAsOf { get; init; } = string.Empty;
    /// <summary>ISO 8601 datetime when this response was generated.</summary>
    public string CheckedAt { get; init; } = string.Empty;
    /// <summary>Null when this result appears as an item inside a bulk response.</summary>
    public Quota? Quota { get; init; }
}

public sealed record PostcodeSummary
{
    /// <summary>low, medium, or high.</summary>
    public string? PropertyRiskLevel { get; init; }
    /// <summary>low, medium, or high.</summary>
    public string? LiveabilityLevel { get; init; }
    /// <summary>low, medium, or high.</summary>
    public string? InsuranceRiskLevel { get; init; }
    /// <summary>weak, fair, good, or strong.</summary>
    public string? InvestmentOutlook { get; init; }
    /// <summary>strong_positive, positive, neutral, weak_negative, or strong_negative.</summary>
    public string? GrowthSignal { get; init; }
    /// <summary>low, medium, or high.</summary>
    public string DataConfidence { get; init; } = string.Empty;
    /// <summary>improving, stable, declining, or mixed.</summary>
    public string? AreaTrajectory { get; init; }
    /// <summary>poor, fair, good, or excellent.</summary>
    public string? FamilySuitability { get; init; }
    /// <summary>poor, fair, good, or excellent.</summary>
    public string? RetirementSuitability { get; init; }
}

/// <summary>All intelligence signals for a postcode, grouped by category.</summary>
public sealed record PostcodeSignals
{
    public PostcodeBroadbandSignals? Broadband { get; init; }
    public PostcodeFloodSignals? Flood { get; init; }
    public PostcodePropertySignals? Property { get; init; }
    public PostcodeCrimeSignals? Crime { get; init; }
    public PostcodeEnvironmentSignals? Environment { get; init; }
    public PostcodeHousingSignals? Housing { get; init; }
    public PostcodePoliticalSignals? Political { get; init; }
    public PostcodeDeprivationSignals? Deprivation { get; init; }
    public PostcodeDemographicsSignals? Demographics { get; init; }
}

public sealed record PostcodeBroadbandSignals
{
    /// <summary>True if superfast broadband (&gt;30 Mbps) is available.</summary>
    public bool? Superfast { get; init; }
    /// <summary>True if ultrafast broadband (&gt;100 Mbps) is available.</summary>
    public bool? Ultrafast { get; init; }
    /// <summary>True if gigabit-capable broadband is available.</summary>
    public bool? Gigabit { get; init; }
}

public sealed record PostcodeFloodSignals
{
    /// <summary>high, medium, low, or very_low.</summary>
    public string? RiversSea { get; init; }
    /// <summary>high, medium, low, or very_low.</summary>
    public string? Groundwater { get; init; }
    /// <summary>worsening, stable, improving, or insufficient_data.</summary>
    public string? RiversSeaTrend { get; init; }
    /// <summary>worsening, stable, improving, or insufficient_data.</summary>
    public string? GroundwaterTrend { get; init; }
}

public sealed record PostcodePropertySignals
{
    /// <summary>Median residential transaction price over the last 12 months.</summary>
    public int? AveragePrice { get; init; }
    /// <summary>25th percentile residential transaction price.</summary>
    public int? PriceLow { get; init; }
    /// <summary>75th percentile residential transaction price.</summary>
    public int? PriceHigh { get; init; }
    /// <summary>Year-over-year percentage change in median price (e.g. 5.2 = +5.2%).</summary>
    public double? PriceTrend { get; init; }
    public int PriceTrendPeriodMonths { get; init; }
    public int? TransactionVolume { get; init; }
    /// <summary>postcode, sector, or district.</summary>
    public string? Granularity { get; init; }
    /// <summary>high, medium, or low.</summary>
    public string? TrendConfidence { get; init; }
}

public sealed record PostcodeCrimeSignals
{
    /// <summary>very_low, low, medium, high, or very_high.</summary>
    public string? RateBand { get; init; }
    public PostcodeCrimeGranularity? DataGranularity { get; init; }
    public PostcodeCrimeCategories? Categories { get; init; }
}

public sealed record PostcodeCrimeGranularity
{
    /// <summary>lsoa (England/Wales) or datazone (Scotland).</summary>
    public string? Band { get; init; }
    /// <summary>lsoa (England/Wales) or local_authority (Scotland).</summary>
    public string? Categories { get; init; }
}

public sealed record PostcodeCrimeCategories
{
    public PostcodeCrimeCategory? Violence { get; init; }
    public PostcodeCrimeCategory? Property { get; init; }
    public PostcodeCrimeCategory? Vehicle { get; init; }
    public PostcodeCrimeCategory? Antisocial { get; init; }
    public PostcodeCrimeCategory? Drugs { get; init; }
    public PostcodeCrimeCategory? Damage { get; init; }
}

public sealed record PostcodeCrimeCategory
{
    /// <summary>very_low, low, medium, high, or very_high.</summary>
    public string? Band { get; init; }
    /// <summary>increasing, stable, decreasing, or insufficient_data.</summary>
    public string? Trend { get; init; }
}

public sealed record PostcodeEnvironmentSignals
{
    /// <summary>very_low, low, moderate, or high.</summary>
    public string? AirQualityBand { get; init; }
    /// <summary>worsening, stable, improving, or insufficient_data.</summary>
    public string? AirQualityTrend { get; init; }
    /// <summary>Annual mean NO2 concentration (µg/m³).</summary>
    public double? No2UgM3 { get; init; }
    /// <summary>Annual mean PM2.5 concentration (µg/m³).</summary>
    public double? Pm25UgM3 { get; init; }
    /// <summary>very_low, low, medium, high, or very_high.</summary>
    public string? RadonPotential { get; init; }
    /// <summary>Distance to the nearest green space in metres.</summary>
    public int? GreenSpaceProximityMetres { get; init; }
    /// <summary>True if the postcode falls within a national park.</summary>
    public bool? IsNationalPark { get; init; }
    /// <summary>True if the postcode falls within an Area of Outstanding Natural Beauty.</summary>
    public bool? IsAonb { get; init; }
    /// <summary>True if the postcode falls within a green belt.</summary>
    public bool? IsGreenBelt { get; init; }
}

public sealed record PostcodeHousingSignals
{
    /// <summary>Most common EPC energy efficiency rating (A–G).</summary>
    public string? EpcAverageRating { get; init; }
    public CouncilTaxBandEstimate? CouncilTaxBand { get; init; }
    /// <summary>detached, semi_detached, terraced, flat, or other.</summary>
    public string? DominantPropertyType { get; init; }
}

/// <summary>Estimated council tax band range for a postcode.</summary>
public sealed record CouncilTaxBandEstimate
{
    /// <summary>Lower bound band letter (A–I). Equals Upper when a single band is determined.</summary>
    public string Lower { get; init; } = string.Empty;
    /// <summary>Upper bound band letter (A–I). Equals Lower when a single band is determined.</summary>
    public string Upper { get; init; } = string.Empty;
    /// <summary>exact_nrs, derived_hmlr, or derived_lsoa.</summary>
    public string Source { get; init; } = string.Empty;
}

public sealed record PostcodePoliticalSignals
{
    public string? MpName { get; init; }
    public string? MpParty { get; init; }
    /// <summary>Hex colour code associated with the MP's party.</summary>
    public string? MpPartyColour { get; init; }
}

public sealed record PostcodeDeprivationSignals
{
    /// <summary>Index of Multiple Deprivation decile (1 = most deprived, 10 = least deprived). England only.</summary>
    public int? ImdDecile { get; init; }
    /// <summary>improving, stable, declining, or insufficient_data.</summary>
    public string? ImdTrend { get; init; }
}

public sealed record PostcodeDemographicsSignals
{
    public double? PercOwnerOccupied { get; init; }
    public double? PercPrivateRented { get; init; }
    public double? PercNoCarVan { get; init; }
    public double? MedianAge { get; init; }
    public double? PercEconomicallyActive { get; init; }
}

public sealed record PostcodeScores
{
    /// <summary>Property risk score (0–1). Lower is better.</summary>
    public double? PropertyRiskScore { get; init; }
    /// <summary>Liveability score (0–1). Higher is better.</summary>
    public double? LiveabilityScore { get; init; }
    /// <summary>Investment attractiveness score (0–1). Higher is better.</summary>
    public double? InvestmentScore { get; init; }
    /// <summary>Affordability index (0–1). Higher is better.</summary>
    public double? AffordabilityIndex { get; init; }
    public string ScoreConvention { get; init; } = string.Empty;
}

/// <summary>National percentile rankings for a postcode (Starter tier and above).</summary>
public sealed record PostcodePercentiles
{
    public double? FloodRivers { get; init; }
    public double? FloodGroundwater { get; init; }
    public double? CrimeRate { get; init; }
    public double? Imd { get; init; }
    public double? PropertyPrice { get; init; }
    public double? PropertyPriceRegional { get; init; }
    public double? Radon { get; init; }
    public double? AirQuality { get; init; }
    public double? GreenSpaceProximity { get; init; }
    public double? Epc { get; init; }
}

/// <summary>ONS geography codes for a postcode.</summary>
public sealed record GeographyCodes
{
    public string? AdminDistrict { get; init; }
    public string? AdminCounty { get; init; }
    public string? AdminWard { get; init; }
    public string? ParliamentaryConstituency { get; init; }
    public string? Lsoa { get; init; }
    public string? Msoa { get; init; }
}

public sealed record PostcodeSources
{
    public string Geography { get; init; } = string.Empty;
    public string Flood { get; init; } = string.Empty;
    public string Crime { get; init; } = string.Empty;
    public string Property { get; init; } = string.Empty;
    public string Deprivation { get; init; } = string.Empty;
    public string Broadband { get; init; } = string.Empty;
    public string Environment { get; init; } = string.Empty;
    public string Epc { get; init; } = string.Empty;
    public string GreenSpace { get; init; } = string.Empty;
    public string? Demographics { get; init; }
}

/// <summary>Result set from a <c>/within</c> radius search.</summary>
public sealed record PostcodeWithinResult
{
    public int Total { get; init; }
    /// <summary>Ordered by distance ascending.</summary>
    public PostcodeResult[] Results { get; init; } = System.Array.Empty<PostcodeResult>();
    public Quota Quota { get; init; } = new Quota();
}
