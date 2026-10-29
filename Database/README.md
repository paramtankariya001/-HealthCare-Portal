# HealthCarePortalDB database layer

Run the numbered SQL files in filename order in SQL Server Management Studio or with `sqlcmd`. Script `00_CreateTypes.sql` bootstraps the database so the type definitions are available before table-valued-parameter procedures are created; `01_CreateDatabase.sql` is intentionally idempotent afterward.

## Overview

The schema is normalized around users and roles, patient/doctor/receptionist profiles, departments and specializations, schedules and appointments, clinical records, prescriptions/tests, billing/payments, notifications, and audit logs. Profile and master entities use `IsActive` for soft deactivation. Foreign keys use non-cascading delete behavior so historical clinical and financial records are preserved.

## Tables and keys

| Table | Primary key | Main relationships |
|---|---|---|
| Roles | RoleId | Users |
| Users | UserId | Roles, profiles, notifications, audit logs |
| Departments | DepartmentId | Specializations, appointments |
| Specializations | SpecializationId | Departments, doctors |
| Patients | PatientId | Users, appointments, clinical and billing records |
| Doctors | DoctorId | Users, specializations, schedules, clinical records |
| Receptionists | ReceptionistId | Users, appointments |
| DoctorSchedules | ScheduleId | Doctors |
| Appointments | AppointmentId | Patients, doctors, departments, receptionists |
| Consultations | ConsultationId | One appointment, patient, doctor |
| MedicalRecords | MedicalRecordId | Patient, doctor, optional appointment/consultation |
| Prescriptions | PrescriptionId | Patient, doctor, optional appointment/consultation |
| PrescriptionItems | PrescriptionItemId | Prescriptions |
| MedicalTests | MedicalTestId | Patient, doctor, optional appointment |
| MedicalTestReports | ReportId | One medical test |
| Bills | BillId | Patient, optional appointment/doctor/user |
| BillItems | BillItemId | Bills |
| Payments | PaymentId | Bills and patients |
| Notifications | NotificationId | Users |
| AuditLogs | AuditLogId | Optional user |

The complete FK definitions are in `03_CreateConstraints.sql`. Important one-to-one rules are enforced by unique keys on consultation-to-appointment and report-to-test. Specialization names are unique within a department.

## Workflows

- **Appointments:** availability is generated from active doctor schedules in slot-duration increments. Booking validates active entities, future date, weekday, working interval, breaks, and overlap. `UPDLOCK`/`HOLDLOCK` plus a transaction closes the concurrent-booking race. Cancelled and no-show appointments do not block a slot.
- **Billing:** `usp_Bill_Create` accepts a structured item list, calculates subtotal and grand total in a transaction, and inserts the bill and items atomically. `usp_Payment_Create` locks the bill, rejects overpayment, inserts the payment, and updates the payment status.
- **Prescriptions:** `usp_Prescription_Create` accepts a structured item list and commits the prescription and all items together.
- **Medical records:** consultations capture an appointment's patient and doctor, then can be referenced by medical records, prescriptions, and tests. Records are never physically deleted by the supplied procedures.

## Procedures

- `14_CreateAuthProcedures.sql`: login.
- `15_CreatePatientProcedures.sql`: patient profile, search, dashboard, appointments, history, prescriptions, tests, bills.
- `16_CreateDoctorProcedures.sql`: doctor search/management, schedules, dashboard, appointments, patients.
- `17_CreateReceptionistProcedures.sql`: receptionist dashboard, patient management, booking, check-in, availability.
- `18_CreateAdminProcedures.sql`: administrative read dashboards and collections.
- `19_CreateAppointmentProcedures.sql`: slots, booking, conflict checks, status, cancellation, rescheduling, queries.
- `20_CreateMedicalProcedures.sql`: consultations, records, prescriptions, tests, reports.
- `21_CreateBillingProcedures.sql`: bills, bill items, payments, payment status.
- `22_CreateNotificationProcedures.sql`: create/read/mark notifications.
- `23_CreateReportProcedures.sql`: appointment, patient, revenue, doctor, and department reports.
- `24_CreateAuditProcedures.sql`: audit creation and retrieval.
- `26_CreateApplicationProcedures.sql`: application authentication context and patient registration.

## Indexing and security

`04_CreateIndexes.sql` supports login, role filtering, profile lookup, doctor availability, appointment timelines/status, patient history, billing, notifications, and audit reporting. Passwords are represented only by `PasswordHash`. Sample users in `07_InsertSampleUsers.sql` use a PBKDF2 hash of `Password@123`. Run `27_EnsureDemoAdmin.sql` for local demonstration so the Admin account authenticates as `admin` / `admin`. Change that default password before production deployment. The login procedure compares a supplied hash value and does not hash or compare plaintext. All procedures are parameterized and avoid dynamic SQL.

## Test entry point

Run `25_TestQueries.sql` after all scripts. It covers connection, login, doctor searches, availability, appointment reads and commented booking/conflict calls, clinical, billing, dashboards, reports, and audit retrieval. The repository should pass TVP parameters for prescription and bill creation using `dbo.PrescriptionItemTableType` and `dbo.BillItemTableType`.
