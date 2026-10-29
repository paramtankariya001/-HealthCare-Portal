using HealthCare_Portal.Models;
using HealthCare_Portal.ViewModels;

namespace HealthCare_Portal.Repositories.Interfaces;

public interface IBillingRepository
{
    Task<long> CreateBillAsync(string invoiceNumber, int patientId, IReadOnlyList<BillItemViewModel> items, long? appointmentId, int? doctorId, decimal discount, decimal tax, int? createdByUserId, CancellationToken cancellationToken = default);
    Task<Bill?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Bill?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Bill>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Bill>> GetByPatientAsync(int patientId, CancellationToken cancellationToken = default);
    Task<long> CreatePaymentAsync(string transactionCode, long billId, int patientId, decimal amount, string paymentMethod, string? referenceNumber, string? notes, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Payment>> GetPaymentsByBillAsync(long billId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Payment>> GetPaymentsByPatientAsync(int patientId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Payment>> GetAllPaymentsAsync(CancellationToken cancellationToken = default);
}
