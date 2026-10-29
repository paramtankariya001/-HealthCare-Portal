USE HealthCarePortalDB;
GO
CREATE OR ALTER PROCEDURE dbo.usp_Report_AppointmentSummary 
    @FromDate DATE = NULL,
    @ToDate DATE = NULL 
AS 
BEGIN
    SET NOCOUNT ON;
    SELECT AppointmentStatus, COUNT_BIG(*) AS AppointmentCount 
    FROM dbo.Appointments 
    WHERE (@FromDate IS NULL OR AppointmentDate >= @FromDate) 
      AND (@ToDate IS NULL OR AppointmentDate <= @ToDate) 
    GROUP BY AppointmentStatus;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Report_PatientSummary 
    @FromDate DATE = NULL,
    @ToDate DATE = NULL 
AS 
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) AS NewPatients,
           COUNT(CASE WHEN Gender = N'Male' THEN 1 END) AS MalePatients,
           COUNT(CASE WHEN Gender = N'Female' THEN 1 END) AS FemalePatients 
    FROM dbo.Patients 
    WHERE (@FromDate IS NULL OR CONVERT(date, CreatedAt) >= @FromDate) 
      AND (@ToDate IS NULL OR CONVERT(date, CreatedAt) <= @ToDate);
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Report_RevenueSummary 
    @FromDate DATE = NULL,
    @ToDate DATE = NULL 
AS 
BEGIN
    SET NOCOUNT ON;
    SELECT CONVERT(date, PaymentDate) AS PaymentDate,
           SUM(Amount) AS Revenue 
    FROM dbo.Payments 
    WHERE PaymentStatus = N'Successful' 
      AND (@FromDate IS NULL OR CONVERT(date, PaymentDate) >= @FromDate) 
      AND (@ToDate IS NULL OR CONVERT(date, PaymentDate) <= @ToDate) 
    GROUP BY CONVERT(date, PaymentDate) 
    ORDER BY PaymentDate;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Report_DoctorPerformance 
    @FromDate DATE = NULL,
    @ToDate DATE = NULL 
AS 
BEGIN
    SET NOCOUNT ON;
    SELECT d.DoctorId,
           d.DoctorCode,
           d.FirstName,
           d.LastName,
           COUNT(a.AppointmentId) AS TotalAppointments,
           COUNT(CASE WHEN a.AppointmentStatus = N'Completed' THEN 1 END) AS CompletedAppointments,
           ISNULL(SUM(CASE WHEN a.AppointmentStatus = N'Completed' THEN a.ConsultationFee ELSE 0 END), 0) AS ConsultationRevenue 
    FROM dbo.Doctors d 
    LEFT JOIN dbo.Appointments a ON a.DoctorId = d.DoctorId 
      AND (@FromDate IS NULL OR a.AppointmentDate >= @FromDate) 
      AND (@ToDate IS NULL OR a.AppointmentDate <= @ToDate) 
    GROUP BY d.DoctorId, d.DoctorCode, d.FirstName, d.LastName;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Report_DepartmentStatistics 
    @FromDate DATE = NULL,
    @ToDate DATE = NULL 
AS 
BEGIN
    SET NOCOUNT ON;
    SELECT dep.DepartmentId,
           dep.DepartmentName,
           COUNT(a.AppointmentId) AS AppointmentCount,
           COUNT(CASE WHEN a.AppointmentStatus = N'Completed' THEN 1 END) AS CompletedCount 
    FROM dbo.Departments dep 
    LEFT JOIN dbo.Appointments a ON a.DepartmentId = dep.DepartmentId 
      AND (@FromDate IS NULL OR a.AppointmentDate >= @FromDate) 
      AND (@ToDate IS NULL OR a.AppointmentDate <= @ToDate) 
    GROUP BY dep.DepartmentId, dep.DepartmentName;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Report_BillingSummary 
    @FromDate DATE = NULL,
    @ToDate DATE = NULL 
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        COUNT(b.BillId) AS TotalBills,
        ISNULL(SUM(b.GrandTotal), 0) AS TotalBilled,
        ISNULL(SUM(p.PaidAmount), 0) AS TotalPaid,
        ISNULL(SUM(b.GrandTotal) - SUM(p.PaidAmount), 0) AS TotalOutstanding
    FROM dbo.Bills b
    OUTER APPLY (
        SELECT ISNULL(SUM(Amount), 0) AS PaidAmount 
        FROM dbo.Payments 
        WHERE BillId = b.BillId AND PaymentStatus = N'Successful'
    ) p
    WHERE (@FromDate IS NULL OR CONVERT(date, b.BillDate) >= @FromDate)
      AND (@ToDate IS NULL OR CONVERT(date, b.BillDate) <= @ToDate);
END;
GO
