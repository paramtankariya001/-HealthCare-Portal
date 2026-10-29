namespace HealthCare_Portal.Repositories.Interfaces;

public interface IReportRepository
{
    Task<IReadOnlyList<Dictionary<string, object?>>> GetAppointmentSummaryAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<Dictionary<string, object?>?> GetPatientSummaryAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Dictionary<string, object?>>> GetRevenueSummaryAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Dictionary<string, object?>>> GetDoctorPerformanceAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Dictionary<string, object?>>> GetDepartmentStatisticsAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<Dictionary<string, object?>?> GetBillingSummaryAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
}
