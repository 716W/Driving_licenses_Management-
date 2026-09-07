# Troubleshooting

This document covers common problems and their solutions.

---

## NETSDK1100 — Windows is required to build Windows desktop applications

### Symptom

```
error NETSDK1100: Windows is required to build Windows desktop applications.
```

### Cause

You are building the `DVLD` project (which targets `net9.0-windows`) on Linux or macOS without enabling Windows cross-targeting.

### Solution

Add `-p:EnableWindowsTargeting=true` to your build or publish command:

```bash
dotnet build DVLD/DVLD.sln -p:EnableWindowsTargeting=true

dotnet build DVLD/DVLD.sln -c Release -p:EnableWindowsTargeting=true

dotnet publish DVLD/DVLD.csproj -c Release -r win-x64 --self-contained \
  -p:EnableWindowsTargeting=true -o publish/win-x64-self-contained
```

### Why this property is needed

The DVLD UI project targets `net9.0-windows`. On Linux/macOS, the .NET SDK does not include the Windows Desktop targets by default. `EnableWindowsTargeting=true` tells MSBuild to import those targets even on non-Windows hosts, enabling cross-compilation of WinForms projects.

---

## Missing `Microsoft.WindowsDesktop.App` Runtime

### Symptom

When running the published application on a Windows machine:

```
The framework 'Microsoft.WindowsDesktop.App', version '9.0.x' was not found.
```

### Cause

The application was published as **framework-dependent** and the target Windows machine does not have the .NET 9 Windows Desktop Runtime installed.

### Solution

Choose one of:

1. **Install the runtime** on the target machine:
   - Download .NET 9 Windows Desktop Runtime from https://dotnet.microsoft.com/download/dotnet/9.0
   - Install it on the target Windows machine

2. **Use self-contained publish** so the runtime is bundled:
   ```bash
   dotnet publish DVLD/DVLD.csproj -c Release -r win-x64 --self-contained \
     -p:EnableWindowsTargeting=true \
     -o publish/win-x64-self-contained
   ```

---

## SQL Server Connection Failure

### Symptom

Application throws `InvalidOperationException` at startup, or a `SqlException` when the first database operation is attempted:

```
DVLD database connection string is not configured.
```

or:

```
A network-related or instance-specific error occurred while establishing a connection to SQL Server.
```

### Diagnosis Steps

1. **Verify configuration exists**

   Check that either `DVLD_CONNECTION_STRING` is set as an environment variable, or `dvld.env` exists next to the executable (or in the working directory). See [configuration.md](configuration.md).

2. **Check SQL Server is running**

   If using Docker:
   ```bash
   docker ps
   # Look for your SQL Server container — status should be "Up"
   ```

   If using local SQL Server on Windows:
   - Open Services (services.msc) and verify SQL Server is running

3. **Verify host and port**

   Default SQL Server port is 1433. Ensure your connection string uses the correct host and port:
   ```
   Server=localhost,1433;...
   ```

4. **Verify credentials**

   Ensure the username and password in the connection string are correct and the user has access to the `DVLD` database.

5. **Verify the database exists**

   Connect to SQL Server with a tool (e.g., SQL Server Management Studio or `sqlcmd`) and verify the `DVLD` database exists.

6. **Check firewall rules**

   If SQL Server is on a remote machine or in Docker, ensure port 1433 (or your configured port) is open between the application machine and the database machine.

7. **`TrustServerCertificate=True`**

   If you see SSL certificate errors, add `TrustServerCertificate=True;` to your connection string (as shown in `dvld.env.example`). Note: evaluate security implications for production environments.

---

## Missing dvld.env / DVLD_CONNECTION_STRING

### Symptom

```
DVLD database connection string is not configured.
Set the 'DVLD_CONNECTION_STRING' environment variable,
or create a 'dvld.env' file next to the executable with the line:
  DVLD_CONNECTION_STRING=Server=<host>;Database=DVLD;User Id=<user>;Password=<pass>;TrustServerCertificate=True;
See dvld.env.example for a template. Never commit real credentials.
```

### Solution

1. Copy `dvld.env.example` to `dvld.env` in the same directory as `DVLD.exe` (or in the project directory when developing)
2. Edit `dvld.env` and replace the placeholder values with real connection details:
   ```
   DVLD_CONNECTION_STRING=Server=localhost,1433;Database=DVLD;User Id=dvld_user;Password=YOUR_REAL_PASSWORD;TrustServerCertificate=True;
   ```

---

## Build Warnings

### CS0168 / CS0169 — Unused Variables or Fields

Some `catch` blocks capture exceptions into variables that are not subsequently used (commented-out `Console.WriteLine` calls). These generate CS0168 warnings. They are pre-existing in the original codebase and not migration-generated.

Example pattern throughout the data access layer:
```csharp
catch (Exception ex)
{
    // Console.WriteLine("Error: " + ex.Message);
    isFound = false;
}
```

These warnings are benign for current functionality but represent technical debt (see [known-limitations.md](known-limitations.md)).

### Windows-targeting warnings on Linux

When building on Linux with `-p:EnableWindowsTargeting=true`, you may see warnings about Windows-only APIs. These are expected and do not prevent compilation.

---

## Login Always Fails

### Symptom

Entering correct credentials results in "Invalid Username/Password" message.

### Possible Causes

1. **SQL Server unreachable** — the authentication query fails silently (exceptions are caught and `null` is returned). Check the connection string and SQL Server availability.
2. **Database not populated** — the `Users` table is empty. Verify you have the correct database with seed data.
3. **Incorrect credentials** — passwords are stored as plaintext in the current implementation. Verify the exact password as stored in the database.

---

## Application Window Does Not Appear (WinForms Not Supported)

### Symptom

Running the application produces an error about WinForms or Windows Desktop not being supported.

### Cause

You are attempting to run the WinForms application on Linux or macOS. This is not supported.

### Solution

The application must be run on **Windows 10 or Windows 11** with either:
- .NET 9 Windows Desktop Runtime (for FDD)
- No .NET installation required (for SCD)

See [build-and-publish.md](build-and-publish.md) and [known-limitations.md](known-limitations.md).
