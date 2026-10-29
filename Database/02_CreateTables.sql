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

CREATE TABLE dbo.Roles (
    RoleId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL CONSTRAINT UQ_Roles_RoleName UNIQUE,
    Description NVARCHAR(250) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Roles_IsActive DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Roles_CreatedAt DEFAULT SYSDATETIME()
);

CREATE TABLE dbo.Users (
    UserId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
    RoleId INT NOT NULL,
    Username NVARCHAR(100) NOT NULL CONSTRAINT UQ_Users_Username UNIQUE,
    Email NVARCHAR(150) NOT NULL CONSTRAINT UQ_Users_Email UNIQUE,
    PasswordHash NVARCHAR(500) NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT 1,
    LastLoginAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL
);

CREATE TABLE dbo.Departments (
    DepartmentId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Departments PRIMARY KEY,
    DepartmentName NVARCHAR(100) NOT NULL CONSTRAINT UQ_Departments_Name UNIQUE,
    Description NVARCHAR(500) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Departments_IsActive DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Departments_CreatedAt DEFAULT SYSDATETIME()
);

CREATE TABLE dbo.Specializations (
    SpecializationId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Specializations PRIMARY KEY,
    DepartmentId INT NOT NULL,
    SpecializationName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Specializations_IsActive DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Specializations_CreatedAt DEFAULT SYSDATETIME(),
    CONSTRAINT UQ_Specializations_Department_Name UNIQUE (DepartmentId, SpecializationName)
);

CREATE TABLE dbo.Patients (
    PatientId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Patients PRIMARY KEY,
    UserId INT NULL,
    PatientCode NVARCHAR(30) NOT NULL CONSTRAINT UQ_Patients_Code UNIQUE,
    FirstName NVARCHAR(100) NOT NULL, LastName NVARCHAR(100) NULL,
    Gender NVARCHAR(20) NOT NULL, DateOfBirth DATE NOT NULL,
    BloodGroup NVARCHAR(10) NULL, Phone NVARCHAR(20) NOT NULL,
    Email NVARCHAR(150) NULL, Address NVARCHAR(500) NULL,
    EmergencyContactName NVARCHAR(150) NULL, EmergencyContactPhone NVARCHAR(20) NULL,
    Allergies NVARCHAR(1000) NULL, MedicalHistory NVARCHAR(2000) NULL,
    ProfileImage NVARCHAR(500) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Patients_IsActive DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Patients_CreatedAt DEFAULT SYSDATETIME(),
    UpdatedAt DATETIME2 NULL,
    CONSTRAINT CK_Patients_Gender CHECK (Gender IN (N'Male', N'Female', N'Other')),
    CONSTRAINT CK_Patients_BloodGroup CHECK (BloodGroup IS NULL OR BloodGroup IN (N'A+',N'A-',N'B+',N'B-',N'AB+',N'AB-',N'O+',N'O-'))
);

CREATE TABLE dbo.Doctors (
    DoctorId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Doctors PRIMARY KEY,
    UserId INT NULL, DoctorCode NVARCHAR(30) NOT NULL CONSTRAINT UQ_Doctors_Code UNIQUE,
    FirstName NVARCHAR(100) NOT NULL, LastName NVARCHAR(100) NULL,
    Gender NVARCHAR(20) NOT NULL, DateOfBirth DATE NULL, Phone NVARCHAR(20) NOT NULL,
    Email NVARCHAR(150) NOT NULL, Qualification NVARCHAR(250) NULL,
    SpecializationId INT NOT NULL, ExperienceYears INT NOT NULL CONSTRAINT DF_Doctors_Experience DEFAULT 0,
    ConsultationFee DECIMAL(10,2) NOT NULL, AboutDoctor NVARCHAR(2000) NULL,
    ProfileImage NVARCHAR(500) NULL, IsActive BIT NOT NULL CONSTRAINT DF_Doctors_IsActive DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Doctors_CreatedAt DEFAULT SYSDATETIME(), UpdatedAt DATETIME2 NULL,
    CONSTRAINT CK_Doctors_Gender CHECK (Gender IN (N'Male', N'Female', N'Other')),
    CONSTRAINT CK_Doctors_Experience CHECK (ExperienceYears >= 0),
    CONSTRAINT CK_Doctors_Fee CHECK (ConsultationFee >= 0)
);

CREATE TABLE dbo.Receptionists (
    ReceptionistId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Receptionists PRIMARY KEY,
    UserId INT NULL, EmployeeCode NVARCHAR(30) NOT NULL CONSTRAINT UQ_Receptionists_Code UNIQUE,
    FirstName NVARCHAR(100) NOT NULL, LastName NVARCHAR(100) NULL,
    Gender NVARCHAR(20) NOT NULL, Phone NVARCHAR(20) NOT NULL, Email NVARCHAR(150) NOT NULL,
    JoiningDate DATE NULL, ProfileImage NVARCHAR(500) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Receptionists_IsActive DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Receptionists_CreatedAt DEFAULT SYSDATETIME(),
    CONSTRAINT CK_Receptionists_Gender CHECK (Gender IN (N'Male', N'Female', N'Other'))
);

CREATE TABLE dbo.DoctorSchedules (
    ScheduleId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DoctorSchedules PRIMARY KEY,
    DoctorId INT NOT NULL, DayOfWeek TINYINT NOT NULL, StartTime TIME NOT NULL, EndTime TIME NOT NULL,
    BreakStartTime TIME NULL, BreakEndTime TIME NULL,
    SlotDurationMinutes INT NOT NULL CONSTRAINT DF_DoctorSchedules_Slot DEFAULT 30,
    IsActive BIT NOT NULL CONSTRAINT DF_DoctorSchedules_IsActive DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_DoctorSchedules_CreatedAt DEFAULT SYSDATETIME(),
    CONSTRAINT CK_DoctorSchedules_Day CHECK (DayOfWeek BETWEEN 1 AND 7),
    CONSTRAINT CK_DoctorSchedules_Time CHECK (EndTime > StartTime),
    CONSTRAINT CK_DoctorSchedules_Break CHECK ((BreakStartTime IS NULL AND BreakEndTime IS NULL) OR (BreakStartTime IS NOT NULL AND BreakEndTime IS NOT NULL AND BreakEndTime > BreakStartTime AND BreakStartTime >= StartTime AND BreakEndTime <= EndTime)),
    CONSTRAINT CK_DoctorSchedules_Slot CHECK (SlotDurationMinutes > 0)
);

CREATE TABLE dbo.Appointments (
    AppointmentId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Appointments PRIMARY KEY,
    AppointmentCode NVARCHAR(40) NOT NULL CONSTRAINT UQ_Appointments_Code UNIQUE,
    PatientId INT NOT NULL, DoctorId INT NOT NULL, DepartmentId INT NOT NULL,
    AppointmentDate DATE NOT NULL, StartTime TIME NOT NULL, EndTime TIME NOT NULL,
    ReasonForVisit NVARCHAR(1000) NULL,
    AppointmentStatus NVARCHAR(30) NOT NULL CONSTRAINT DF_Appointments_Status DEFAULT N'Pending',
    BookingSource NVARCHAR(30) NOT NULL CONSTRAINT DF_Appointments_Source DEFAULT N'Patient',
    ConsultationFee DECIMAL(10,2) NOT NULL, ReceptionistId INT NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Appointments_CreatedAt DEFAULT SYSDATETIME(), UpdatedAt DATETIME2 NULL,
    CONSTRAINT CK_Appointments_Time CHECK (EndTime > StartTime),
    CONSTRAINT CK_Appointments_Fee CHECK (ConsultationFee >= 0),
    CONSTRAINT CK_Appointments_Status CHECK (AppointmentStatus IN (N'Pending',N'Confirmed',N'Checked-In',N'In Consultation',N'Completed',N'Cancelled',N'No Show')),
    CONSTRAINT CK_Appointments_Source CHECK (BookingSource IN (N'Patient',N'Receptionist',N'Admin'))
);

CREATE TABLE dbo.Consultations (
    ConsultationId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Consultations PRIMARY KEY,
    AppointmentId BIGINT NOT NULL CONSTRAINT UQ_Consultations_Appointment UNIQUE,
    PatientId INT NOT NULL, DoctorId INT NOT NULL, Symptoms NVARCHAR(2000) NULL,
    BloodPressure NVARCHAR(20) NULL, Temperature DECIMAL(5,2) NULL, Weight DECIMAL(6,2) NULL,
    PulseRate INT NULL, Diagnosis NVARCHAR(2000) NULL, DoctorNotes NVARCHAR(4000) NULL,
    FollowUpDate DATE NULL, ConsultationDate DATETIME2 NOT NULL CONSTRAINT DF_Consultations_Date DEFAULT SYSDATETIME(),
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Consultations_CreatedAt DEFAULT SYSDATETIME(),
    CONSTRAINT CK_Consultations_Vitals CHECK ((Temperature IS NULL OR Temperature >= 0) AND (Weight IS NULL OR Weight >= 0) AND (PulseRate IS NULL OR PulseRate > 0))
);

CREATE TABLE dbo.MedicalRecords (
    MedicalRecordId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MedicalRecords PRIMARY KEY,
    PatientId INT NOT NULL, DoctorId INT NOT NULL, AppointmentId BIGINT NULL, ConsultationId BIGINT NULL,
    RecordType NVARCHAR(50) NOT NULL, RecordTitle NVARCHAR(200) NULL, Description NVARCHAR(4000) NULL,
    RecordDate DATE NOT NULL, CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_MedicalRecords_CreatedAt DEFAULT SYSDATETIME(),
    CONSTRAINT CK_MedicalRecords_Type CHECK (RecordType IN (N'Consultation',N'Diagnosis',N'Follow-Up',N'Medical History',N'Other'))
);

CREATE TABLE dbo.Prescriptions (
    PrescriptionId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Prescriptions PRIMARY KEY,
    PrescriptionCode NVARCHAR(40) NOT NULL CONSTRAINT UQ_Prescriptions_Code UNIQUE,
    PatientId INT NOT NULL, DoctorId INT NOT NULL, AppointmentId BIGINT NULL, ConsultationId BIGINT NULL,
    Diagnosis NVARCHAR(2000) NULL, Instructions NVARCHAR(3000) NULL, FollowUpDate DATE NULL,
    PrescriptionDate DATE NOT NULL CONSTRAINT DF_Prescriptions_Date DEFAULT CONVERT(date, GETDATE()),
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Prescriptions_CreatedAt DEFAULT SYSDATETIME()
);

CREATE TABLE dbo.PrescriptionItems (
    PrescriptionItemId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PrescriptionItems PRIMARY KEY,
    PrescriptionId BIGINT NOT NULL, MedicineName NVARCHAR(200) NOT NULL, Dosage NVARCHAR(100) NULL,
    Frequency NVARCHAR(100) NULL, Duration NVARCHAR(100) NULL, Instructions NVARCHAR(500) NULL
);

CREATE TABLE dbo.MedicalTests (
    MedicalTestId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MedicalTests PRIMARY KEY,
    TestCode NVARCHAR(40) NOT NULL CONSTRAINT UQ_MedicalTests_Code UNIQUE,
    PatientId INT NOT NULL, DoctorId INT NOT NULL, AppointmentId BIGINT NULL, TestName NVARCHAR(200) NOT NULL,
    Priority NVARCHAR(30) NOT NULL CONSTRAINT DF_MedicalTests_Priority DEFAULT N'Normal', Instructions NVARCHAR(1000) NULL,
    TestDate DATE NOT NULL, TestStatus NVARCHAR(40) NOT NULL CONSTRAINT DF_MedicalTests_Status DEFAULT N'Requested',
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_MedicalTests_CreatedAt DEFAULT SYSDATETIME(), UpdatedAt DATETIME2 NULL,
    CONSTRAINT CK_MedicalTests_Priority CHECK (Priority IN (N'Normal',N'Urgent',N'Critical')),
    CONSTRAINT CK_MedicalTests_Status CHECK (TestStatus IN (N'Requested',N'Sample Collected',N'Processing',N'Completed',N'Report Available',N'Cancelled'))
);

CREATE TABLE dbo.MedicalTestReports (
    ReportId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MedicalTestReports PRIMARY KEY,
    MedicalTestId BIGINT NOT NULL CONSTRAINT UQ_MedicalTestReports_Test UNIQUE,
    ReportTitle NVARCHAR(200) NULL, ResultSummary NVARCHAR(4000) NULL, ReportFilePath NVARCHAR(500) NULL,
    ReportedBy NVARCHAR(200) NULL, ReportDate DATE NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_MedicalTestReports_CreatedAt DEFAULT SYSDATETIME()
);

CREATE TABLE dbo.Bills (
    BillId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Bills PRIMARY KEY,
    InvoiceNumber NVARCHAR(40) NOT NULL CONSTRAINT UQ_Bills_Invoice UNIQUE,
    PatientId INT NOT NULL, AppointmentId BIGINT NULL, DoctorId INT NULL,
    BillDate DATETIME2 NOT NULL CONSTRAINT DF_Bills_BillDate DEFAULT SYSDATETIME(),
    Subtotal DECIMAL(12,2) NOT NULL CONSTRAINT DF_Bills_Subtotal DEFAULT 0,
    Discount DECIMAL(12,2) NOT NULL CONSTRAINT DF_Bills_Discount DEFAULT 0,
    Tax DECIMAL(12,2) NOT NULL CONSTRAINT DF_Bills_Tax DEFAULT 0,
    GrandTotal DECIMAL(12,2) NOT NULL CONSTRAINT DF_Bills_GrandTotal DEFAULT 0,
    PaymentStatus NVARCHAR(30) NOT NULL CONSTRAINT DF_Bills_PaymentStatus DEFAULT N'Pending',
    CreatedByUserId INT NULL, CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Bills_CreatedAt DEFAULT SYSDATETIME(),
    CONSTRAINT CK_Bills_Amounts CHECK (Subtotal >= 0 AND Discount >= 0 AND Tax >= 0 AND GrandTotal >= 0 AND Discount <= Subtotal + Tax),
    CONSTRAINT CK_Bills_Status CHECK (PaymentStatus IN (N'Pending',N'Partially Paid',N'Paid',N'Refunded'))
);

CREATE TABLE dbo.BillItems (
    BillItemId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BillItems PRIMARY KEY,
    BillId BIGINT NOT NULL, ServiceName NVARCHAR(200) NOT NULL, Description NVARCHAR(500) NULL,
    Quantity INT NOT NULL CONSTRAINT DF_BillItems_Quantity DEFAULT 1, UnitPrice DECIMAL(10,2) NOT NULL,
    TotalPrice AS (CONVERT(decimal(12,2), Quantity * UnitPrice)) PERSISTED,
    CONSTRAINT CK_BillItems_Quantity CHECK (Quantity > 0), CONSTRAINT CK_BillItems_UnitPrice CHECK (UnitPrice >= 0)
);

CREATE TABLE dbo.Payments (
    PaymentId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Payments PRIMARY KEY,
    TransactionCode NVARCHAR(50) NOT NULL CONSTRAINT UQ_Payments_Transaction UNIQUE,
    BillId BIGINT NOT NULL, PatientId INT NOT NULL, Amount DECIMAL(12,2) NOT NULL,
    PaymentMethod NVARCHAR(30) NOT NULL, PaymentDate DATETIME2 NOT NULL CONSTRAINT DF_Payments_Date DEFAULT SYSDATETIME(),
    PaymentStatus NVARCHAR(30) NOT NULL CONSTRAINT DF_Payments_Status DEFAULT N'Successful',
    ReferenceNumber NVARCHAR(100) NULL, Notes NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Payments_CreatedAt DEFAULT SYSDATETIME(),
    CONSTRAINT CK_Payments_Amount CHECK (Amount > 0), CONSTRAINT CK_Payments_Method CHECK (PaymentMethod IN (N'Cash',N'Card',N'UPI')),
    CONSTRAINT CK_Payments_Status CHECK (PaymentStatus IN (N'Successful',N'Pending',N'Failed',N'Refunded'))
);

CREATE TABLE dbo.Notifications (
    NotificationId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Notifications PRIMARY KEY,
    UserId INT NOT NULL, Title NVARCHAR(200) NOT NULL, Message NVARCHAR(1000) NOT NULL,
    NotificationType NVARCHAR(50) NOT NULL, IsRead BIT NOT NULL CONSTRAINT DF_Notifications_IsRead DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Notifications_CreatedAt DEFAULT SYSDATETIME(),
    CONSTRAINT CK_Notifications_Type CHECK (NotificationType IN (N'Appointment',N'Prescription',N'Medical Test',N'Payment',N'System',N'Reminder'))
);

CREATE TABLE dbo.AuditLogs (
    AuditLogId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditLogs PRIMARY KEY,
    UserId INT NULL, RoleName NVARCHAR(50) NULL, Action NVARCHAR(100) NOT NULL, ModuleName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(1000) NULL, RecordId NVARCHAR(100) NULL, IPAddress NVARCHAR(50) NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_AuditLogs_CreatedAt DEFAULT SYSDATETIME()
);
GO
