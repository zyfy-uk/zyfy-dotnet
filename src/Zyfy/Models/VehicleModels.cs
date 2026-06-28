namespace Zyfy.Models;

/// <summary>Full enriched response for a single UK vehicle lookup.</summary>
public sealed record VehicleResult
{
    /// <summary>Normalised registration mark — uppercase, no spaces.</summary>
    public string Registration { get; init; } = string.Empty;
    public string? Make { get; init; }
    public string? Model { get; init; }
    /// <summary>car, van, motorcycle, bus, hgv, motorhome, trailer, tractor, other.</summary>
    public string? VehicleType { get; init; }
    public string? Colour { get; init; }
    public string? FuelType { get; init; }
    /// <summary>Engine displacement in cubic centimetres.</summary>
    public int? EngineCapacityCc { get; init; }
    public int? YearOfManufacture { get; init; }
    /// <summary>YYYY-MM format.</summary>
    public string? MonthOfFirstRegistration { get; init; }
    public double? VehicleAgeYears { get; init; }
    public VehicleSummary? Summary { get; init; }
    /// <summary>Detailed intelligence signals derived from MOT history and DVLA data.</summary>
    public VehicleSignals? Signals { get; init; }
    public VehicleScores? Scores { get; init; }
    public FleetFailureProfile? FleetFailureProfile { get; init; }
    public FleetAdvisoryProfile? FleetAdvisoryProfile { get; init; }
    public VehicleSources Sources { get; init; } = new VehicleSources();
    public string SchemaVersion { get; init; } = string.Empty;
    /// <summary>True when background enrichment is still in progress; data will improve on retry.</summary>
    public bool EnrichmentPending { get; init; }
    /// <summary>ISO 8601 datetime when the underlying data was last refreshed.</summary>
    public string DataAsOf { get; init; } = string.Empty;
    /// <summary>ISO 8601 datetime when this response was generated.</summary>
    public string CheckedAt { get; init; } = string.Empty;
    /// <summary>Null when this result appears as an item inside a bulk response.</summary>
    public Quota? Quota { get; init; }
}

public sealed record VehicleSummary
{
    /// <summary>good, consider, caution, or avoid.</summary>
    public string? BuyRecommendation { get; init; }
    /// <summary>low, medium, or high.</summary>
    public string? VehicleRiskLevel { get; init; }
    /// <summary>low, medium, or high.</summary>
    public string? MotRiskLevel { get; init; }
    /// <summary>good, fair, poor, or bad.</summary>
    public string? ConditionBand { get; init; }
    /// <summary>good, fair, poor, or bad.</summary>
    public string? MaintenanceBand { get; init; }
    /// <summary>none, low, or high.</summary>
    public string? MileageAnomalyRisk { get; init; }
    /// <summary>True when V5C colour differs from the colour recorded at the last MOT.</summary>
    public bool? ColourChangeIndicated { get; init; }
    /// <summary>True when this vehicle's advisory rate exceeds the fleet average for its make/model/year.</summary>
    public bool? AboveAverageAdvisories { get; init; }
    /// <summary>False for Northern Ireland vehicles where DVA does not share failure details.</summary>
    public bool MotFailureDetailAvailable { get; init; }
}

/// <summary>Detailed intelligence signals derived from MOT history and DVLA data.</summary>
public sealed record VehicleSignals
{
    /// <summary>CO2 emissions in grams per kilometre.</summary>
    public int? Co2EmissionsGPerKm { get; init; }
    /// <summary>Euro emission standard (e.g. Euro 6).</summary>
    public string? EuroEmissionStandard { get; init; }
    /// <summary>True if compliant with the London Ultra Low Emission Zone.</summary>
    public bool? UlezCompliant { get; init; }
    public bool MarkedForExport { get; init; }
    public bool? HasOutstandingRecall { get; init; }
    /// <summary>Date the V5C logbook was last issued (YYYY-MM-DD).</summary>
    public string? V5cLastIssued { get; init; }
    public string? TaxStatus { get; init; }
    /// <summary>YYYY-MM-DD</summary>
    public string? TaxDueDate { get; init; }
    public int? TaxDaysRemaining { get; init; }
    /// <summary>Vehicle Excise Duty band.</summary>
    public string? VedBand { get; init; }
    /// <summary>Annual VED cost in GBP.</summary>
    public int? VedAnnualCostGbp { get; init; }
    public string? MotStatus { get; init; }
    /// <summary>YYYY-MM-DD</summary>
    public string? MotExpiryDate { get; init; }
    public int? MotDaysRemaining { get; init; }
    /// <summary>True when the MOT expires within 30 days.</summary>
    public bool ImminentMot { get; init; }
    /// <summary>consistent, increasing, decreasing, or erratic.</summary>
    public string? OdometerTrend { get; init; }
    /// <summary>Most recent recorded odometer reading in miles.</summary>
    public int? LatestOdometerMiles { get; init; }
    /// <summary>Estimated typical annual mileage in miles.</summary>
    public int? TypicalAnnualMileageMiles { get; init; }
    /// <summary>below_average, average, or above_average.</summary>
    public string? OdometerVsFleetAverage { get; init; }
    /// <summary>Historical MOT pass rate as a fraction (0–1).</summary>
    public double? MotPassRate { get; init; }
    public int TotalMotTests { get; init; }
    public int TotalMotFailures { get; init; }
    public int TotalAdvisoryCount { get; init; }
    public int TotalFailureItemCount { get; init; }
    public int LatestAdvisoryCount { get; init; }
    public int LatestFailureItemCount { get; init; }
    public bool DangerousDefectEver { get; init; }
    public bool HighFailureHistory { get; init; }
    /// <summary>increasing, stable, or decreasing.</summary>
    public string? AdvisoryTrend { get; init; }
    /// <summary>worsening, stable, or improving.</summary>
    public string? AdvisoryMomentum { get; init; }
    public int? DaysSinceLastFailure { get; init; }
    public int? FailuresLast24Months { get; init; }
    public int? AdvisoriesLast3Tests { get; init; }
    public int TrendWindowTests { get; init; }
    /// <summary>YYYY-MM-DD</summary>
    public string? FirstMotDate { get; init; }
    /// <summary>YYYY-MM-DD</summary>
    public string? LastMotDate { get; init; }
    public string? LastMotResult { get; init; }
    /// <summary>Estimated date when the first MOT becomes due (YYYY-MM-DD).</summary>
    public string? FirstMotDue { get; init; }
    /// <summary>Failure defect categories that have recurred across more than one MOT test.</summary>
    public string[]? FailureClusters { get; init; }
    public int? RepeatFailureCount { get; init; }
    public string[]? AdvisoryClusters { get; init; }
    public NcapSafetyRating? NcapSafetyRating { get; init; }
    public DrivetrainStressProfile? DrivetrainStressProfile { get; init; }
}

/// <summary>Euro NCAP safety rating for the vehicle.</summary>
public sealed record NcapSafetyRating
{
    /// <summary>Overall Euro NCAP star rating (0–5).</summary>
    public int? OverallStars { get; init; }
    /// <summary>Adult occupant protection score (0–100).</summary>
    public int? AdultOccupant { get; init; }
    /// <summary>Child occupant protection score (0–100).</summary>
    public int? ChildOccupant { get; init; }
    /// <summary>Vulnerable road user protection score (0–100).</summary>
    public int? VulnerableRoadUsers { get; init; }
    /// <summary>Safety assist score (0–100).</summary>
    public int? SafetyAssist { get; init; }
    public int? TestedYear { get; init; }
}

public sealed record DrivetrainStressProfile
{
    /// <summary>short_urban, mixed, or long_distance.</summary>
    public string? LikelyDrivingPattern { get; init; }
    /// <summary>Diesel vehicles only. low, elevated, or high.</summary>
    public string? DpfRisk { get; init; }
}

public sealed record VehicleScores
{
    /// <summary>MOT risk score (0–1). Lower is better.</summary>
    public double? MotRiskScore { get; init; }
    /// <summary>Condition score (0–1). Higher is better.</summary>
    public double? ConditionScore { get; init; }
    public double? ConditionPercentile { get; init; }
    /// <summary>Maintenance score (0–1). Higher is better.</summary>
    public double? MaintenanceScore { get; init; }
    public double? MaintenancePercentile { get; init; }
    /// <summary>This vehicle's failure rate divided by fleet average. 1.0 = average.</summary>
    public double? FailureRateRatio { get; init; }
    /// <summary>This vehicle's advisory rate divided by fleet average. 1.0 = average.</summary>
    public double? AdvisoryRateRatio { get; init; }
    public int? BenchmarkSampleSize { get; init; }
    public double? AvgAdvisoriesPerTestForMmy { get; init; }
    public double? AvgFailuresPerTestForMmy { get; init; }
    /// <summary>Likelihood the vehicle is off-road (0–1). Higher = more likely SORN'd or scrapped.</summary>
    public double? OffRoadLikelihoodScore { get; init; }
    public string ScoreConvention { get; init; } = string.Empty;
}

public sealed record FleetFailureProfile
{
    public string MileageBand { get; init; } = string.Empty;
    public int SampleSize { get; init; }
    public FleetItem[] TopFailures { get; init; } = System.Array.Empty<FleetItem>();
}

public sealed record FleetAdvisoryProfile
{
    public string MileageBand { get; init; } = string.Empty;
    public int SampleSize { get; init; }
    public FleetItem[] TopAdvisories { get; init; } = System.Array.Empty<FleetItem>();
}

public sealed record FleetItem
{
    public string Category { get; init; } = string.Empty;
    /// <summary>Occurrence rate as a fraction (e.g. 0.12 = 12% of vehicles in the sample).</summary>
    public double Rate { get; init; }
}

public sealed record VehicleSources
{
    public string MotHistory { get; init; } = string.Empty;
    public string MutableData { get; init; } = string.Empty;
    public string? SafetyRating { get; init; }
}
