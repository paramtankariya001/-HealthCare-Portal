using HealthCare_Portal.Models;
using HealthCare_Portal.ViewModels;

namespace HealthCare_Portal.Services.Interfaces;

public interface IMedicalService
{
    // Consultation
    Task<OperationResult<long>> SaveConsultationAsync(int doctorId, ConsultationViewModel model, CancellationToken cancellationToken = default);
    Task<OperationResult<Consultation>> GetConsultationByAppointmentAsync(long appointmentId, int doctorId, CancellationToken cancellationToken = default);
    Task<OperationResult<IReadOnlyList<Consultation>>> GetConsultationsByPatientAsync(int patientId, int doctorId, CancellationToken cancellationToken = default);
    Task<OperationResult<IReadOnlyList<MedicalRecord>>> GetMedicalRecordsByPatientAsync(int patientId, CancellationToken cancellationToken = default);

    // Prescription
    Task<OperationResult<long>> CreatePrescriptionAsync(int doctorId, PrescriptionViewModel model, CancellationToken cancellationToken = default);
    Task<OperationResult<Prescription>> GetPrescriptionByIdAsync(long prescriptionId, int doctorId, CancellationToken cancellationToken = default);
    Task<OperationResult<IReadOnlyList<Prescription>>> GetPrescriptionsByDoctorAsync(int doctorId, CancellationToken cancellationToken = default);
    Task<OperationResult<IReadOnlyList<Prescription>>> GetPrescriptionsByPatientAsync(int patientId, int doctorId, CancellationToken cancellationToken = default);
    Task<OperationResult<IReadOnlyList<Prescription>>> GetPatientPrescriptionsAsync(int patientId, CancellationToken cancellationToken = default);

    // Medical Tests
    Task<OperationResult<long>> RequestMedicalTestAsync(int doctorId, long appointmentId, string testName, string priority, string? instructions, DateTime testDate, CancellationToken cancellationToken = default);
    Task<OperationResult<MedicalTest>> GetMedicalTestByIdAsync(long medicalTestId, int doctorId, CancellationToken cancellationToken = default);
    Task<OperationResult<IReadOnlyList<MedicalTest>>> GetMedicalTestsByDoctorAsync(int doctorId, CancellationToken cancellationToken = default);
    Task<OperationResult<IReadOnlyList<MedicalTest>>> GetMedicalTestsByPatientAsync(int patientId, int doctorId, CancellationToken cancellationToken = default);
}
