USE HealthCarePortalDB;
GO
INSERT dbo.DoctorSchedules (DoctorId,DayOfWeek,StartTime,EndTime,BreakStartTime,BreakEndTime,SlotDurationMinutes)
SELECT DoctorId,v.DayOfWeek,'09:00','13:00','11:00','11:30',30 FROM dbo.Doctors d CROSS JOIN (VALUES (1),(3)) v(DayOfWeek);
GO
