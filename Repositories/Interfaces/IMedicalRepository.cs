using HealthCare_Portal.Models;
using HealthCare_Portal.ViewModels;

namespace HealthCare_Portal.Repositories.Interfaces;

public interface IMedicalRepository
{
    // Consultation
    Task<long> CreateConsultationAsync(Consultation consultation, CancellationToken cancellationToken = default);
    Task<Consultation?> GetConsultationByAppointmentAsync(long appointmentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Consultation>> GetConsultationsByPatientAsync(int patientId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MedicalRecord>> GetMedicalRecordsByPatientAsync(int patientId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Consultation>> GetConsultationsByDoctorAsync(int doctorId, CancellationToken cancellationToken = default);

    // Prescription
    Task<long> CreatePrescriptionAsync(string prescriptionCode, int patientId, int doctorId, long? appointmentId, long? consultationId, string? diagnosis, string? instructions, DateTime? followUpDate, IEnumerable<PrescriptionItemViewModel> items, CancellationToken cancellationToken = default);
    Task<Prescription?> GetPrescriptionByIdAsync(long prescriptionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Prescription>> GetPrescriptionsByPatientAsync(int patientId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Prescription>> GetPrescriptionsByDoctorAsync(int doctorId, CancellationToken cancellationToken = default);

    // Medical Tests
    Task<long> CreateMedicalTestAsync(string testCode, int patientId, int doctorId, long? appointmentId, string testName, string priority, string? instructions, DateTime testDate, CancellationToken cancellationToken = default);
    Task<MedicalTest?> GetMedicalTestByIdAsync(long medicalTestId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MedicalTest>> GetMedicalTestsByPatientAsync(int patientId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MedicalTest>> GetMedicalTestsByDoctorAsync(int doctorId, CancellationToken cancellationToken = default);
    Task<bool> UpdateMedicalTestStatusAsync(long medicalTestId, string status, CancellationToken cancellationToken = default);
    Task<MedicalTestReport?> GetMedicalTestReportAsync(long medicalTestId, CancellationToken cancellationToken = default);
}
