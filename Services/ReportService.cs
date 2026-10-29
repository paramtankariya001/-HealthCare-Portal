using HealthCare_Portal.Models;
using HealthCare_Portal.Repositories.Interfaces;
using HealthCare_Portal.Services.Interfaces;

namespace HealthCare_Portal.Services;

public sealed class ReportService(
    IReportRepository repository,
    ILogger<ReportService> logger) : IReportService
{
    public async Task<OperationResult<IReadOnlyList<Dictionary<string, object?>>>> GetAppointmentSummaryAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        try
        {
            var data = await repository.GetAppointmentSummaryAsync(from, to, ct);
            return OperationResult<IReadOnlyList<Dictionary<string, object?>>>.Ok(data);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load appointment summary report.");
            return OperationResult<IReadOnlyList<Dictionary<string, object?>>>.Fail("Unable to generate appointment report.");
        }
    }

    public async Task<OperationResult<Dictionary<string, object?>?>> GetPatientSummaryAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        try
        {
            var data = await repository.GetPatientSummaryAsync(from, to, ct);
            return OperationResult<Dictionary<string, object?>?>.Ok(data);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load patient summary report.");
            return OperationResult<Dictionary<string, object?>?>.Fail("Unable to generate patient report.");
        }
    }

    public async Task<OperationResult<IReadOnlyList<Dictionary<string, object?>>>> GetRevenueSummaryAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        try
        {
            var data = await repository.GetRevenueSummaryAsync(from, to, ct);
            return OperationResult<IReadOnlyList<Dictionary<string, object?>>>.Ok(data);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load revenue summary report.");
            return OperationResult<IReadOnlyList<Dictionary<string, object?>>>.Fail("Unable to generate revenue report.");
        }
    }

    public async Task<OperationResult<IReadOnlyList<Dictionary<string, object?>>>> GetDoctorPerformanceAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        try
        {
            var data = await repository.GetDoctorPerformanceAsync(from, to, ct);
            return OperationResult<IReadOnlyList<Dictionary<string, object?>>>.Ok(data);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load doctor performance report.");
            return OperationResult<IReadOnlyList<Dictionary<string, object?>>>.Fail("Unable to generate doctor performance report.");
        }
    }

    public async Task<OperationResult<IReadOnlyList<Dictionary<string, object?>>>> GetDepartmentStatisticsAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        try
        {
            var data = await repository.GetDepartmentStatisticsAsync(from, to, ct);
            return OperationResult<IReadOnlyList<Dictionary<string, object?>>>.Ok(data);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load department statistics report.");
            return OperationResult<IReadOnlyList<Dictionary<string, object?>>>.Fail("Unable to generate department statistics.");
        }
    }

    public async Task<OperationResult<Dictionary<string, object?>?>> GetBillingSummaryAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        try
        {
            var data = await repository.GetBillingSummaryAsync(from, to, ct);
            return OperationResult<Dictionary<string, object?>?>.Ok(data);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load billing summary report.");
            return OperationResult<Dictionary<string, object?>?>.Fail("Unable to generate billing report.");
        }
    }
}
