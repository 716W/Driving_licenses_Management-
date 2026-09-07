# Testing

This document describes the current testing status and what is required to validate the DVLD application.

---

## Static Validation (Build-Level)

The following validations can be performed without a Windows machine or SQL Server:

### Package Restore

```bash
dotnet restore DVLD/DVLD.sln
```

Verifies that all NuGet packages (specifically `Microsoft.Data.SqlClient` 6.0.3) resolve correctly.

### Debug Build

```bash
dotnet build DVLD/DVLD.sln -p:EnableWindowsTargeting=true
```

Verifies that all C# source files compile without errors.

### Release Build

```bash
dotnet build DVLD/DVLD.sln -c Release -p:EnableWindowsTargeting=true
```

Verifies release mode compilation.

### Framework-Dependent Publish

```bash
dotnet publish DVLD/DVLD.csproj -c Release -r win-x64 --no-self-contained \
  -p:EnableWindowsTargeting=true \
  -o publish/win-x64-framework-dependent
```

Verifies that the FDD publish pipeline completes successfully.

### Self-Contained Publish

```bash
dotnet publish DVLD/DVLD.csproj -c Release -r win-x64 --self-contained \
  -p:EnableWindowsTargeting=true \
  -o publish/win-x64-self-contained
```

Verifies that the SCD publish pipeline (including bundling the runtime) completes successfully.

### Regression — Hardcoded Credentials Scan

Verify that no credentials were accidentally committed:

```bash
grep -rn "Password=" DVLD_DataAccess/clsDataAccessSettings.cs
grep -rn "Server=" DVLD/ DVLD_Buisness/ DVLD_DataAccess/ --include="*.cs"
```

Confirm that no real connection strings are embedded in source.

---

## Runtime Validation

> **Requires Windows + SQL Server.**

Runtime validation cannot be performed on Linux. The following items require execution of the application on a Windows machine:

- Login form loads and authenticates against the `Users` table
- Remember-Me pre-fills username correctly and does not pre-fill password
- Main form menu items open the correct forms
- Person CRUD operations (add, edit, delete, search)
- User CRUD operations (add, edit, delete, change password)
- Driver list displays correctly
- Local driving license application creation
- Test scheduling and result recording (Vision → Written → Street prerequisite enforcement)
- License issuance after passing all three tests
- License renewal
- License replacement (lost / damaged)
- License detention with fine fees
- Detained license release
- International license application
- Person license history display
- Application type management
- Test type management
- Sign-out and re-login

---

## Database Validation

All data operations require a running SQL Server with the DVLD database schema. This means:

1. SQL Server must be accessible from the Windows machine running the application
2. The `DVLD` database must exist with the correct schema (tables: `Users`, `People`, `Drivers`, `Applications`, `LocalDrivingLicenseApplications`, `Licenses`, `InternationalLicenses`, `DetainedLicenses`, `TestTypes`, `Tests`, `TestAppointments`, `LicenseClasses`, `ApplicationTypes`, `Countries`, and the view `detainedLicenses_View`)
3. `dvld.env` or `DVLD_CONNECTION_STRING` must be configured

---

## Current Test Coverage

> **No automated test suite was found or verified in the repository.**

There are no unit tests, integration tests, or UI tests in the current codebase. The solution contains only three projects: `DVLD`, `DVLD_Buisness`, and `DVLD_DataAccess`. No test projects exist.

---

## Recommended Future Tests

The following test areas are recommended as future work. These are recommendations only — not currently implemented.

| Test Area | Type | Priority |
|---|---|---|
| `clsUser.FindByUsernameAndPassword` — valid credentials | Unit/Integration | High |
| `clsUser.FindByUsernameAndPassword` — invalid credentials | Unit/Integration | High |
| `ChangePassword` — password updated in database | Integration | High |
| `clsDataAccessSettings` — env var resolution | Unit | High |
| `clsDataAccessSettings` — dvld.env file resolution | Unit | High |
| `clsDataAccessSettings` — fail-fast when neither configured | Unit | High |
| `clsGlobal.RememberUsernameAndPassword` — only username persisted | Unit | High |
| `clsGlobal.GetStoredCredential` — password always returned empty | Unit | High |
| Person CRUD operations | Integration | Medium |
| User CRUD operations | Integration | Medium |
| Local DL application workflow (create → tests → issue license) | Integration | Medium |
| Test prerequisite enforcement | Unit | Medium |
| License renew / replace / detain / release | Integration | Medium |
| International license application | Integration | Low |
| Application type / test type / license class management | Integration | Low |

### Recommended Test Framework

For future test projects targeting this codebase:
- **Unit tests:** xUnit or NUnit
- **Mocking:** Moq or NSubstitute
- **Integration tests:** Require a real SQL Server instance (e.g., SQL Server in Docker for CI)
