using System.Data;
using Microsoft.Data.SqlClient;
using HealthCare_Portal.Data;
using HealthCare_Portal.Models;
using HealthCare_Portal.Repositories.Interfaces;
using HealthCare_Portal.ViewModels;

namespace HealthCare_Portal.Repositories;

public sealed class MedicalRepository(DbHelper db) : IMedicalRepository
{
    // Consultation
    public async Task<long> CreateConsultationAsync(Consultation c, CancellationToken ct = default)
    {
        var parameters = new[]
        {
            DbHelper.Parameter("@AppointmentId", c.AppointmentId),
            DbHelper.Parameter("@Symptoms", c.Symptoms),
            DbHelper.Parameter("@BloodPressure", c.BloodPressure),
            DbHelper.Parameter("@Temperature", c.Temperature),
            DbHelper.Parameter("@Weight", c.Weight),
            DbHelper.Parameter("@PulseRate", c.PulseRate),
            DbHelper.Parameter("@Diagnosis", c.Diagnosis),
            DbHelper.Parameter("@DoctorNotes", c.DoctorNotes),
            DbHelper.Parameter("@FollowUpDate", c.FollowUpDate?.Date)
        };

        var x = await db.ExecuteScalarAsync("dbo.usp_Consultation_Create", parameters, ct);
        return Convert.ToInt64(x);
    }

    public async Task<Consultation?> GetConsultationByAppointmentAsync(long appointmentId, CancellationToken ct = default)
    {
        var rows = await db.ExecuteStoredProcedureAsync("dbo.usp_Consultation_GetByAppointment", new[] {
            DbHelper.Parameter("@AppointmentId", appointmentId)
        }, ct);
        return rows.Count == 0 ? null : MapConsultation(rows[0]);
    }

    public async Task<IReadOnlyList<Consultation>> GetConsultationsByPatientAsync(int patientId, CancellationToken ct = default)
    {
        var rows = await db.ExecuteStoredProcedureAsync("dbo.usp_Consultation_GetByPatient", new[] {
            DbHelper.Parameter("@PatientId", patientId)
        }, ct);
        return rows.Select(MapConsultation).ToList();
    }

    public async Task<IReadOnlyList<Consultation>> GetConsultationsByDoctorAsync(int doctorId, CancellationToken ct = default)
    {
        var rows = await db.ExecuteStoredProcedureAsync("dbo.usp_Consultation_GetByDoctor", new[] {
            DbHelper.Parameter("@DoctorId", doctorId)
        }, ct);
        return rows.Select(MapConsultation).ToList();
    }

    public async Task<IReadOnlyList<MedicalRecord>> GetMedicalRecordsByPatientAsync(int patientId, CancellationToken ct = default)
    {
        var rows = await db.ExecuteStoredProcedureAsync("dbo.usp_MedicalRecord_GetByPatient", new[] { DbHelper.Parameter("@PatientId", patientId) }, ct);
        return rows.Select(r => new MedicalRecord
        {
            MedicalRecordId = Convert.ToInt64(r["MedicalRecordId"]), PatientId = Convert.ToInt32(r["PatientId"]), DoctorId = Convert.ToInt32(r["DoctorId"]),
            AppointmentId = r.GetValueOrDefault("AppointmentId") is null or DBNull ? null : Convert.ToInt64(r["AppointmentId"]),
            ConsultationId = r.GetValueOrDefault("ConsultationId") is null or DBNull ? null : Convert.ToInt64(r["ConsultationId"]),
            RecordType = Convert.ToString(r["RecordType"]) ?? "", RecordTitle = r.GetValueOrDefault("RecordTitle")?.ToString(), Description = r.GetValueOrDefault("Description")?.ToString(), RecordDate = Convert.ToDateTime(r["RecordDate"])
        }).ToList();
    }

    // Prescription
    public async Task<long> CreatePrescriptionAsync(string prescriptionCode, int patientId, int doctorId, long? appointmentId, long? consultationId, string? diagnosis, string? instructions, DateTime? followUpDate, IEnumerable<PrescriptionItemViewModel> items, CancellationToken ct = default)
    {
        var table = new DataTable();
        table.Columns.Add("MedicineName", typeof(string));
        table.Columns.Add("Dosage", typeof(string));
        table.Columns.Add("Frequency", typeof(string));
        table.Columns.Add("Duration", typeof(string));
        table.Columns.Add("Instructions", typeof(string));

        foreach (var item in items)
        {
            table.Rows.Add(
                item.MedicineName,
                (object?)item.Dosage ?? DBNull.Value,
                (object?)item.Frequency ?? DBNull.Value,
                (object?)item.Duration ?? DBNull.Value,
                (object?)item.Instructions ?? DBNull.Value
            );
        }

        var itemsParam = new SqlParameter("@Items", SqlDbType.Structured)
        {
            TypeName = "dbo.PrescriptionItemTableType",
            Value = table
        };

        var parameters = new[]
        {
            DbHelper.Parameter("@PrescriptionCode", prescriptionCode),
            DbHelper.Parameter("@PatientId", patientId),
            DbHelper.Parameter("@DoctorId", doctorId),
            itemsParam,
            DbHelper.Parameter("@AppointmentId", appointmentId),
            DbHelper.Parameter("@ConsultationId", consultationId),
            DbHelper.Parameter("@Diagnosis", diagnosis),
            DbHelper.Parameter("@Instructions", instructions),
            DbHelper.Parameter("@FollowUpDate", followUpDate?.Date)
        };

        var x = await db.ExecuteScalarAsync("dbo.usp_Prescription_Create", parameters, ct);
        return Convert.ToInt64(x);
    }

    public async Task<Prescription?> GetPrescriptionByIdAsync(long prescriptionId, CancellationToken ct = default)
    {
        var rows = await db.ExecuteStoredProcedureAsync("dbo.usp_Prescription_GetById", new[] {
            DbHelper.Parameter("@PrescriptionId", prescriptionId)
        }, ct);

        if (rows.Count == 0) return null;

        var rx = MapPrescription(rows[0]);
        foreach (var r in rows)
        {
            if (r.TryGetValue("PrescriptionItemId", out var itemId) && itemId != null && itemId != DBNull.Value)
            {
                rx.Items.Add(new PrescriptionItem
                {
                    PrescriptionItemId = Convert.ToInt64(itemId),
                    PrescriptionId = rx.PrescriptionId,
                    MedicineName = Convert.ToString(r["MedicineName"]) ?? "",
                    Dosage = r.GetValueOrDefault("Dosage")?.ToString(),
                    Frequency = r.GetValueOrDefault("Frequency")?.ToString(),
                    Duration = r.GetValueOrDefault("Duration")?.ToString(),
                    Instructions = r.GetValueOrDefault("Instructions")?.ToString()
                });
            }
        }
        return rx;
    }

    public async Task<IReadOnlyList<Prescription>> GetPrescriptionsByPatientAsync(int patientId, CancellationToken ct = default)
    {
        var rows = await db.ExecuteStoredProcedureAsync("dbo.usp_Prescription_GetByPatient", new[] {
            DbHelper.Parameter("@PatientId", patientId)
        }, ct);
        return rows.Select(MapPrescription).ToList();
    }

    public async Task<IReadOnlyList<Prescription>> GetPrescriptionsByDoctorAsync(int doctorId, CancellationToken ct = default)
    {
        var rows = await db.ExecuteStoredProcedureAsync("dbo.usp_Prescription_GetByDoctor", new[] {
            DbHelper.Parameter("@DoctorId", doctorId)
        }, ct);
        return rows.Select(MapPrescription).ToList();
    }

    // Medical Tests
    public async Task<long> CreateMedicalTestAsync(string testCode, int patientId, int doctorId, long? appointmentId, string testName, string priority, string? instructions, DateTime testDate, CancellationToken ct = default)
    {
        var parameters = new[]
        {
            DbHelper.Parameter("@TestCode", testCode),
            DbHelper.Parameter("@PatientId", patientId),
            DbHelper.Parameter("@DoctorId", doctorId),
            DbHelper.Parameter("@AppointmentId", appointmentId),
            DbHelper.Parameter("@TestName", testName),
            DbHelper.Parameter("@Priority", priority),
            DbHelper.Parameter("@Instructions", instructions),
            DbHelper.Parameter("@TestDate", testDate.Date)
        };

        var x = await db.ExecuteScalarAsync("dbo.usp_MedicalTest_Create", parameters, ct);
        return Convert.ToInt64(x);
    }

    public async Task<MedicalTest?> GetMedicalTestByIdAsync(long medicalTestId, CancellationToken ct = default)
    {
        var rows = await db.ExecuteStoredProcedureAsync("dbo.usp_MedicalTest_GetById", new[] {
            DbHelper.Parameter("@MedicalTestId", medicalTestId)
        }, ct);
        return rows.Count == 0 ? null : MapMedicalTest(rows[0]);
    }

    public async Task<IReadOnlyList<MedicalTest>> GetMedicalTestsByPatientAsync(int patientId, CancellationToken ct = default)
    {
        var rows = await db.ExecuteStoredProcedureAsync("dbo.usp_MedicalTest_GetByPatient", new[] {
            DbHelper.Parameter("@PatientId", patientId)
        }, ct);
        return rows.Select(MapMedicalTest).ToList();
    }

    public async Task<IReadOnlyList<MedicalTest>> GetMedicalTestsByDoctorAsync(int doctorId, CancellationToken ct = default)
    {
        var rows = await db.ExecuteStoredProcedureAsync("dbo.usp_MedicalTest_GetByDoctor", new[] {
            DbHelper.Parameter("@DoctorId", doctorId)
        }, ct);
        return rows.Select(MapMedicalTest).ToList();
    }

    public async Task<bool> UpdateMedicalTestStatusAsync(long medicalTestId, string status, CancellationToken ct = default)
    {
        await db.ExecuteNonQueryAsync("dbo.usp_MedicalTest_UpdateStatus", new[] {
            DbHelper.Parameter("@MedicalTestId", medicalTestId),
            DbHelper.Parameter("@TestStatus", status)
        }, ct);
        return true;
    }

    public async Task<MedicalTestReport?> GetMedicalTestReportAsync(long medicalTestId, CancellationToken ct = default)
    {
        var rows = await db.ExecuteStoredProcedureAsync("dbo.usp_MedicalTest_GetReport", new[] {
            DbHelper.Parameter("@MedicalTestId", medicalTestId)
        }, ct);
        if (rows.Count == 0) return null;
        var r = rows[0];
        return new MedicalTestReport
        {
            ReportId = Convert.ToInt64(r["ReportId"]),
            MedicalTestId = Convert.ToInt64(r["MedicalTestId"]),
            ReportTitle = r.GetValueOrDefault("ReportTitle")?.ToString(),
            ResultSummary = r.GetValueOrDefault("ResultSummary")?.ToString(),
            ReportFilePath = r.GetValueOrDefault("ReportFilePath")?.ToString(),
            ReportedBy = r.GetValueOrDefault("ReportedBy")?.ToString(),
            ReportDate = r.GetValueOrDefault("ReportDate") is null ? null : Convert.ToDateTime(r["ReportDate"])
        };
    }

    // Mapping Helpers
    private static Consultation MapConsultation(Dictionary<string, object?> r)
    {
        var c = new Consultation
        {
            ConsultationId = Convert.ToInt64(r["ConsultationId"]),
            AppointmentId = Convert.ToInt64(r["AppointmentId"]),
            PatientId = Convert.ToInt32(r["PatientId"]),
            DoctorId = Convert.ToInt32(r["DoctorId"]),
            Symptoms = r.GetValueOrDefault("Symptoms")?.ToString(),
            BloodPressure = r.GetValueOrDefault("BloodPressure")?.ToString(),
            Temperature = r.GetValueOrDefault("Temperature") is null ? null : Convert.ToDecimal(r["Temperature"]),
            Weight = r.GetValueOrDefault("Weight") is null ? null : Convert.ToDecimal(r["Weight"]),
            PulseRate = r.GetValueOrDefault("PulseRate") is null ? null : Convert.ToInt32(r["PulseRate"]),
            Diagnosis = r.GetValueOrDefault("Diagnosis")?.ToString(),
            DoctorNotes = r.GetValueOrDefault("DoctorNotes")?.ToString(),
            FollowUpDate = r.GetValueOrDefault("FollowUpDate") is null ? null : Convert.ToDateTime(r["FollowUpDate"]),
            ConsultationDate = r.TryGetValue("ConsultationDate", out var dt) && dt != null ? Convert.ToDateTime(dt) : DateTime.Now
        };

        if (r.TryGetValue("PatientFirstName", out var pf) && pf != null && pf != DBNull.Value)
            c.PatientName = $"{pf} {r.GetValueOrDefault("PatientLastName")}".Trim();
        if (r.TryGetValue("PatientCode", out var pc))
            c.PatientCode = pc?.ToString();
        if (r.TryGetValue("DoctorFirstName", out var df) && df != null && df != DBNull.Value)
            c.DoctorName = $"Dr. {df} {r.GetValueOrDefault("DoctorLastName")}".Trim();
        if (r.TryGetValue("DepartmentName", out var dn))
            c.DepartmentName = dn?.ToString();

        return c;
    }

    private static Prescription MapPrescription(Dictionary<string, object?> r)
    {
        var p = new Prescription
        {
            PrescriptionId = Convert.ToInt64(r["PrescriptionId"]),
            PrescriptionCode = Convert.ToString(r["PrescriptionCode"]) ?? "",
            PatientId = Convert.ToInt32(r["PatientId"]),
            DoctorId = Convert.ToInt32(r["DoctorId"]),
            AppointmentId = r.GetValueOrDefault("AppointmentId") is null ? null : Convert.ToInt64(r["AppointmentId"]),
            ConsultationId = r.GetValueOrDefault("ConsultationId") is null ? null : Convert.ToInt64(r["ConsultationId"]),
            Diagnosis = r.GetValueOrDefault("Diagnosis")?.ToString(),
            Instructions = r.GetValueOrDefault("Instructions")?.ToString(),
            FollowUpDate = r.GetValueOrDefault("FollowUpDate") is null ? null : Convert.ToDateTime(r["FollowUpDate"]),
            PrescriptionDate = r.TryGetValue("PrescriptionDate", out var dt) && dt != null ? Convert.ToDateTime(dt) : DateTime.Now
        };

        if (r.TryGetValue("PatientFirstName", out var pf) && pf != null && pf != DBNull.Value)
            p.PatientName = $"{pf} {r.GetValueOrDefault("PatientLastName")}".Trim();
        if (r.TryGetValue("PatientCode", out var pc))
            p.PatientCode = pc?.ToString();
        if (r.TryGetValue("DoctorFirstName", out var df) && df != null && df != DBNull.Value)
            p.DoctorName = $"Dr. {df} {r.GetValueOrDefault("DoctorLastName")}".Trim();

        return p;
    }

    private static MedicalTest MapMedicalTest(Dictionary<string, object?> r)
    {
        var t = new MedicalTest
        {
            MedicalTestId = Convert.ToInt64(r["MedicalTestId"]),
            TestCode = Convert.ToString(r["TestCode"]) ?? "",
            PatientId = Convert.ToInt32(r["PatientId"]),
            DoctorId = Convert.ToInt32(r["DoctorId"]),
            AppointmentId = r.GetValueOrDefault("AppointmentId") is null ? null : Convert.ToInt64(r["AppointmentId"]),
            TestName = Convert.ToString(r["TestName"]) ?? "",
            Priority = Convert.ToString(r["Priority"]) ?? "Normal",
            Instructions = r.GetValueOrDefault("Instructions")?.ToString(),
            TestDate = Convert.ToDateTime(r["TestDate"]),
            TestStatus = Convert.ToString(r["TestStatus"]) ?? "Requested"
        };

        if (r.TryGetValue("PatientFirstName", out var pf) && pf != null && pf != DBNull.Value)
            t.PatientName = $"{pf} {r.GetValueOrDefault("PatientLastName")}".Trim();
        if (r.TryGetValue("PatientCode", out var pc))
            t.PatientCode = pc?.ToString();
        if (r.TryGetValue("DoctorFirstName", out var df) && df != null && df != DBNull.Value)
            t.DoctorName = $"Dr. {df} {r.GetValueOrDefault("DoctorLastName")}".Trim();

        if (r.TryGetValue("ReportId", out var repId) && repId != null && repId != DBNull.Value)
        {
            t.Report = new MedicalTestReport
            {
                ReportId = Convert.ToInt64(repId),
                MedicalTestId = t.MedicalTestId,
                ReportTitle = r.GetValueOrDefault("ReportTitle")?.ToString(),
                ResultSummary = r.GetValueOrDefault("ResultSummary")?.ToString(),
                ReportFilePath = r.GetValueOrDefault("ReportFilePath")?.ToString(),
                ReportedBy = r.GetValueOrDefault("ReportedBy")?.ToString(),
                ReportDate = r.GetValueOrDefault("ReportDate") is null ? null : Convert.ToDateTime(r["ReportDate"])
            };
        }

        return t;
    }
}
