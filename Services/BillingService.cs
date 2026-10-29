using Microsoft.Data.SqlClient;
using HealthCare_Portal.Models;
using HealthCare_Portal.Repositories.Interfaces;
using HealthCare_Portal.Services.Interfaces;
using HealthCare_Portal.ViewModels;

namespace HealthCare_Portal.Services;

public sealed class BillingService(
    IBillingRepository repository,
    IPatientRepository patientRepo,
    IAppointmentRepository appointmentRepo,
    INotificationRepository notificationRepo,
    ILogger<BillingService> logger) : IBillingService
{
    private static readonly HashSet<string> AllowedPaymentMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "Cash", "Card", "UPI"
    };

    public async Task<OperationResult<long>> CreateBillAsync(BillViewModel request, int? createdByUserId = null, CancellationToken ct = default)
    {
        if (request.PatientId <= 0)
            return OperationResult<long>.Fail("Please select a valid patient.");

        if (request.BillItems == null || request.BillItems.Count == 0)
            return OperationResult<long>.Fail("At least one bill item is required.");

        foreach (var item in request.BillItems)
        {
            if (string.IsNullOrWhiteSpace(item.ServiceName))
                return OperationResult<long>.Fail("Each bill item must have a service name.");

            if (item.Quantity <= 0)
                return OperationResult<long>.Fail("Item quantity must be greater than zero.");

            if (item.UnitPrice < 0)
                return OperationResult<long>.Fail("Unit price cannot be negative.");
        }

        if (request.Discount < 0)
            return OperationResult<long>.Fail("Discount cannot be negative.");

        if (request.Tax < 0)
            return OperationResult<long>.Fail("Tax cannot be negative.");

        // Recalculate on server to ensure mathematical consistency
        var subtotal = request.BillItems.Sum(i => Math.Round(i.Quantity * i.UnitPrice, 2));
        var grandTotal = Math.Round(subtotal - request.Discount + request.Tax, 2);

        if (request.Discount > (subtotal + request.Tax))
            return OperationResult<long>.Fail("Discount cannot exceed the total amount.");

        if (grandTotal < 0)
            return OperationResult<long>.Fail("Grand total cannot be negative.");

        try
        {
            var doctorId = request.DoctorId;
            if (request.AppointmentId.HasValue)
            {
                if (request.AppointmentId.Value <= 0)
                    return OperationResult<long>.Fail("Invalid appointment reference.");

                var appointment = await appointmentRepo.GetByIdAsync(request.AppointmentId.Value, ct);
                if (appointment is null || appointment.PatientId != request.PatientId)
                    return OperationResult<long>.Fail("The selected appointment does not belong to the selected patient.");

                if (doctorId.HasValue && doctorId.Value != appointment.DoctorId)
                    return OperationResult<long>.Fail("The selected doctor does not match the appointment.");

                doctorId = appointment.DoctorId;
            }

            var invoiceNumber = $"INV-{DateTime.UtcNow:yyMMdd}-{Guid.NewGuid():N}"[..20].ToUpperInvariant();
            var billId = await repository.CreateBillAsync(
                invoiceNumber,
                request.PatientId,
                request.BillItems,
                request.AppointmentId,
                doctorId,
                request.Discount,
                request.Tax,
                createdByUserId,
                ct);

            // Notify patient if associated with a user account
            try
            {
                var patient = await patientRepo.GetByIdAsync(request.PatientId, ct);
                if (patient?.UserId != null)
                {
                    await notificationRepo.CreateAsync(
                        patient.UserId.Value,
                        "Invoice Generated",
                        $"Invoice {invoiceNumber} for ₹{grandTotal:N2} has been generated.",
                        "Payment",
                        ct);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send invoice notification to patient {PatientId}.", request.PatientId);
            }

            return OperationResult<long>.Ok(billId, $"Bill generated successfully. Invoice: {invoiceNumber}");
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to create bill for patient {PatientId}.", request.PatientId);
            return OperationResult<long>.Fail(ex.Message.Contains("already exists") 
                ? "A bill already exists for this appointment." 
                : "Unable to create the bill.");
        }
    }

    public async Task<OperationResult<Bill>> GetByIdAsync(long id, CancellationToken ct = default)
    {
        if (id <= 0)
            return OperationResult<Bill>.Fail("Invalid bill ID.");

        try
        {
            var bill = await repository.GetByIdAsync(id, ct);
            return bill is null
                ? OperationResult<Bill>.Fail("Bill information could not be found.")
                : OperationResult<Bill>.Ok(bill);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to get bill {BillId}.", id);
            return OperationResult<Bill>.Fail("Database error while loading bill.");
        }
    }

    public async Task<OperationResult<Bill>> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            return OperationResult<Bill>.Fail("Invalid invoice number.");

        try
        {
            var bill = await repository.GetByInvoiceNumberAsync(invoiceNumber.Trim(), ct);
            return bill is null
                ? OperationResult<Bill>.Fail("Invoice could not be found.")
                : OperationResult<Bill>.Ok(bill);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to get invoice {InvoiceNumber}.", invoiceNumber);
            return OperationResult<Bill>.Fail("Database error while loading invoice.");
        }
    }

    public async Task<OperationResult<IReadOnlyList<Bill>>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            var list = await repository.GetAllAsync(ct);
            return OperationResult<IReadOnlyList<Bill>>.Ok(list);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to retrieve all bills.");
            return OperationResult<IReadOnlyList<Bill>>.Fail("Failed to retrieve bills.");
        }
    }

    public async Task<OperationResult<IReadOnlyList<Bill>>> GetByPatientAsync(int patientId, CancellationToken ct = default)
    {
        if (patientId <= 0)
            return OperationResult<IReadOnlyList<Bill>>.Fail("Invalid patient ID.");

        try
        {
            var list = await repository.GetByPatientAsync(patientId, ct);
            return OperationResult<IReadOnlyList<Bill>>.Ok(list);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to get bills for patient {PatientId}.", patientId);
            return OperationResult<IReadOnlyList<Bill>>.Fail("Failed to retrieve patient bills.");
        }
    }

    public async Task<OperationResult<long>> RecordPaymentAsync(PaymentViewModel request, CancellationToken ct = default)
    {
        if (request.BillId <= 0)
            return OperationResult<long>.Fail("Please select a valid bill.");

        if (request.Amount <= 0)
            return OperationResult<long>.Fail("Payment amount must be greater than zero.");

        if (string.IsNullOrWhiteSpace(request.PaymentMethod) || !AllowedPaymentMethods.Contains(request.PaymentMethod))
            return OperationResult<long>.Fail("Please select a valid payment method (Cash, Card, UPI).");

        try
        {
            var bill = await repository.GetByIdAsync(request.BillId, ct);
            if (bill == null)
                return OperationResult<long>.Fail("Bill not found.");

            if (bill.BalanceAmount <= 0)
                return OperationResult<long>.Fail("This bill has already been fully paid.");

            if (request.Amount > bill.BalanceAmount)
                return OperationResult<long>.Fail($"Payment amount (₹{request.Amount:N2}) exceeds remaining balance of ₹{bill.BalanceAmount:N2}.");

            var transactionCode = $"TXN-{DateTime.UtcNow:yyMMdd}-{Guid.NewGuid():N}"[..20].ToUpperInvariant();
            var paymentId = await repository.CreatePaymentAsync(
                transactionCode,
                request.BillId,
                bill.PatientId,
                request.Amount,
                request.PaymentMethod,
                request.ReferenceNumber,
                request.Notes,
                ct);

            // Notify patient of payment receipt
            try
            {
                var patient = await patientRepo.GetByIdAsync(bill.PatientId, ct);
                if (patient?.UserId != null)
                {
                    await notificationRepo.CreateAsync(
                        patient.UserId.Value,
                        "Payment Receipt",
                        $"Payment of ₹{request.Amount:N2} received for Invoice {bill.InvoiceNumber} via {request.PaymentMethod}.",
                        "Payment",
                        ct);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send payment notification to patient {PatientId}.", bill.PatientId);
            }

            return OperationResult<long>.Ok(paymentId, $"Payment of ₹{request.Amount:N2} recorded successfully. Ref: {transactionCode}");
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to record payment for bill {BillId}.", request.BillId);
            return OperationResult<long>.Fail("Unable to record the payment. The bill may have changed or the amount may exceed its outstanding balance.");
        }
    }

    public async Task<OperationResult<IReadOnlyList<Payment>>> GetPaymentsByBillAsync(long billId, CancellationToken ct = default)
    {
        if (billId <= 0)
            return OperationResult<IReadOnlyList<Payment>>.Fail("Invalid bill ID.");

        try
        {
            var list = await repository.GetPaymentsByBillAsync(billId, ct);
            return OperationResult<IReadOnlyList<Payment>>.Ok(list);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to get payments for bill {BillId}.", billId);
            return OperationResult<IReadOnlyList<Payment>>.Fail("Failed to retrieve payments.");
        }
    }

    public async Task<OperationResult<IReadOnlyList<Payment>>> GetAllPaymentsAsync(CancellationToken ct = default)
    {
        try
        {
            var list = await repository.GetAllPaymentsAsync(ct);
            return OperationResult<IReadOnlyList<Payment>>.Ok(list);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Failed to retrieve all payments.");
            return OperationResult<IReadOnlyList<Payment>>.Fail("Failed to retrieve payments.");
        }
    }
}
