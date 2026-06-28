using System;
using System.Threading;
using System.Threading.Tasks;
using Zyfy.Internal;
using Zyfy.Models;

namespace Zyfy.Resources;

/// <summary>
/// Provides postcode intelligence lookups. Access via <see cref="ZyfyClient.Postcode"/>.
/// </summary>
public sealed class PostcodeResource
{
    private readonly HttpTransport _transport;

    internal PostcodeResource(HttpTransport transport)
    {
        _transport = transport;
    }

    /// <summary>
    /// Look up a single UK postcode.
    /// Returns geographic classification, broadband, flood risk, property prices,
    /// crime, air quality, deprivation, housing, and more.
    /// Northern Ireland postcodes (BT prefix) are not supported and will throw a
    /// <see cref="ValidationException"/> with code <c>unsupported_region</c>.
    /// </summary>
    /// <param name="postcode">UK postcode. Normalised to uppercase trimmed.</param>
    /// <param name="maxEnrichmentRetries">Per-call override for enrichment retry behaviour.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task<PostcodeResult> LookupAsync(
        string postcode,
        int? maxEnrichmentRetries = null,
        CancellationToken cancellationToken = default)
    {
        var pc = NormalisePostcode(postcode);
        return _transport.GetAsync<PostcodeResult>($"/postcode/{Uri.EscapeDataString(pc)}", maxEnrichmentRetries, cancellationToken);
    }

    /// <param name="postcode">UK postcode. Normalised to uppercase trimmed.</param>
    /// <param name="maxEnrichmentRetries">Per-call override for enrichment retry behaviour.</param>
    public PostcodeResult Lookup(string postcode, int? maxEnrichmentRetries = null)
    {
        var pc = NormalisePostcode(postcode);
        return _transport.Get<PostcodeResult>($"/postcode/{Uri.EscapeDataString(pc)}", maxEnrichmentRetries);
    }

    /// <summary>
    /// Find the nearest postcode to a set of WGS84 coordinates.
    /// The response includes <c>QueryPointDistanceMetres</c>.
    /// Throws <see cref="NotFoundException"/> if no postcode centroid is found within the radius.
    /// </summary>
    /// <param name="lat">WGS84 latitude.</param>
    /// <param name="lon">WGS84 longitude.</param>
    /// <param name="radius">Search radius in metres. Default 1000.</param>
    /// <param name="maxEnrichmentRetries">Per-call override for enrichment retry behaviour.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task<PostcodeResult> NearestAsync(
        double lat,
        double lon,
        int? radius = null,
        int? maxEnrichmentRetries = null,
        CancellationToken cancellationToken = default)
    {
        var path = BuildNearestPath(lat, lon, radius);
        return _transport.GetAsync<PostcodeResult>(path, maxEnrichmentRetries, cancellationToken);
    }

    /// <param name="lat">WGS84 latitude.</param>
    /// <param name="lon">WGS84 longitude.</param>
    /// <param name="radius">Search radius in metres. Default 1000.</param>
    /// <param name="maxEnrichmentRetries">Per-call override for enrichment retry behaviour.</param>
    public PostcodeResult Nearest(double lat, double lon, int? radius = null, int? maxEnrichmentRetries = null)
    {
        var path = BuildNearestPath(lat, lon, radius);
        return _transport.Get<PostcodeResult>(path, maxEnrichmentRetries);
    }

    /// <summary>
    /// Return every enriched postcode within a radius of a WGS84 coordinate.
    /// Results are ordered by distance ascending. Each postcode costs one quota unit.
    /// Available on Starter and above. Max radius 5,000 m.
    /// </summary>
    /// <param name="lat">WGS84 latitude.</param>
    /// <param name="lon">WGS84 longitude.</param>
    /// <param name="radius">Search radius in metres. Default 1000, max 5000.</param>
    /// <param name="maxEnrichmentRetries">Per-call override for enrichment retry behaviour.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task<PostcodeWithinResult> WithinAsync(
        double lat,
        double lon,
        int? radius = null,
        int? maxEnrichmentRetries = null,
        CancellationToken cancellationToken = default)
    {
        var path = BuildWithinPath(lat, lon, radius);
        return _transport.GetAsync<PostcodeWithinResult>(path, maxEnrichmentRetries, cancellationToken);
    }

    /// <param name="lat">WGS84 latitude.</param>
    /// <param name="lon">WGS84 longitude.</param>
    /// <param name="radius">Search radius in metres. Default 1000, max 5000.</param>
    /// <param name="maxEnrichmentRetries">Per-call override for enrichment retry behaviour.</param>
    public PostcodeWithinResult Within(double lat, double lon, int? radius = null, int? maxEnrichmentRetries = null)
    {
        var path = BuildWithinPath(lat, lon, radius);
        return _transport.Get<PostcodeWithinResult>(path, maxEnrichmentRetries);
    }

    /// <summary>
    /// Bulk postcode lookup. Up to your tier's bulk cap per call.
    /// </summary>
    /// <param name="postcodes">Postcodes to look up.</param>
    /// <param name="maxEnrichmentRetries">Per-call override for enrichment retry behaviour.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task<BulkPostcodeResult> BulkLookupAsync(
        string[] postcodes,
        int? maxEnrichmentRetries = null,
        CancellationToken cancellationToken = default)
    {
        return _transport.PostAsync<BulkPostcodeResult>("/postcode/bulk", new { postcodes }, maxEnrichmentRetries, cancellationToken);
    }

    /// <param name="postcodes">Postcodes to look up.</param>
    /// <param name="maxEnrichmentRetries">Per-call override for enrichment retry behaviour.</param>
    public BulkPostcodeResult BulkLookup(string[] postcodes, int? maxEnrichmentRetries = null)
    {
        return _transport.Post<BulkPostcodeResult>("/postcode/bulk", new { postcodes }, maxEnrichmentRetries);
    }

    /// <summary>
    /// Submit an async bulk postcode job. Returns a <see cref="BulkJobSubmitted"/> with a
    /// <c>JobId</c> to poll with <see cref="GetJobAsync"/>.
    /// </summary>
    /// <param name="postcodes">Postcodes to look up.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task<BulkJobSubmitted> SubmitBulkAsync(string[] postcodes, CancellationToken cancellationToken = default)
    {
        return _transport.PostAsync<BulkJobSubmitted>("/postcode/bulk/async", new { postcodes }, null, cancellationToken);
    }

    /// <param name="postcodes">Postcodes to look up.</param>
    public BulkJobSubmitted SubmitBulk(string[] postcodes)
    {
        return _transport.Post<BulkJobSubmitted>("/postcode/bulk/async", new { postcodes });
    }

    /// <summary>
    /// Poll the status of an async bulk postcode job.
    /// </summary>
    /// <param name="jobId">The job ID returned by <see cref="SubmitBulkAsync"/>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task<BulkJobStatus<BulkPostcodeItem>> GetJobAsync(string jobId, CancellationToken cancellationToken = default)
    {
        return _transport.GetAsync<BulkJobStatus<BulkPostcodeItem>>(
            $"/postcode/bulk/jobs/{Uri.EscapeDataString(jobId)}", null, cancellationToken);
    }

    /// <param name="jobId">The job ID returned by <see cref="SubmitBulk"/>.</param>
    public BulkJobStatus<BulkPostcodeItem> GetJob(string jobId)
    {
        return _transport.Get<BulkJobStatus<BulkPostcodeItem>>(
            $"/postcode/bulk/jobs/{Uri.EscapeDataString(jobId)}");
    }

    /// <summary>
    /// Delete an async bulk postcode job and its results.
    /// </summary>
    /// <param name="jobId">The job ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task<DeletedJob> DeleteJobAsync(string jobId, CancellationToken cancellationToken = default)
    {
        return _transport.DeleteAsync<DeletedJob>(
            $"/postcode/bulk/jobs/{Uri.EscapeDataString(jobId)}", cancellationToken);
    }

    /// <param name="jobId">The job ID to delete.</param>
    public DeletedJob DeleteJob(string jobId)
    {
        return _transport.Delete<DeletedJob>(
            $"/postcode/bulk/jobs/{Uri.EscapeDataString(jobId)}");
    }

    private static string NormalisePostcode(string postcode)
    {
        return postcode.Trim().ToUpperInvariant();
    }

    private static string BuildNearestPath(double lat, double lon, int? radius)
    {
        var path = $"/postcode/nearest?lat={lat}&lon={lon}";
        if (radius.HasValue)
        {
            path += $"&radius={radius.Value}";
        }
        return path;
    }

    private static string BuildWithinPath(double lat, double lon, int? radius)
    {
        var path = $"/postcode/within?lat={lat}&lon={lon}";
        if (radius.HasValue)
        {
            path += $"&radius={radius.Value}";
        }
        return path;
    }
}
