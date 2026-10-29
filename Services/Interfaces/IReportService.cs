namespace HealthCare_Portal.Services.Interfaces;

public interface IReportService
{
    Task<OperationResult<IReadOnlyList<Dictionary<string, object?>>>> GetAppointmentSummaryAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<OperationResult<Dictionary<string, object?>?>> GetPatientSummaryAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<OperationResult<IReadOnlyList<Dictionary<string, object?>>>> GetRevenueSummaryAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<OperationResult<IReadOnlyList<Dictionary<string, object?>>>> GetDoctorPerformanceAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<OperationResult<IReadOnlyList<Dictionary<string, object?>>>> GetDepartmentStatisticsAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<OperationResult<Dictionary<string, object?>?>> GetBillingSummaryAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
}
