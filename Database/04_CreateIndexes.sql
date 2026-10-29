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
CREATE INDEX IX_Users_RoleId ON dbo.Users(RoleId);
CREATE INDEX IX_Users_Email ON dbo.Users(Email);
CREATE INDEX IX_Patients_Phone ON dbo.Patients(Phone);
CREATE INDEX IX_Doctors_SpecializationId ON dbo.Doctors(SpecializationId);
CREATE INDEX IX_Doctors_Phone ON dbo.Doctors(Phone);
CREATE INDEX IX_DoctorSchedules_Doctor_Day ON dbo.DoctorSchedules(DoctorId, DayOfWeek, IsActive);
CREATE INDEX IX_Appointments_Patient_Date ON dbo.Appointments(PatientId, AppointmentDate);
CREATE INDEX IX_Appointments_Doctor_Date ON dbo.Appointments(DoctorId, AppointmentDate, StartTime, EndTime);
CREATE INDEX IX_Appointments_Status ON dbo.Appointments(AppointmentStatus);
CREATE INDEX IX_MedicalRecords_PatientId ON dbo.MedicalRecords(PatientId, RecordDate DESC);
CREATE INDEX IX_Prescriptions_PatientId ON dbo.Prescriptions(PatientId, PrescriptionDate DESC);
CREATE INDEX IX_Prescriptions_DoctorId ON dbo.Prescriptions(DoctorId);
CREATE INDEX IX_MedicalTests_PatientId ON dbo.MedicalTests(PatientId, TestDate DESC);
CREATE INDEX IX_MedicalTests_Status ON dbo.MedicalTests(TestStatus);
CREATE INDEX IX_Bills_PatientId ON dbo.Bills(PatientId, BillDate DESC);
CREATE INDEX IX_Bills_PaymentStatus ON dbo.Bills(PaymentStatus);
CREATE INDEX IX_Payments_BillId ON dbo.Payments(BillId);
CREATE INDEX IX_Notifications_User_Read ON dbo.Notifications(UserId, IsRead, CreatedAt DESC);
CREATE INDEX IX_AuditLogs_UserId ON dbo.AuditLogs(UserId);
CREATE INDEX IX_AuditLogs_CreatedAt ON dbo.AuditLogs(CreatedAt DESC);
GO
