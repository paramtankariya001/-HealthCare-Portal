USE HealthCarePortalDB;
GO
INSERT dbo.Consultations (AppointmentId,PatientId,DoctorId,Symptoms,Diagnosis,DoctorNotes,FollowUpDate)
SELECT a.AppointmentId,a.PatientId,a.DoctorId,N'Routine symptoms',N'Initial assessment',N'Development consultation record',DATEADD(DAY,30,a.AppointmentDate)
FROM dbo.Appointments a WHERE a.AppointmentId IN (5,8,9,12,15,18,19,20);
INSERT dbo.MedicalRecords (PatientId,DoctorId,AppointmentId,ConsultationId,RecordType,RecordTitle,Description,RecordDate)
SELECT c.PatientId,c.DoctorId,c.AppointmentId,c.ConsultationId,N'Consultation',N'Initial consultation',N'Demo medical record',CONVERT(date,c.ConsultationDate) FROM dbo.Consultations c;
INSERT dbo.Prescriptions (PrescriptionCode,PatientId,DoctorId,AppointmentId,ConsultationId,Diagnosis,Instructions,FollowUpDate)
SELECT CONCAT(N'RX',RIGHT(CONCAT(N'000',c.ConsultationId),3)),c.PatientId,c.DoctorId,c.AppointmentId,c.ConsultationId,N'Initial diagnosis',N'Take medicines as directed',DATEADD(DAY,14,CONVERT(date,c.ConsultationDate)) FROM dbo.Consultations c;
INSERT dbo.PrescriptionItems (PrescriptionId,MedicineName,Dosage,Frequency,Duration,Instructions)
SELECT p.PrescriptionId,v.MedicineName,N'1 tablet',v.Frequency,N'5 days',N'After food' FROM dbo.Prescriptions p JOIN (VALUES (N'Paracetamol',N'Twice daily'),(N'Omeprazole',N'Once daily')) v(MedicineName,Frequency) ON 1=1;
INSERT dbo.MedicalTests (TestCode,PatientId,DoctorId,AppointmentId,TestName,Priority,Instructions,TestDate,TestStatus)
SELECT CONCAT(N'TST',RIGHT(CONCAT(N'000',a.AppointmentId),3)),a.PatientId,a.DoctorId,a.AppointmentId,v.TestName,v.Priority,N'Provide sample at laboratory',a.AppointmentDate,v.Status FROM dbo.Appointments a JOIN (VALUES (1,N'CBC',N'Normal',N'Requested'),(2,N'ECG',N'Urgent',N'Completed'),(3,N'Blood Sugar',N'Normal',N'Report Available'),(4,N'X-Ray',N'Normal',N'Processing'),(5,N'Lipid Profile',N'Critical',N'Completed'),(6,N'Urine Test',N'Normal',N'Requested'),(7,N'MRI Brain',N'Urgent',N'Report Available'),(8,N'Dental X-Ray',N'Normal',N'Completed'),(9,N'Thyroid Panel',N'Normal',N'Requested'),(10,N'Vitamin D',N'Normal',N'Completed')) v(N,TestName,Priority,Status) ON v.N=a.AppointmentId WHERE a.AppointmentId<=10;
INSERT dbo.MedicalTestReports (MedicalTestId,ReportTitle,ResultSummary,ReportedBy,ReportDate) SELECT MedicalTestId,CONCAT(TestName,N' Report'),N'No critical abnormality in demo result',N'Laboratory',TestDate FROM dbo.MedicalTests WHERE TestStatus IN (N'Completed',N'Report Available');
GO
