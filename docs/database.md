# Database

This document describes how the DVLD application connects to and interacts with the database, based on the actual Data Access layer source code.

---

## Database Technology

- **Database engine:** Microsoft SQL Server
- **Client library:** `Microsoft.Data.SqlClient` 6.0.3 (NuGet package in `DVLD_DataAccess`)
- **Access method:** ADO.NET — `SqlConnection`, `SqlCommand`, `SqlDataReader`
- No ORM (Entity Framework or similar) is used

---

## Connection Configuration

The connection string is resolved by `clsDataAccessSettings` in `DVLD_DataAccess/clsDataAccessSettings.cs`.

### Resolution Order

1. **Environment variable** `DVLD_CONNECTION_STRING` (checked first)
2. **`dvld.env` file** — searched in:
   - The application's base directory (next to the `.exe`)
   - The current working directory (useful during development / F5 launch)
3. **Fail-fast** — if neither source provides a non-empty value, an `InvalidOperationException` is thrown immediately with a clear message

### Fail-Fast Behavior

```
DVLD database connection string is not configured.
Set the 'DVLD_CONNECTION_STRING' environment variable,
or create a 'dvld.env' file next to the executable with the line:
  DVLD_CONNECTION_STRING=Server=<host>;Database=DVLD;User Id=<user>;Password=<pass>;TrustServerCertificate=True;
See dvld.env.example for a template. Never commit real credentials.
```

### Connection String Format

```
DVLD_CONNECTION_STRING=Server=<host>,<port>;Database=DVLD;User Id=<user>;Password=<password>;TrustServerCertificate=True;
```

> **Security:** Never hardcode real credentials. Never commit `dvld.env`. See [configuration.md](configuration.md) and [authentication-security.md](authentication-security.md).

---

## Data Access Layer Pattern

Every data class in `DVLD_DataAccess` follows the same pattern:

```csharp
SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString);
string query = "... parameterized SQL ...";
SqlCommand command = new SqlCommand(query, connection);
command.Parameters.AddWithValue("@ParamName", value);

try
{
    connection.Open();
    // ExecuteReader / ExecuteScalar / ExecuteNonQuery
}
catch (Exception ex)
{
    // return false or -1
}
finally
{
    connection.Close();
}
```

Key characteristics:
- **Parameterized queries** — all user-supplied values are added as `SqlParameter` via `AddWithValue`; no string concatenation of user input into SQL
- **Open/close per operation** — each method opens and closes its own connection
- **No transactions** — no explicit `SqlTransaction` was found in the current codebase
- **No connection pooling configuration** — relies on ADO.NET's default connection pool

### ADO.NET Methods Used

| Method | Purpose |
|---|---|
| `ExecuteReader()` | SELECT queries returning one or more rows |
| `ExecuteScalar()` | INSERT ... SELECT SCOPE_IDENTITY() — returns the new row's identity |
| `ExecuteNonQuery()` | UPDATE, DELETE — returns rows affected count |

---

## Database Entities

The following tables and views are referenced in the Data Access layer source code. Schema details are inferred from column names accessed in the code.

### Users

Referenced by: `clsUserData`

| Column | Type (inferred) | Notes |
|---|---|---|
| `UserID` | int (PK) | Identity |
| `PersonID` | int (FK → People) | |
| `UserName` | string | Unique |
| `Password` | string | Stored as plaintext (see [authentication-security.md](authentication-security.md)) |
| `IsActive` | bool | Account active flag |

Operations: `GetUserInfoByUserID`, `GetUserInfoByPersonID`, `GetUserInfoByUsernameAndPassword`, `AddNewUser`, `UpdateUser`, `DeleteUser`, `IsUserExist` (by ID/username/PersonID), `ChangePassword`

---

### People

Referenced by: `clsPersonData`

| Column | Type (inferred) | Notes |
|---|---|---|
| `PersonID` | int (PK) | Identity |
| `FirstName` | string | Required |
| `SecondName` | string | Required |
| `ThirdName` | string | Nullable |
| `LastName` | string | Required |
| `NationalNo` | string | Unique national identifier |
| `DateOfBirth` | DateTime | |
| `Gendor` | byte/short | 0 = Male, else Female (from SQL CASE in GetAllPeople) |
| `Address` | string | |
| `Phone` | string | |
| `Email` | string | Nullable |
| `NationalityCountryID` | int (FK → Countries) | |
| `ImagePath` | string | Nullable; file path to person photo |

Operations: `GetPersonInfoByID`, `GetPersonInfoByNationalNo`, `AddNewPerson`, `UpdatePerson`, `GetAllPeople`, `DeletePerson`, `IsPersonExist`

---

### Countries

Referenced by: `clsPersonData` (JOIN in GetAllPeople), `clsCountryData`

| Column | Type (inferred) | Notes |
|---|---|---|
| `CountryID` | int (PK) | |
| `CountryName` | string | |

---

### Drivers

Referenced by: `clsDriverData`

| Column | Type (inferred) | Notes |
|---|---|---|
| `DriverID` | int (PK) | Identity |
| `PersonID` | int (FK → People) | |
| `CreatedByUserID` | int (FK → Users) | |
| `CreatedDate` | DateTime | |

Operations: `GetDriverInfoByDriverID`, `GetDriverInfoByPersonID`, `AddNewDriver`, `UpdateDriver`, `GetAllDrivers`

---

### Applications

Referenced by: `clsApplicationData`

| Column | Type (inferred) | Notes |
|---|---|---|
| `ApplicationID` | int (PK) | Identity |
| `ApplicantPersonID` | int (FK → People) | |
| `ApplicationDate` | DateTime | |
| `ApplicationTypeID` | int (FK → ApplicationTypes) | |
| `ApplicationStatus` | byte | 1=New, 2=Cancelled, 3=Completed |
| `LastStatusDate` | DateTime | |
| `PaidFees` | float | |
| `CreatedByUserID` | int (FK → Users) | |

Operations: `GetApplicationInfoByID`, `AddNewApplication`, `UpdateApplication`, `DeleteApplication`, `IsApplicationExist`, `UpdateStatus`, `DoesPersonHaveActiveApplication`, `GetActiveApplicationID`, `GetActiveApplicationIDForLicenseClass`

---

### ApplicationTypes

Referenced by: `clsApplicationTypeData` (`ApplicationType.cs`)

| Column | Type (inferred) | Notes |
|---|---|---|
| Application type metadata | | |

Known types (from `clsApplication.enApplicationType`): NewDrivingLicense(1), RenewDrivingLicense(2), ReplaceLostDrivingLicense(3), ReplaceDamagedDrivingLicense(4), ReleaseDetainedDrivingLicsense(5), NewInternationalLicense(6), RetakeTest(7)

---

### LocalDrivingLicenseApplications

Referenced by: `clsLocalDrivingLicenseApplicationData`

| Column | Type (inferred) | Notes |
|---|---|---|
| `LocalDrivingLicenseApplicationID` | int (PK) | Identity |
| `ApplicationID` | int (FK → Applications) | |
| `LicenseClassID` | int (FK → LicenseClasses) | |

---

### LicenseClasses

Referenced by: `clsLicenseClassData` (`LicenseClass.cs`)

| Column | Type (inferred) | Notes |
|---|---|---|
| `LicenseClassID` | int (PK) | |
| `ClassName` | string | |
| `ClassDescription` | string | |
| `MinimumAllowedAge` | byte | |
| `DefaultValidityLength` | byte | Years |
| `ClassFees` | float | |

---

### Licenses

Referenced by: `clsLicenseData`

| Column | Type (inferred) | Notes |
|---|---|---|
| `LicenseID` | int (PK) | Identity |
| `ApplicationID` | int (FK → Applications) | |
| `DriverID` | int (FK → Drivers) | |
| `LicenseClass` | int (FK → LicenseClasses) | |
| `IssueDate` | DateTime | |
| `ExpirationDate` | DateTime | |
| `Notes` | string | |
| `PaidFees` | float | |
| `IsActive` | bool | |
| `IssueReason` | byte | 1=FirstTime, 2=Renew, 3=DamagedReplacement, 4=LostReplacement |
| `CreatedByUserID` | int (FK → Users) | |

---

### InternationalLicenses

Referenced by: `clsInternationalLicenseData`

| Column | Type (inferred) | Notes |
|---|---|---|
| `InternationalLicenseID` | int (PK) | Identity |
| `ApplicationID` | int (FK → Applications) | |
| `DriverID` | int (FK → Drivers) | |
| `IssuedUsingLocalLicenseID` | int (FK → Licenses) | |
| `IssueDate` | DateTime | |
| `ExpirationDate` | DateTime | |
| `IsActive` | bool | |
| `CreatedByUserID` | int (FK → Users) | |

---

### DetainedLicenses

Referenced by: `clsDetainedLicenseData`

| Column | Type (inferred) | Notes |
|---|---|---|
| `DetainID` | int (PK) | Identity |
| `LicenseID` | int (FK → Licenses) | |
| `DetainDate` | DateTime | |
| `FineFees` | float | |
| `CreatedByUserID` | int (FK → Users) | |
| `IsReleased` | bool | |
| `ReleaseDate` | DateTime | Nullable |
| `ReleasedByUserID` | int (FK → Users) | Nullable |
| `ReleaseApplicationID` | int (FK → Applications) | Nullable |

Also uses a view: `detainedLicenses_View` (referenced in `GetAllDetainedLicenses`)

---

### TestTypes

Referenced by: `clsTestTypeData`

| Column | Type (inferred) | Notes |
|---|---|---|
| `TestTypeID` | int (PK) | 1=Vision, 2=Written, 3=Street |
| `TestTypeTitle` | string | |
| `TestTypeDescription` | string | |
| `TestTypeFees` | float | |

---

### TestAppointments / Tests

Referenced by: `clsTestAppointmentData`, `clsTestData`

These tables manage scheduled test appointments and their results. The specific column schema is not fully extracted here as the classes are complex, but they reference: `LocalDrivingLicenseApplicationID`, `TestTypeID`, appointment date/time, and test result (pass/fail).

---

## Historical SQL Bug Corrections

The following SQL bugs existed in the original .NET Framework codebase and were corrected during the .NET 9 migration. They are documented here as historical record only — the current code is correct.

### Bug 1 — `ChangePassword` (clsUserData)

**Original defect:** The SQL query used `Password = @Password` in the `SET` clause, but the parameter `@Password` was either missing or incorrectly named, causing the update to silently fail.

**Correction:** Verified that the parameter is correctly defined as `command.Parameters.AddWithValue("@Password", NewPassword)` with the query `set Password = @Password where UserID = @UserID`.

Current state: **Fixed** — both `@UserID` and `@Password` parameters are correctly bound.

---

### Bug 2 — `UpdateDetainedLicense` (clsDetainedLicenseData)

**Original defect:** The UPDATE query for `DetainedLicenses` had a SQL syntax or parameter mismatch — either an extra parameter was added that did not appear in the SQL, or a parameter in the SQL had no corresponding `AddWithValue` call.

**Correction:** The current implementation correctly matches the SQL `SET` clause columns (`LicenseID`, `DetainDate`, `FineFees`, `CreatedByUserID`) with their corresponding `AddWithValue` calls, and the `WHERE DetainID=@DetainID` clause with the `@DetainID` parameter.

Current state: **Fixed**.

---

### Bug 3 — `AddNewTestType` (clsTestTypeData)

**Original defect:** The `INSERT INTO TestTypes` statement had a column/parameter mismatch. The VALUES list used `@ApplicationFees` while the column was `TestTypeFees`, and the corresponding `AddWithValue` call used `"@ApplicationFees"` — a naming inconsistency that could cause confusion or runtime failure depending on SQL Server behavior.

**Current state:** The code uses `@ApplicationFees` for the parameter name in both the SQL VALUES clause and the `AddWithValue` call, making the mapping consistent. The SQL column `TestTypeFees` is correctly targeted.

> See [migration-to-dotnet-9.md](migration-to-dotnet-9.md) for context on when and why these were fixed.
