# Known Limitations

This document lists verified limitations of the current DVLD application. Each limitation includes its status, impact, current workaround (if any), and recommended future action.

---

## Platform Limitations

### WinForms Requires Windows for Runtime Execution

| Property | Detail |
|---|---|
| **Status** | Architectural constraint — inherent to WinForms |
| **Impact** | The application cannot run on Linux or macOS |
| **Workaround** | Build on any OS (with `-p:EnableWindowsTargeting=true`); run only on Windows |
| **Future Action** | A cross-platform UI would require migrating to a different UI framework (e.g., MAUI, Avalonia, or a web front-end). This is a significant architectural change not planned in the current scope. |

---

## Database Limitations

### SQL Server Required

| Property | Detail |
|---|---|
| **Status** | By design — Microsoft SQL Server is the only supported database engine |
| **Impact** | Cannot use SQLite, PostgreSQL, or other database engines without rewriting the data access layer |
| **Workaround** | Use SQL Server Express (free) for development/testing |
| **Future Action** | Abstracting the data access layer behind interfaces and using an ORM (e.g., Entity Framework Core) would allow supporting other databases. Currently not planned. |

### No Database Schema in Repository

| Property | Detail |
|---|---|
| **Status** | No SQL schema files, migration scripts, or seed data files were found in the repository |
| **Impact** | A new developer must obtain the database schema and seed data from an external source |
| **Workaround** | Not verified in repository — *Not verified in repository* |
| **Future Action** | Add SQL schema scripts (CREATE TABLE statements) and seed data scripts to the repository to enable reproducible database setup |

---

## Testing Limitations

### No Automated Test Suite

| Property | Detail |
|---|---|
| **Status** | No test projects exist in the solution |
| **Impact** | Regressions can only be detected through manual testing |
| **Workaround** | Manual testing on Windows with a SQL Server instance |
| **Future Action** | Add xUnit or NUnit test projects. Priority areas: authentication, data access connection string resolution, and business logic (test prerequisites, license issuance workflow). See [testing.md](testing.md). |

### Runtime Validation Requires Windows + SQL Server

| Property | Detail |
|---|---|
| **Status** | Cannot execute WinForms on Linux/macOS |
| **Impact** | CI/CD pipelines on Linux cannot perform full end-to-end testing |
| **Workaround** | Use a Windows build agent or run manual tests on Windows |
| **Future Action** | For CI, use a Windows runner (GitHub Actions `windows-latest`) with a SQL Server service container |

---

## Security Limitations

### Plaintext Password Storage

| Property | Detail |
|---|---|
| **Status** | Passwords stored as plaintext strings in the `Users.Password` column |
| **Impact** | Any user with database read access can see all user passwords |
| **Workaround** | Restrict database access to the application's dedicated database user; do not share passwords across systems |
| **Future Action** | Implement PBKDF2 hashing (e.g., `Rfc2898DeriveBytes` with SHA-256) for password storage and verification. Requires a migration strategy for existing plaintext passwords (e.g., force password reset). |

### No Role-Based Access Control

| Property | Detail |
|---|---|
| **Status** | The only access control is the `IsActive` flag on user accounts |
| **Impact** | All active users have identical access to all application features |
| **Workaround** | Procedural/administrative controls — limit who has system user accounts |
| **Future Action** | Implement a roles or permissions system if different user roles (e.g., clerk, supervisor, admin) are needed |

### `data.txt` Username File

| Property | Detail |
|---|---|
| **Status** | Stored in the current working directory; no encryption |
| **Impact** | Anyone with file system access to the machine can read the stored username |
| **Workaround** | File system-level access controls on the machine; note that the password is NOT stored |
| **Future Action** | Store the remember-me username in a more secure location (e.g., Windows Credential Manager) |

---

## Architectural Limitations

### No Automated Error Logging

| Property | Detail |
|---|---|
| **Status** | Exception details are caught and suppressed; `catch` blocks contain commented-out `Console.WriteLine` calls |
| **Impact** | Silent failures — if a database operation fails, the UI receives `false` or `null` with no diagnostic information |
| **Workaround** | None — failures silently degrade |
| **Future Action** | Integrate a logging framework (e.g., `Microsoft.Extensions.Logging`, Serilog) and write exception details to a log file |

### No Database Connection Pooling Configuration

| Property | Detail |
|---|---|
| **Status** | Each method opens and closes its own `SqlConnection` |
| **Impact** | Relies on ADO.NET's default connection pooling; no explicit pool size tuning |
| **Workaround** | Default ADO.NET pooling is adequate for a single-user desktop application |
| **Future Action** | For multi-user deployments, consider connection pooling configuration |

### No Dependency Injection

| Property | Detail |
|---|---|
| **Status** | All dependencies are resolved via static method calls; no DI container |
| **Impact** | Difficult to unit test in isolation; tight coupling between layers |
| **Workaround** | Not applicable for current use |
| **Future Action** | Introduce interfaces for data access classes and use a DI container (e.g., `Microsoft.Extensions.DependencyInjection`) to enable mocking in tests |

### Vehicle License Services Not Implemented

| Property | Detail |
|---|---|
| **Status** | The main menu contains a "Vehicle Licenses Services" menu item that shows "Not Implemented Yet" |
| **Impact** | Vehicle licensing functionality is absent |
| **Workaround** | Not applicable |
| **Future Action** | Implement vehicle license management if required |

---

## Technical Debt

### Inconsistent Naming (`DVLD_Buisness`)

The business layer project is named `DVLD_Buisness` (misspelling of "Business"). This is carried through all project references, namespaces, and assembly names. Renaming would require updates across all three projects and any deployment scripts.

### Commented-Out Code

Multiple `catch` blocks contain `//Console.WriteLine("Error: " + ex.Message);`. These are development artifacts that should be replaced with proper logging.

### Missing SQL Schema

The DVLD database schema is not included in the repository. This is a significant gap for new developers and for automated environment setup.
