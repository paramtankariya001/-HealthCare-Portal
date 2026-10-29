using Microsoft.Data.SqlClient;
using HealthCare_Portal.Models;
using HealthCare_Portal.Repositories.Interfaces;
using HealthCare_Portal.Services.Interfaces;
using HealthCare_Portal.ViewModels;

namespace HealthCare_Portal.Services;

public sealed class MedicalService(
    IMedicalRepository medicalRepository,
    IAppointmentRepository appointmentRepository,
    ILogger<MedicalService> logger) : IMedicalService
{
    // Consultation
    public async Task<OperationResult<long>> SaveConsultationAsync(int doctorId, ConsultationViewModel model, CancellationToken ct = default)
    {
        if (doctorId <= 0)
            return OperationResult<long>.Fail("Invalid doctor session.");

        if (model.AppointmentId <= 0)
            return OperationResult<long>.Fail("Invalid appointment reference.");

        try
        {
            var appt = await appointmentRepository.GetByIdAsync(model.AppointmentId, ct);
            if (appt is null)
                return OperationResult<long>.Fail("Appointment not found.");

            if (appt.DoctorId != doctorId)
                return OperationResult<long>.Fail("You are not authorized to record a consultation for this appointment.");

            if (appt.AppointmentStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
                return OperationResult<long>.Fail("Cannot create a consultation for a cancelled appointment.");

            var existing = await medicalRepository.GetConsultationByAppointmentAsync(model.AppointmentId, ct);
            if (existing is not null)
                return OperationResult<long>.Fail("A consultation has already been recorded for this appointment.");

            var consultation = new Consultation
            {
                AppointmentId = model.AppointmentId,
                PatientId = appt.PatientId,
                DoctorId = doctorId,
                Symptoms = model.Symptoms,
                BloodPressure = model.BloodPressure,
                Temperature = model.Temperature,
                Weight = model.Weight,
                PulseRate = model.Pulse,
                Diagnosis = model.Diagnosis,
                DoctorNotes = model.Notes,
                FollowUpDate = model.FollowUpDate
            };

            var id = await medicalRepository.CreateConsultationAsync(consultation, ct);
            return OperationResult<long>.Ok(id, "Consultation record saved successfully.");
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to save consultation for appointment {AppointmentId}.", model.AppointmentId);
            return OperationResult<long>.Fail("Unable to save the consultation.");
        }
    }

    public async Task<OperationResult<Consultation>> GetConsultationByAppointmentAsync(long appointmentId, int doctorId, CancellationToken ct = default)
    {
        try
        {
            var consultation = await medicalRepository.GetConsultationByAppointmentAsync(appointmentId, ct);
            if (consultation is null)
                return OperationResult<Consultation>.Fail("No consultation record found for this appointment.");

            if (consultation.DoctorId != doctorId)
                return OperationResult<Consultation>.Fail("You are not authorized to view this consultation.");

            return OperationResult<Consultation>.Ok(consultation);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to retrieve consultation for appointment {AppointmentId}.", appointmentId);
            return OperationResult<Consultation>.Fail("Database error while loading consultation.");
        }
    }

    public async Task<OperationResult<IReadOnlyList<Consultation>>> GetConsultationsByPatientAsync(int patientId, int doctorId, CancellationToken ct = default)
    {
        try
        {
            if (!await DoctorHasPatientRelationshipAsync(patientId, doctorId, ct))
                return OperationResult<IReadOnlyList<Consultation>>.Fail("You are not authorized to view this patient's records.");

            var list = await medicalRepository.GetConsultationsByPatientAsync(patientId, ct);
            return OperationResult<IReadOnlyList<Consultation>>.Ok(list);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to retrieve consultations for patient {PatientId}.", patientId);
            return OperationResult<IReadOnlyList<Consultation>>.Fail("Database error while loading consultation history.");
        }
    }

    public async Task<OperationResult<IReadOnlyList<MedicalRecord>>> GetMedicalRecordsByPatientAsync(int patientId, CancellationToken ct = default)
    {
        try { return OperationResult<IReadOnlyList<MedicalRecord>>.Ok(await medicalRepository.GetMedicalRecordsByPatientAsync(patientId, ct)); }
        catch (SqlException ex) { logger.LogError(ex, "Failed to retrieve medical records for patient {PatientId}.", patientId); return OperationResult<IReadOnlyList<MedicalRecord>>.Fail("Database error while loading medical records."); }
    }

    // Prescription
    public async Task<OperationResult<long>> CreatePrescriptionAsync(int doctorId, PrescriptionViewModel model, CancellationToken ct = default)
    {
        if (doctorId <= 0)
            return OperationResult<long>.Fail("Invalid doctor session.");

        if (model.AppointmentId <= 0)
            return OperationResult<long>.Fail("Invalid appointment reference.");

        var validItems = model.PrescriptionItems.Where(i => !string.IsNullOrWhiteSpace(i.MedicineName)).ToList();
        if (validItems.Count == 0)
            return OperationResult<long>.Fail("At least one medicine is required to create a prescription.");

        try
        {
            var appt = await appointmentRepository.GetByIdAsync(model.AppointmentId, ct);
            if (appt is null)
                return OperationResult<long>.Fail("Appointment not found.");

            if (appt.DoctorId != doctorId)
                return OperationResult<long>.Fail("You are not authorized to prescribe for this appointment.");

            var consultation = await medicalRepository.GetConsultationByAppointmentAsync(model.AppointmentId, ct);
            long? consultationId = consultation?.ConsultationId;

            var code = $"RX-{Guid.NewGuid():N}"[..12].ToUpperInvariant();
            var id = await medicalRepository.CreatePrescriptionAsync(
                code,
                appt.PatientId,
                doctorId,
                appt.AppointmentId,
                consultationId,
                model.Diagnosis,
                model.Instructions,
                model.FollowUpDate,
                validItems,
                ct
            );

            return OperationResult<long>.Ok(id, "Digital prescription created successfully.");
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to create prescription for appointment {AppointmentId}.", model.AppointmentId);
            return OperationResult<long>.Fail("Unable to save the prescription.");
        }
    }

    public async Task<OperationResult<Prescription>> GetPrescriptionByIdAsync(long prescriptionId, int doctorId, CancellationToken ct = default)
    {
        try
        {
            var rx = await medicalRepository.GetPrescriptionByIdAsync(prescriptionId, ct);
            if (rx is null)
                return OperationResult<Prescription>.Fail("Prescription not found.");

            if (rx.DoctorId != doctorId)
                return OperationResult<Prescription>.Fail("You are not authorized to view this prescription.");

            return OperationResult<Prescription>.Ok(rx);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to retrieve prescription {PrescriptionId}.", prescriptionId);
            return OperationResult<Prescription>.Fail("Database error while loading prescription.");
        }
    }

    public async Task<OperationResult<IReadOnlyList<Prescription>>> GetPrescriptionsByDoctorAsync(int doctorId, CancellationToken ct = default)
    {
        try
        {
            var list = await medicalRepository.GetPrescriptionsByDoctorAsync(doctorId, ct);
            return OperationResult<IReadOnlyList<Prescription>>.Ok(list);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to retrieve prescriptions for doctor {DoctorId}.", doctorId);
            return OperationResult<IReadOnlyList<Prescription>>.Fail("Database error while loading prescriptions.");
        }
    }

    public async Task<OperationResult<IReadOnlyList<Prescription>>> GetPrescriptionsByPatientAsync(int patientId, int doctorId, CancellationToken ct = default)
    {
        try
        {
            if (!await DoctorHasPatientRelationshipAsync(patientId, doctorId, ct))
                return OperationResult<IReadOnlyList<Prescription>>.Fail("You are not authorized to view this patient's prescriptions.");

            var list = await medicalRepository.GetPrescriptionsByPatientAsync(patientId, ct);
            return OperationResult<IReadOnlyList<Prescription>>.Ok(list);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to retrieve prescriptions for patient {PatientId}.", patientId);
            return OperationResult<IReadOnlyList<Prescription>>.Fail("Database error while loading patient prescriptions.");
        }
    }

    public async Task<OperationResult<IReadOnlyList<Prescription>>> GetPatientPrescriptionsAsync(int patientId, CancellationToken ct = default)
    {
        try { return OperationResult<IReadOnlyList<Prescription>>.Ok(await medicalRepository.GetPrescriptionsByPatientAsync(patientId, ct)); }
        catch (SqlException ex) { logger.LogError(ex, "Failed to retrieve prescriptions for patient {PatientId}.", patientId); return OperationResult<IReadOnlyList<Prescription>>.Fail("Database error while loading patient prescriptions."); }
    }

    // Medical Tests
    public async Task<OperationResult<long>> RequestMedicalTestAsync(int doctorId, long appointmentId, string testName, string priority, string? instructions, DateTime testDate, CancellationToken ct = default)
    {
        if (doctorId <= 0)
            return OperationResult<long>.Fail("Invalid doctor session.");

        if (string.IsNullOrWhiteSpace(testName))
            return OperationResult<long>.Fail("Medical test name is required.");

        try
        {
            var appt = await appointmentRepository.GetByIdAsync(appointmentId, ct);
            if (appt is null)
                return OperationResult<long>.Fail("Appointment not found.");

            if (appt.DoctorId != doctorId)
                return OperationResult<long>.Fail("You are not authorized to request tests for this appointment.");

            var testCode = $"LAB-{Guid.NewGuid():N}"[..12].ToUpperInvariant();
            var id = await medicalRepository.CreateMedicalTestAsync(
                testCode,
                appt.PatientId,
                doctorId,
                appointmentId,
                testName.Trim(),
                string.IsNullOrWhiteSpace(priority) ? "Normal" : priority.Trim(),
                instructions,
                testDate,
                ct
            );

            return OperationResult<long>.Ok(id, "Medical test request submitted successfully.");
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to request medical test for appointment {AppointmentId}.", appointmentId);
            return OperationResult<long>.Fail("Unable to submit the medical test request.");
        }
    }

    public async Task<OperationResult<MedicalTest>> GetMedicalTestByIdAsync(long medicalTestId, int doctorId, CancellationToken ct = default)
    {
        try
        {
            var test = await medicalRepository.GetMedicalTestByIdAsync(medicalTestId, ct);
            if (test is null)
                return OperationResult<MedicalTest>.Fail("Medical test request not found.");

            if (test.DoctorId != doctorId)
                return OperationResult<MedicalTest>.Fail("You are not authorized to view this test request.");

            return OperationResult<MedicalTest>.Ok(test);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to retrieve medical test {MedicalTestId}.", medicalTestId);
            return OperationResult<MedicalTest>.Fail("Database error while loading medical test.");
        }
    }

    public async Task<OperationResult<IReadOnlyList<MedicalTest>>> GetMedicalTestsByDoctorAsync(int doctorId, CancellationToken ct = default)
    {
        try
        {
            var list = await medicalRepository.GetMedicalTestsByDoctorAsync(doctorId, ct);
            return OperationResult<IReadOnlyList<MedicalTest>>.Ok(list);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to retrieve medical tests for doctor {DoctorId}.", doctorId);
            return OperationResult<IReadOnlyList<MedicalTest>>.Fail("Database error while loading medical tests.");
        }
    }

    public async Task<OperationResult<IReadOnlyList<MedicalTest>>> GetMedicalTestsByPatientAsync(int patientId, int doctorId, CancellationToken ct = default)
    {
        try
        {
            if (!await DoctorHasPatientRelationshipAsync(patientId, doctorId, ct))
                return OperationResult<IReadOnlyList<MedicalTest>>.Fail("You are not authorized to view this patient's tests.");

            var list = await medicalRepository.GetMedicalTestsByPatientAsync(patientId, ct);
            return OperationResult<IReadOnlyList<MedicalTest>>.Ok(list);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to retrieve medical tests for patient {PatientId}.", patientId);
            return OperationResult<IReadOnlyList<MedicalTest>>.Fail("Database error while loading patient medical tests.");
        }
    }

    private async Task<bool> DoctorHasPatientRelationshipAsync(int patientId, int doctorId, CancellationToken ct)
    {
        if (patientId <= 0 || doctorId <= 0)
            return false;

        var appointments = await appointmentRepository.GetByDoctorAsync(doctorId, cancellationToken: ct);
        return appointments.Any(a => a.PatientId == patientId);
    }
}
