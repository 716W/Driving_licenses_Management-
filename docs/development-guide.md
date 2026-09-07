# Development Guide

This document describes how to set up a development environment, work with the codebase, and make changes safely.

---

## Repository Setup

1. **Clone the repository**

   ```bash
   git clone <repository-url>
   cd Driving_licenses_Management-
   ```

2. **Install the .NET 9 SDK**

   Download from https://dotnet.microsoft.com/download/dotnet/9.0

   Verify:
   ```bash
   dotnet --version
   # Expected: 9.x.x
   ```

3. **Configure the database connection**

   ```bash
   cp dvld.env.example dvld.env
   # Edit dvld.env — replace placeholders with your actual SQL Server connection details
   ```

4. **Restore NuGet packages**

   ```bash
   dotnet restore DVLD/DVLD.sln
   ```

5. **Build**

   ```bash
   # Windows
   dotnet build DVLD/DVLD.sln

   # Linux / macOS
   dotnet build DVLD/DVLD.sln -p:EnableWindowsTargeting=true
   ```

---

## Branching

The repository history shows:
- `master` — mainline; the merged result of the .NET 9 migration
- `chore/net9-migration` — the migration branch (now merged via PR #1)
- `feature/net9-migration` — the initial project files commit

For new work, create branches from `master` following Conventional Commits prefixes:

| Prefix | Use for |
|---|---|
| `feature/` | New features |
| `fix/` | Bug fixes |
| `chore/` | Tooling, dependencies |
| `refactor/` | Code restructuring |
| `docs/` | Documentation |

---

## Database Development

### SQL Server Requirement

The application requires a running SQL Server instance. Options:

- **Local SQL Server** on Windows (Express, Developer, or full edition)
- **Docker** on any platform:
  ```bash
  docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=<YourPassword>" \
    -p 1433:1433 --name dvld-sql \
    -d mcr.microsoft.com/mssql/server:2022-latest
  ```
- **Remote SQL Server** — update `dvld.env` with the remote host/port

### Connection String

Edit `dvld.env`:
```
DVLD_CONNECTION_STRING=Server=localhost,1433;Database=DVLD;User Id=sa;Password=<YourPassword>;TrustServerCertificate=True;
```

The database name expected by the application is `DVLD` (referenced in error messages in `clsDataAccessSettings`).

---

## Opening in Visual Studio

1. Open `DVLD/DVLD.sln` in Visual Studio 2022 or later
2. The solution loads all three projects: `DVLD`, `DVLD_Buisness`, `DVLD_DataAccess`
3. Set `DVLD` as the startup project (it should be by default)
4. Press F5 to build and run (Windows only)

---

## Debugging

- Runtime debugging of WinForms requires **Windows**
- On Linux/macOS, you can build and inspect code but cannot execute the WinForms app
- Use `Console.WriteLine` or a debugger attached via Visual Studio on Windows for runtime debugging
- Exception messages from `clsDataAccessSettings` will appear at application startup if the connection string is missing

---

## Code Organization

### Where to Add UI Logic

- New forms go in the appropriate subdirectory under `DVLD/`
- Add menu item handlers in `frmMain.cs`
- For reusable display components, create a new `ctrl*.cs` user control

### Where to Add Business Logic

- Add new business entities in `DVLD_Buisness/` following the existing `cls*` naming convention
- Business classes should not import anything from `DVLD` (no UI dependency) and should not import `Microsoft.Data.SqlClient` directly
- Use the `Mode` (AddNew/Update) pattern for entity save operations

### Where to Add Data Access

- Add new data access classes in `DVLD_DataAccess/` following the `cls*Data` naming convention
- Always use parameterized `SqlCommand` — never concatenate user input into SQL strings
- Follow the open/try/catch/finally/close pattern used throughout the existing code

---

## Safe Change Guidelines

The following practices are observed throughout the codebase and should be followed for new changes:

### Layer Boundaries
- UI code (`DVLD/`) → calls Business layer only (`DVLD_Buisness/`)
- Business code (`DVLD_Buisness/`) → calls Data Access layer only (`DVLD_DataAccess/`)
- Data Access code (`DVLD_DataAccess/`) → SQL Server only
- Never call `clsUserData`, `clsPersonData`, etc. directly from a WinForms form

### SQL Safety
- Always use `command.Parameters.AddWithValue("@ParamName", value)` for any user-supplied input
- Never build SQL strings by concatenating user input: `"WHERE Name = '" + textBox.Text + "'"` is forbidden

### Secrets
- Never hardcode connection strings, passwords, or credentials in any source file
- Use `dvld.env` (local, gitignored) or the `DVLD_CONNECTION_STRING` environment variable

### Null Safety
- The database returns `DBNull.Value` for nullable columns — always check before casting (see `clsPersonData` for examples)
- Business layer `Find*` methods return `null` when a record is not found — check before using the returned object

### File Paths
- The current `data.txt` remember-me mechanism uses `Directory.GetCurrentDirectory()` — be aware this may differ between development (F5) and published deployments

---

## Adding a New Feature — Typical Steps

1. **Data Access** — add a new class or method in `DVLD_DataAccess/` that executes parameterized SQL
2. **Business Logic** — add a new business class or method in `DVLD_Buisness/` that calls the Data Access class
3. **UI** — add a new form or control in `DVLD/` that calls the Business class
4. **Main Menu** — add a menu item in `frmMain.Designer.cs` (via the Visual Studio Form Designer on Windows) and wire up the handler in `frmMain.cs`
5. **Test** — verify the build succeeds and manually test the feature on Windows with a running SQL Server
