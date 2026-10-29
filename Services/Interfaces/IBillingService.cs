using HealthCare_Portal.Models;
using HealthCare_Portal.ViewModels;

namespace HealthCare_Portal.Services.Interfaces;

public interface IBillingService
{
    Task<OperationResult<long>> CreateBillAsync(BillViewModel request, int? createdByUserId = null, CancellationToken cancellationToken = default);
    Task<OperationResult<Bill>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<OperationResult<Bill>> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default);
    Task<OperationResult<IReadOnlyList<Bill>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<OperationResult<IReadOnlyList<Bill>>> GetByPatientAsync(int patientId, CancellationToken cancellationToken = default);
    Task<OperationResult<long>> RecordPaymentAsync(PaymentViewModel request, CancellationToken cancellationToken = default);
    Task<OperationResult<IReadOnlyList<Payment>>> GetPaymentsByBillAsync(long billId, CancellationToken cancellationToken = default);
    Task<OperationResult<IReadOnlyList<Payment>>> GetAllPaymentsAsync(CancellationToken cancellationToken = default);
}
