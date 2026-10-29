USE HealthCarePortalDB;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Bill_Create 
    @InvoiceNumber NVARCHAR(40),
    @PatientId INT,
    @Items dbo.BillItemTableType READONLY,
    @AppointmentId BIGINT = NULL,
    @DoctorId INT = NULL,
    @Discount DECIMAL(12,2) = 0,
    @Tax DECIMAL(12,2) = 0,
    @CreatedByUserId INT = NULL 
AS
BEGIN 
    SET NOCOUNT ON; 
    SET XACT_ABORT ON; 
    BEGIN TRY 
        BEGIN TRANSACTION; 

        IF NOT EXISTS(SELECT 1 FROM dbo.Patients WHERE PatientId = @PatientId AND IsActive = 1) 
            THROW 53001, N'Patient not found or inactive.', 1; 

        IF NOT EXISTS(SELECT 1 FROM @Items) 
            THROW 53002, N'At least one bill item is required.', 1; 

        IF @AppointmentId IS NOT NULL AND EXISTS(SELECT 1 FROM dbo.Bills WHERE AppointmentId = @AppointmentId)
            THROW 53007, N'A bill already exists for this appointment.', 1;

        DECLARE @Subtotal DECIMAL(12,2) = (SELECT SUM(Quantity * UnitPrice) FROM @Items);
        DECLARE @GrandTotal DECIMAL(12,2) = @Subtotal - @Discount + @Tax; 

        IF @Discount < 0 OR @Tax < 0 OR @GrandTotal < 0 OR @Discount > (@Subtotal + @Tax)
            THROW 53003, N'Invalid bill amounts or discount exceeds total.', 1; 

        INSERT dbo.Bills (InvoiceNumber, PatientId, AppointmentId, DoctorId, Subtotal, Discount, Tax, GrandTotal, CreatedByUserId) 
        VALUES (@InvoiceNumber, @PatientId, @AppointmentId, @DoctorId, @Subtotal, @Discount, @Tax, @GrandTotal, @CreatedByUserId); 

        DECLARE @BillId BIGINT = CONVERT(bigint, SCOPE_IDENTITY()); 

        INSERT dbo.BillItems (BillId, ServiceName, Description, Quantity, UnitPrice) 
        SELECT @BillId, ServiceName, Description, Quantity, UnitPrice 
        FROM @Items; 

        COMMIT; 
        SELECT @BillId AS BillId; 
    END TRY 
    BEGIN CATCH 
        IF XACT_STATE() <> 0 ROLLBACK; 
        THROW; 
    END CATCH 
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Bill_GetById 
    @BillId BIGINT 
AS 
BEGIN
    SET NOCOUNT ON;
    SELECT b.*,
           p.PatientCode,
           p.FirstName AS PatientFirstName,
           p.LastName AS PatientLastName,
           p.Phone AS PatientPhone,
           p.Email AS PatientEmail,
           p.Address AS PatientAddress,
           d.DoctorCode,
           d.FirstName AS DoctorFirstName,
           d.LastName AS DoctorLastName,
           dep.DepartmentName,
           ISNULL((SELECT SUM(Amount) FROM dbo.Payments WHERE BillId = b.BillId AND PaymentStatus = N'Successful'), 0) AS PaidAmount,
           b.GrandTotal - ISNULL((SELECT SUM(Amount) FROM dbo.Payments WHERE BillId = b.BillId AND PaymentStatus = N'Successful'), 0) AS BalanceAmount,
           i.BillItemId,
           i.ServiceName,
           i.Description,
           i.Quantity,
           i.UnitPrice,
           i.TotalPrice 
    FROM dbo.Bills b 
    JOIN dbo.Patients p ON p.PatientId = b.PatientId
    LEFT JOIN dbo.Doctors d ON d.DoctorId = b.DoctorId
    LEFT JOIN dbo.Specializations sp ON sp.SpecializationId = d.SpecializationId
    LEFT JOIN dbo.Departments dep ON dep.DepartmentId = sp.DepartmentId
    LEFT JOIN dbo.BillItems i ON i.BillId = b.BillId 
    WHERE b.BillId = @BillId;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Bill_GetInvoice 
    @InvoiceNumber NVARCHAR(40) 
AS 
BEGIN
    SET NOCOUNT ON;
    SELECT b.*,
           p.PatientCode,
           p.FirstName AS PatientFirstName,
           p.LastName AS PatientLastName,
           p.Phone AS PatientPhone,
           p.Email AS PatientEmail,
           p.Address AS PatientAddress,
           d.DoctorCode,
           d.FirstName AS DoctorFirstName,
           d.LastName AS DoctorLastName,
           dep.DepartmentName,
           ISNULL((SELECT SUM(Amount) FROM dbo.Payments WHERE BillId = b.BillId AND PaymentStatus = N'Successful'), 0) AS PaidAmount,
           b.GrandTotal - ISNULL((SELECT SUM(Amount) FROM dbo.Payments WHERE BillId = b.BillId AND PaymentStatus = N'Successful'), 0) AS BalanceAmount,
           i.BillItemId,
           i.ServiceName,
           i.Description,
           i.Quantity,
           i.UnitPrice,
           i.TotalPrice 
    FROM dbo.Bills b 
    JOIN dbo.Patients p ON p.PatientId = b.PatientId
    LEFT JOIN dbo.Doctors d ON d.DoctorId = b.DoctorId
    LEFT JOIN dbo.Specializations sp ON sp.SpecializationId = d.SpecializationId
    LEFT JOIN dbo.Departments dep ON dep.DepartmentId = sp.DepartmentId
    LEFT JOIN dbo.BillItems i ON i.BillId = b.BillId 
    WHERE b.InvoiceNumber = @InvoiceNumber;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Bill_GetByPatient 
    @PatientId INT 
AS 
BEGIN
    SET NOCOUNT ON;
    SELECT b.*,
           p.PatientCode,
           p.FirstName AS PatientFirstName,
           p.LastName AS PatientLastName,
           d.FirstName AS DoctorFirstName,
           d.LastName AS DoctorLastName,
           ISNULL((SELECT SUM(Amount) FROM dbo.Payments WHERE BillId = b.BillId AND PaymentStatus = N'Successful'), 0) AS PaidAmount,
           b.GrandTotal - ISNULL((SELECT SUM(Amount) FROM dbo.Payments WHERE BillId = b.BillId AND PaymentStatus = N'Successful'), 0) AS BalanceAmount
    FROM dbo.Bills b 
    JOIN dbo.Patients p ON p.PatientId = b.PatientId
    LEFT JOIN dbo.Doctors d ON d.DoctorId = b.DoctorId
    WHERE b.PatientId = @PatientId 
    ORDER BY b.BillDate DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Bill_GetAll 
AS 
BEGIN
    SET NOCOUNT ON;
    SELECT b.*,
           p.PatientCode,
           p.FirstName AS PatientFirstName,
           p.LastName AS PatientLastName,
           p.Phone AS PatientPhone,
           d.FirstName AS DoctorFirstName,
           d.LastName AS DoctorLastName,
           ISNULL((SELECT SUM(Amount) FROM dbo.Payments WHERE BillId = b.BillId AND PaymentStatus = N'Successful'), 0) AS PaidAmount,
           b.GrandTotal - ISNULL((SELECT SUM(Amount) FROM dbo.Payments WHERE BillId = b.BillId AND PaymentStatus = N'Successful'), 0) AS BalanceAmount
    FROM dbo.Bills b 
    JOIN dbo.Patients p ON p.PatientId = b.PatientId
    LEFT JOIN dbo.Doctors d ON d.DoctorId = b.DoctorId
    ORDER BY b.BillDate DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Bill_UpdatePaymentStatus 
    @BillId BIGINT,
    @PaymentStatus NVARCHAR(30) 
AS 
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Bills 
    SET PaymentStatus = @PaymentStatus 
    WHERE BillId = @BillId; 

    IF @@ROWCOUNT = 0 
        THROW 53004, N'Bill not found.', 1; 
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Payment_Create 
    @TransactionCode NVARCHAR(50),
    @BillId BIGINT,
    @PatientId INT,
    @Amount DECIMAL(12,2),
    @PaymentMethod NVARCHAR(30),
    @ReferenceNumber NVARCHAR(100) = NULL,
    @Notes NVARCHAR(500) = NULL 
AS 
BEGIN 
    SET NOCOUNT ON; 
    SET XACT_ABORT ON; 
    BEGIN TRY 
        BEGIN TRANSACTION; 

        DECLARE @Total DECIMAL(12,2), @Paid DECIMAL(12,2); 
        SELECT @Total = GrandTotal 
        FROM dbo.Bills WITH (UPDLOCK, HOLDLOCK) 
        WHERE BillId = @BillId AND PatientId = @PatientId; 

        IF @Total IS NULL 
            THROW 53005, N'Bill not found for patient.', 1; 

        SET @Paid = ISNULL((SELECT SUM(Amount) FROM dbo.Payments WITH (UPDLOCK, HOLDLOCK) WHERE BillId = @BillId AND PaymentStatus = N'Successful'), 0); 

        IF @Amount <= 0 OR @Amount > (@Total - @Paid) 
            THROW 53006, N'Payment exceeds the outstanding balance or is invalid.', 1; 

        INSERT dbo.Payments (TransactionCode, BillId, PatientId, Amount, PaymentMethod, ReferenceNumber, Notes) 
        VALUES (@TransactionCode, @BillId, @PatientId, @Amount, @PaymentMethod, @ReferenceNumber, @Notes); 

        DECLARE @NewPaymentId BIGINT = CAST(SCOPE_IDENTITY() AS BIGINT);
        SET @Paid = @Paid + @Amount; 

        UPDATE dbo.Bills 
        SET PaymentStatus = CASE WHEN @Paid >= GrandTotal THEN N'Paid' ELSE N'Partially Paid' END 
        WHERE BillId = @BillId; 

        COMMIT; 
        SELECT @NewPaymentId AS PaymentId, @Paid AS TotalPaid; 
    END TRY 
    BEGIN CATCH 
        IF XACT_STATE() <> 0 ROLLBACK; 
        THROW; 
    END CATCH 
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Payment_GetByBill 
    @BillId BIGINT 
AS 
BEGIN
    SET NOCOUNT ON;
    SELECT p.*,
           b.InvoiceNumber,
           pt.FirstName AS PatientFirstName,
           pt.LastName AS PatientLastName
    FROM dbo.Payments p
    JOIN dbo.Bills b ON b.BillId = p.BillId
    JOIN dbo.Patients pt ON pt.PatientId = p.PatientId
    WHERE p.BillId = @BillId 
    ORDER BY p.PaymentDate DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Payment_GetByPatient 
    @PatientId INT 
AS 
BEGIN
    SET NOCOUNT ON;
    SELECT p.*,
           b.InvoiceNumber,
           pt.FirstName AS PatientFirstName,
           pt.LastName AS PatientLastName
    FROM dbo.Payments p
    JOIN dbo.Bills b ON b.BillId = p.BillId
    JOIN dbo.Patients pt ON pt.PatientId = p.PatientId
    WHERE p.PatientId = @PatientId 
    ORDER BY p.PaymentDate DESC;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_Payment_GetAll 
AS 
BEGIN
    SET NOCOUNT ON;
    SELECT p.*,
           b.InvoiceNumber,
           pt.PatientCode,
           pt.FirstName AS PatientFirstName,
           pt.LastName AS PatientLastName
    FROM dbo.Payments p
    JOIN dbo.Bills b ON b.BillId = p.BillId
    JOIN dbo.Patients pt ON pt.PatientId = p.PatientId
    ORDER BY p.PaymentDate DESC;
END;
GO
