using System;
using System.Threading;
using System.Threading.Tasks;
using Zyfy.Internal;
using Zyfy.Models;

namespace Zyfy.Resources;

/// <summary>
/// Provides vehicle intelligence lookups. Access via <see cref="ZyfyClient.Vehicle"/>.
/// </summary>
public sealed class VehicleResource
{
    private readonly HttpTransport _transport;

    internal VehicleResource(HttpTransport transport)
    {
        _transport = transport;
    }

    /// <summary>
    /// Look up a single UK vehicle by registration mark.
    /// Returns DVLA details, DVSA MOT history, and computed intelligence signals.
    /// </summary>
    /// <param name="registration">UK registration mark. Normalised to uppercase with spaces removed.</param>
    /// <param name="maxEnrichmentRetries">Per-call override for enrichment retry behaviour.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task<VehicleResult> LookupAsync(
        string registration,
        int? maxEnrichmentRetries = null,
        CancellationToken cancellationToken = default)
    {
        var reg = NormaliseReg(registration);
        return _transport.GetAsync<VehicleResult>($"/vehicle/{Uri.EscapeDataString(reg)}", maxEnrichmentRetries, cancellationToken);
    }

    /// <param name="registration">UK registration mark. Normalised to uppercase with spaces removed.</param>
    /// <param name="maxEnrichmentRetries">Per-call override for enrichment retry behaviour.</param>
    public VehicleResult Lookup(string registration, int? maxEnrichmentRetries = null)
    {
        var reg = NormaliseReg(registration);
        return _transport.Get<VehicleResult>($"/vehicle/{Uri.EscapeDataString(reg)}", maxEnrichmentRetries);
    }

    /// <summary>
    /// Bulk vehicle lookup. Up to your tier's bulk cap per call. Returns all results in a single response.
    /// </summary>
    /// <param name="registrations">Registration marks to look up.</param>
    /// <param name="maxEnrichmentRetries">Per-call override for enrichment retry behaviour.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task<BulkVehicleResult> BulkLookupAsync(
        string[] registrations,
        int? maxEnrichmentRetries = null,
        CancellationToken cancellationToken = default)
    {
        return _transport.PostAsync<BulkVehicleResult>("/vehicle/bulk", new { registrations }, maxEnrichmentRetries, cancellationToken);
    }

    /// <param name="registrations">Registration marks to look up.</param>
    /// <param name="maxEnrichmentRetries">Per-call override for enrichment retry behaviour.</param>
    public BulkVehicleResult BulkLookup(string[] registrations, int? maxEnrichmentRetries = null)
    {
        return _transport.Post<BulkVehicleResult>("/vehicle/bulk", new { registrations }, maxEnrichmentRetries);
    }

    /// <summary>
    /// Submit an async bulk vehicle job. Returns a <see cref="BulkJobSubmitted"/> with a
    /// <c>JobId</c> to poll with <see cref="GetJobAsync"/>. The job expires 24 hours after submission.
    /// </summary>
    /// <param name="registrations">Registration marks to look up.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task<BulkJobSubmitted> SubmitBulkAsync(string[] registrations, CancellationToken cancellationToken = default)
    {
        return _transport.PostAsync<BulkJobSubmitted>("/vehicle/bulk/async", new { registrations }, null, cancellationToken);
    }

    /// <param name="registrations">Registration marks to look up.</param>
    public BulkJobSubmitted SubmitBulk(string[] registrations)
    {
        return _transport.Post<BulkJobSubmitted>("/vehicle/bulk/async", new { registrations });
    }

    /// <summary>
    /// Poll the status of an async bulk vehicle job. Check <c>Status == "complete"</c> before reading results.
    /// </summary>
    /// <param name="jobId">The job ID returned by <see cref="SubmitBulkAsync"/>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task<BulkJobStatus<BulkVehicleItem>> GetJobAsync(string jobId, CancellationToken cancellationToken = default)
    {
        return _transport.GetAsync<BulkJobStatus<BulkVehicleItem>>(
            $"/vehicle/bulk/jobs/{Uri.EscapeDataString(jobId)}", null, cancellationToken);
    }

    /// <param name="jobId">The job ID returned by <see cref="SubmitBulk"/>.</param>
    public BulkJobStatus<BulkVehicleItem> GetJob(string jobId)
    {
        return _transport.Get<BulkJobStatus<BulkVehicleItem>>(
            $"/vehicle/bulk/jobs/{Uri.EscapeDataString(jobId)}");
    }

    /// <summary>
    /// Delete an async bulk vehicle job and its results.
    /// Results are automatically deleted 24 hours after job creation.
    /// </summary>
    /// <param name="jobId">The job ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task<DeletedJob> DeleteJobAsync(string jobId, CancellationToken cancellationToken = default)
    {
        return _transport.DeleteAsync<DeletedJob>(
            $"/vehicle/bulk/jobs/{Uri.EscapeDataString(jobId)}", cancellationToken);
    }

    /// <param name="jobId">The job ID to delete.</param>
    public DeletedJob DeleteJob(string jobId)
    {
        return _transport.Delete<DeletedJob>(
            $"/vehicle/bulk/jobs/{Uri.EscapeDataString(jobId)}");
    }

    private static string NormaliseReg(string registration)
    {
        return registration.ToUpperInvariant().Replace(" ", string.Empty);
    }
}
