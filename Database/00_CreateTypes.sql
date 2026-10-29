IF DB_ID(N'HealthCarePortalDB') IS NULL
BEGIN
    CREATE DATABASE HealthCarePortalDB;
END;
GO
USE HealthCarePortalDB;
GO
IF TYPE_ID(N'dbo.PrescriptionItemTableType') IS NULL
    EXEC(N'CREATE TYPE dbo.PrescriptionItemTableType AS TABLE (MedicineName NVARCHAR(200) NOT NULL,Dosage NVARCHAR(100) NULL,Frequency NVARCHAR(100) NULL,Duration NVARCHAR(100) NULL,Instructions NVARCHAR(500) NULL);');
GO
IF TYPE_ID(N'dbo.BillItemTableType') IS NULL
    EXEC(N'CREATE TYPE dbo.BillItemTableType AS TABLE (ServiceName NVARCHAR(200) NOT NULL,Description NVARCHAR(500) NULL,Quantity INT NOT NULL,UnitPrice DECIMAL(10,2) NOT NULL);');
GO
