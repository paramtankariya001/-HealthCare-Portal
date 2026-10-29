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
INSERT dbo.Bills (InvoiceNumber,PatientId,AppointmentId,DoctorId,Subtotal,Discount,Tax,GrandTotal,PaymentStatus,CreatedByUserId)
SELECT CONCAT(N'INV',RIGHT(CONCAT(N'000',a.AppointmentId),3)),a.PatientId,a.AppointmentId,a.DoctorId,a.ConsultationFee,0,0,a.ConsultationFee,CASE WHEN a.AppointmentId<=10 THEN N'Paid' ELSE N'Pending' END,(SELECT UserId FROM dbo.Users WHERE Username=N'admin') FROM dbo.Appointments a WHERE a.AppointmentId<=15;
INSERT dbo.BillItems (BillId,ServiceName,Description,Quantity,UnitPrice)
SELECT BillId,N'Consultation',N'OPD consultation',1,CONVERT(decimal(10,2),GrandTotal * 0.80) FROM dbo.Bills
UNION ALL
SELECT BillId,N'Facility charge',N'OPD facility and registration charge',1,CONVERT(decimal(10,2),GrandTotal - (GrandTotal * 0.80)) FROM dbo.Bills;
INSERT dbo.Payments (TransactionCode,BillId,PatientId,Amount,PaymentMethod,PaymentStatus,ReferenceNumber) SELECT CONCAT(N'TXN',RIGHT(CONCAT(N'000',BillId),3)),BillId,PatientId,GrandTotal,CASE WHEN BillId%2=0 THEN N'UPI' ELSE N'Card' END,N'Successful',CONCAT(N'DEMO-',BillId) FROM dbo.Bills WHERE BillId<=10;
INSERT dbo.Notifications (UserId,Title,Message,NotificationType) SELECT u.UserId,N'Appointment update',N'Demo appointment notification',N'Appointment' FROM dbo.Users u CROSS JOIN (VALUES(1),(2)) x(n) WHERE u.Username IN (N'patient1',N'patient2',N'patient3',N'patient4',N'patient5',N'patient6',N'patient7',N'patient8',N'patient9',N'patient10');
INSERT dbo.AuditLogs (UserId,RoleName,Action,ModuleName,Description,RecordId) SELECT u.UserId,r.RoleName,N'CREATE',N'Sample Data',N'Development audit record',CONVERT(nvarchar(100),u.UserId) FROM dbo.Users u JOIN dbo.Roles r ON r.RoleId=u.RoleId CROSS JOIN (VALUES(1),(2)) x(n);
GO
