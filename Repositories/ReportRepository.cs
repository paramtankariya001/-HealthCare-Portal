using HealthCare_Portal.Data;
using HealthCare_Portal.Repositories.Interfaces;

namespace HealthCare_Portal.Repositories;

public sealed class ReportRepository(DbHelper db) : IReportRepository
{
    public async Task<IReadOnlyList<Dictionary<string, object?>>> GetAppointmentSummaryAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var parameters = new[]
        {
            DbHelper.Parameter("@FromDate", from?.Date),
            DbHelper.Parameter("@ToDate", to?.Date)
        };

        return await db.ExecuteStoredProcedureAsync("dbo.usp_Report_AppointmentSummary", parameters, ct);
    }

    public async Task<Dictionary<string, object?>?> GetPatientSummaryAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var parameters = new[]
        {
            DbHelper.Parameter("@FromDate", from?.Date),
            DbHelper.Parameter("@ToDate", to?.Date)
        };

        var rows = await db.ExecuteStoredProcedureAsync("dbo.usp_Report_PatientSummary", parameters, ct);
        return rows.Count > 0 ? rows[0] : null;
    }

    public async Task<IReadOnlyList<Dictionary<string, object?>>> GetRevenueSummaryAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var parameters = new[]
        {
            DbHelper.Parameter("@FromDate", from?.Date),
            DbHelper.Parameter("@ToDate", to?.Date)
        };

        return await db.ExecuteStoredProcedureAsync("dbo.usp_Report_RevenueSummary", parameters, ct);
    }

    public async Task<IReadOnlyList<Dictionary<string, object?>>> GetDoctorPerformanceAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var parameters = new[]
        {
            DbHelper.Parameter("@FromDate", from?.Date),
            DbHelper.Parameter("@ToDate", to?.Date)
        };

        return await db.ExecuteStoredProcedureAsync("dbo.usp_Report_DoctorPerformance", parameters, ct);
    }

    public async Task<IReadOnlyList<Dictionary<string, object?>>> GetDepartmentStatisticsAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var parameters = new[]
        {
            DbHelper.Parameter("@FromDate", from?.Date),
            DbHelper.Parameter("@ToDate", to?.Date)
        };

        return await db.ExecuteStoredProcedureAsync("dbo.usp_Report_DepartmentStatistics", parameters, ct);
    }

    public async Task<Dictionary<string, object?>?> GetBillingSummaryAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var parameters = new[]
        {
            DbHelper.Parameter("@FromDate", from?.Date),
            DbHelper.Parameter("@ToDate", to?.Date)
        };

        var rows = await db.ExecuteStoredProcedureAsync("dbo.usp_Report_BillingSummary", parameters, ct);
        return rows.Count > 0 ? rows[0] : null;
    }
}
