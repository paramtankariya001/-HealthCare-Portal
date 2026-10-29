using System.Data;
using Microsoft.Data.SqlClient;
using HealthCare_Portal.Data;
using HealthCare_Portal.Models;
using HealthCare_Portal.Repositories.Interfaces;
using HealthCare_Portal.ViewModels;

namespace HealthCare_Portal.Repositories;

public sealed class BillingRepository(DbHelper db) : IBillingRepository
{
    public async Task<long> CreateBillAsync(
        string invoiceNumber,
        int patientId,
        IReadOnlyList<BillItemViewModel> items,
        long? appointmentId,
        int? doctorId,
        decimal discount,
        decimal tax,
        int? createdByUserId,
        CancellationToken ct = default)
    {
        var table = new DataTable();
        table.Columns.Add("ServiceName", typeof(string));
        table.Columns.Add("Description", typeof(string));
        table.Columns.Add("Quantity", typeof(int));
        table.Columns.Add("UnitPrice", typeof(decimal));

        foreach (var item in items)
        {
            table.Rows.Add(
                item.ServiceName,
                (object?)item.Description ?? DBNull.Value,
                item.Quantity,
                item.UnitPrice
            );
        }

        var itemsParam = new SqlParameter("@Items", SqlDbType.Structured)
        {
            TypeName = "dbo.BillItemTableType",
            Value = table
        };

        var parameters = new[]
        {
            DbHelper.Parameter("@InvoiceNumber", invoiceNumber),
            DbHelper.Parameter("@PatientId", patientId),
            itemsParam,
            DbHelper.Parameter("@AppointmentId", appointmentId),
            DbHelper.Parameter("@DoctorId", doctorId),
            DbHelper.Parameter("@Discount", discount),
            DbHelper.Parameter("@Tax", tax),
            DbHelper.Parameter("@CreatedByUserId", createdByUserId)
        };

        var result = await db.ExecuteScalarAsync("dbo.usp_Bill_Create", parameters, ct);
        return Convert.ToInt64(result);
    }

    public async Task<Bill?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var rows = await db.ExecuteStoredProcedureAsync("dbo.usp_Bill_GetById", new[] { DbHelper.Parameter("@BillId", id) }, ct);
        if (rows.Count == 0) return null;

        var bill = MapBill(rows[0]);
        bill.Items = rows
            .Where(r => r.ContainsKey("BillItemId") && r["BillItemId"] != null && r["BillItemId"] != DBNull.Value)
            .Select(MapBillItem)
            .ToList();

        // Also fetch payments for this bill
        bill.Payments = (await GetPaymentsByBillAsync(id, ct)).ToList();
        return bill;
    }

    public async Task<Bill?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken ct = default)
    {
        var rows = await db.ExecuteStoredProcedureAsync("dbo.usp_Bill_GetInvoice", new[] { DbHelper.Parameter("@InvoiceNumber", invoiceNumber) }, ct);
        if (rows.Count == 0) return null;

        var bill = MapBill(rows[0]);
        bill.Items = rows
            .Where(r => r.ContainsKey("BillItemId") && r["BillItemId"] != null && r["BillItemId"] != DBNull.Value)
            .Select(MapBillItem)
            .ToList();

        bill.Payments = (await GetPaymentsByBillAsync(bill.BillId, ct)).ToList();
        return bill;
    }

    public async Task<IReadOnlyList<Bill>> GetAllAsync(CancellationToken ct = default)
    {
        var rows = await db.ExecuteStoredProcedureAsync("dbo.usp_Bill_GetAll", Array.Empty<SqlParameter>(), ct);
        return rows.Select(MapBill).ToList();
    }

    public async Task<IReadOnlyList<Bill>> GetByPatientAsync(int patientId, CancellationToken ct = default)
    {
        var rows = await db.ExecuteStoredProcedureAsync("dbo.usp_Bill_GetByPatient", new[] { DbHelper.Parameter("@PatientId", patientId) }, ct);
        return rows.Select(MapBill).ToList();
    }

    public async Task<long> CreatePaymentAsync(
        string transactionCode,
        long billId,
        int patientId,
        decimal amount,
        string paymentMethod,
        string? referenceNumber,
        string? notes,
        CancellationToken ct = default)
    {
        var parameters = new[]
        {
            DbHelper.Parameter("@TransactionCode", transactionCode),
            DbHelper.Parameter("@BillId", billId),
            DbHelper.Parameter("@PatientId", patientId),
            DbHelper.Parameter("@Amount", amount),
            DbHelper.Parameter("@PaymentMethod", paymentMethod),
            DbHelper.Parameter("@ReferenceNumber", referenceNumber),
            DbHelper.Parameter("@Notes", notes)
        };

        var result = await db.ExecuteScalarAsync("dbo.usp_Payment_Create", parameters, ct);
        return Convert.ToInt64(result);
    }

    public async Task<IReadOnlyList<Payment>> GetPaymentsByBillAsync(long billId, CancellationToken ct = default)
    {
        var rows = await db.ExecuteStoredProcedureAsync("dbo.usp_Payment_GetByBill", new[] { DbHelper.Parameter("@BillId", billId) }, ct);
        return rows.Select(MapPayment).ToList();
    }

    public async Task<IReadOnlyList<Payment>> GetPaymentsByPatientAsync(int patientId, CancellationToken ct = default)
    {
        var rows = await db.ExecuteStoredProcedureAsync("dbo.usp_Payment_GetByPatient", new[] { DbHelper.Parameter("@PatientId", patientId) }, ct);
        return rows.Select(MapPayment).ToList();
    }

    public async Task<IReadOnlyList<Payment>> GetAllPaymentsAsync(CancellationToken ct = default)
    {
        var rows = await db.ExecuteStoredProcedureAsync("dbo.usp_Payment_GetAll", Array.Empty<SqlParameter>(), ct);
        return rows.Select(MapPayment).ToList();
    }

    private static Bill MapBill(Dictionary<string, object?> r)
    {
        var b = new Bill
        {
            BillId = Convert.ToInt64(r["BillId"]),
            InvoiceNumber = Convert.ToString(r["InvoiceNumber"]) ?? string.Empty,
            PatientId = Convert.ToInt32(r["PatientId"]),
            AppointmentId = r.ContainsKey("AppointmentId") && r["AppointmentId"] != null && r["AppointmentId"] != DBNull.Value ? Convert.ToInt64(r["AppointmentId"]) : null,
            DoctorId = r.ContainsKey("DoctorId") && r["DoctorId"] != null && r["DoctorId"] != DBNull.Value ? Convert.ToInt32(r["DoctorId"]) : null,
            Subtotal = Convert.ToDecimal(r["Subtotal"]),
            Discount = Convert.ToDecimal(r["Discount"]),
            Tax = Convert.ToDecimal(r["Tax"]),
            GrandTotal = Convert.ToDecimal(r["GrandTotal"]),
            PaymentStatus = Convert.ToString(r["PaymentStatus"]) ?? "Pending",
            BillDate = Convert.ToDateTime(r["BillDate"])
        };

        if (r.ContainsKey("PatientCode") && r["PatientCode"] != null)
            b.PatientCode = Convert.ToString(r["PatientCode"]);

        if (r.ContainsKey("PatientFirstName") && r["PatientFirstName"] != null)
        {
            var pFirst = Convert.ToString(r["PatientFirstName"]);
            var pLast = r.ContainsKey("PatientLastName") && r["PatientLastName"] != null ? Convert.ToString(r["PatientLastName"]) : "";
            b.PatientName = $"{pFirst} {pLast}".Trim();
        }

        if (r.ContainsKey("PatientPhone") && r["PatientPhone"] != null)
            b.PatientPhone = Convert.ToString(r["PatientPhone"]);

        if (r.ContainsKey("PatientEmail") && r["PatientEmail"] != null)
            b.PatientEmail = Convert.ToString(r["PatientEmail"]);

        if (r.ContainsKey("PatientAddress") && r["PatientAddress"] != null)
            b.PatientAddress = Convert.ToString(r["PatientAddress"]);

        if (r.ContainsKey("DoctorFirstName") && r["DoctorFirstName"] != null)
        {
            var dFirst = Convert.ToString(r["DoctorFirstName"]);
            var dLast = r.ContainsKey("DoctorLastName") && r["DoctorLastName"] != null ? Convert.ToString(r["DoctorLastName"]) : "";
            b.DoctorName = $"Dr. {dFirst} {dLast}".Trim();
        }

        if (r.ContainsKey("DepartmentName") && r["DepartmentName"] != null)
            b.DepartmentName = Convert.ToString(r["DepartmentName"]);

        if (r.ContainsKey("PaidAmount") && r["PaidAmount"] != null && r["PaidAmount"] != DBNull.Value)
            b.PaidAmount = Convert.ToDecimal(r["PaidAmount"]);

        return b;
    }

    private static BillItem MapBillItem(Dictionary<string, object?> r)
    {
        return new BillItem
        {
            BillItemId = Convert.ToInt64(r["BillItemId"]),
            BillId = Convert.ToInt64(r["BillId"]),
            ServiceName = Convert.ToString(r["ServiceName"]) ?? string.Empty,
            Description = r.ContainsKey("Description") && r["Description"] != null && r["Description"] != DBNull.Value ? Convert.ToString(r["Description"]) : null,
            Quantity = Convert.ToInt32(r["Quantity"]),
            UnitPrice = Convert.ToDecimal(r["UnitPrice"]),
            TotalPrice = Convert.ToDecimal(r["TotalPrice"])
        };
    }

    private static Payment MapPayment(Dictionary<string, object?> r)
    {
        var p = new Payment
        {
            PaymentId = Convert.ToInt64(r["PaymentId"]),
            TransactionCode = Convert.ToString(r["TransactionCode"]) ?? string.Empty,
            BillId = Convert.ToInt64(r["BillId"]),
            PatientId = Convert.ToInt32(r["PatientId"]),
            Amount = Convert.ToDecimal(r["Amount"]),
            PaymentMethod = Convert.ToString(r["PaymentMethod"]) ?? "Cash",
            PaymentDate = Convert.ToDateTime(r["PaymentDate"]),
            PaymentStatus = Convert.ToString(r["PaymentStatus"]) ?? "Successful",
            ReferenceNumber = r.ContainsKey("ReferenceNumber") && r["ReferenceNumber"] != null && r["ReferenceNumber"] != DBNull.Value ? Convert.ToString(r["ReferenceNumber"]) : null,
            Notes = r.ContainsKey("Notes") && r["Notes"] != null && r["Notes"] != DBNull.Value ? Convert.ToString(r["Notes"]) : null
        };

        if (r.ContainsKey("InvoiceNumber") && r["InvoiceNumber"] != null)
            p.InvoiceNumber = Convert.ToString(r["InvoiceNumber"]);

        if (r.ContainsKey("PatientCode") && r["PatientCode"] != null)
            p.PatientCode = Convert.ToString(r["PatientCode"]);

        if (r.ContainsKey("PatientFirstName") && r["PatientFirstName"] != null)
        {
            var pFirst = Convert.ToString(r["PatientFirstName"]);
            var pLast = r.ContainsKey("PatientLastName") && r["PatientLastName"] != null ? Convert.ToString(r["PatientLastName"]) : "";
            p.PatientName = $"{pFirst} {pLast}".Trim();
        }

        return p;
    }
}
