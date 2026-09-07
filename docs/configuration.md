# Configuration

This document describes all configuration mechanisms in the DVLD application, as verified from the source code.

---

## Overview

The application's only runtime configuration requirement is the **SQL Server connection string**. There are no other configuration files, no `appsettings.json`, and no app.config that the application reads at runtime.

---

## Environment Variables

### `DVLD_CONNECTION_STRING`

| Property | Value |
|---|---|
| **Name** | `DVLD_CONNECTION_STRING` |
| **Type** | String |
| **Required** | Yes — unless `dvld.env` is present |
| **Where used** | `DVLD_DataAccess/clsDataAccessSettings.cs` |

This is the primary and highest-priority source for the connection string.

**How to set it (Windows, current session):**
```cmd
set DVLD_CONNECTION_STRING=Server=localhost,1433;Database=DVLD;User Id=dvld_user;Password=YOUR_PASSWORD;TrustServerCertificate=True;
```

**How to set it (Windows, permanent, via System Properties):**
1. Open **System Properties → Advanced → Environment Variables**
2. Add a new System (or User) variable `DVLD_CONNECTION_STRING` with the connection string value

**How to set it (PowerShell, current session):**
```powershell
$env:DVLD_CONNECTION_STRING = "Server=localhost,1433;Database=DVLD;User Id=dvld_user;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
```

---

## dvld.env

### Purpose

`dvld.env` is a plain-text key-value file for local development configuration. It is the fallback when the environment variable is not set.

### Expected Location

The application looks for `dvld.env` in two locations (in this order):
1. The application's base directory (same folder as the `.exe`)
2. The current working directory (useful when launching via `dotnet run` or `F5` in Visual Studio)

### File Format

```
# Comments begin with #
# Blank lines are ignored
DVLD_CONNECTION_STRING=Server=<host>,<port>;Database=DVLD;User Id=<user>;Password=<password>;TrustServerCertificate=True;
```

### Git Exclusion

`dvld.env` is excluded from source control via `.gitignore`:
```
# DVLD security — never commit real credentials or remembered usernames
dvld.env
data.txt
```

**Never commit this file.** It contains your real database password.

---

## dvld.env.example

`dvld.env.example` is a template file that is committed to source control. It contains placeholder values only — no real credentials.

**Developer workflow:**
1. Copy `dvld.env.example` to `dvld.env`
2. Edit `dvld.env` and replace the placeholders with your real connection details
3. Never commit `dvld.env`

Contents of `dvld.env.example`:
```
# DVLD Database Configuration Template
# ---
# Copy this file to dvld.env and fill in your real connection details.
# dvld.env is excluded from source control (.gitignore). Never commit dvld.env.
#
# Format: DVLD_CONNECTION_STRING=<full connection string>
#
# Example (SQL Server with SQL auth):
#   DVLD_CONNECTION_STRING=Server=localhost,1433;Database=DVLD;User Id=dvld_user;Password=YOUR_PASSWORD_HERE;TrustServerCertificate=True;
#
# Example (Windows auth / Integrated Security):
#   DVLD_CONNECTION_STRING=Server=.;Database=DVLD;Integrated Security=True;TrustServerCertificate=True;

DVLD_CONNECTION_STRING=Server=<host>,<port>;Database=DVLD;User Id=<user>;Password=<password>;TrustServerCertificate=True;
```

---

## Configuration Precedence

```
1. DVLD_CONNECTION_STRING  (environment variable)    ← checked first
        ↓ (if absent or empty)
2. dvld.env  (file in exe directory or CWD)          ← checked second
        ↓ (if absent or key not found)
3. InvalidOperationException thrown at startup        ← fail-fast
```

The first non-empty value found is used. The connection string is cached after the first resolution (lazy singleton pattern in `clsDataAccessSettings`).

---

## Remember-Me File (data.txt)

The application uses a secondary configuration artifact: `data.txt` in the current working directory.

### Purpose

Stores the **username only** for the Remember-Me feature on the login form.

### Security Behavior (as implemented)

- **Only the username** is written to `data.txt` — the password is **never** stored on disk
- When Remember-Me is unchecked at login, `data.txt` is deleted
- On application load, if `data.txt` exists, the username is pre-filled in the login form; the password field remains empty and the user must type it

### Git Exclusion

`data.txt` is excluded from source control:
```
dvld.env
data.txt
```

---

## Security Rules

1. **Never commit real credentials.** The connection string contains a database password. Use environment variables or `dvld.env` — both are git-ignored.
2. **Never put production passwords in source code.** `clsDataAccessSettings.cs` intentionally contains no hardcoded credentials.
3. **Never publish `dvld.env`.** When publishing the application for deployment, copy `dvld.env.example` to the target machine and fill in the real credentials there, or set the environment variable on the target system.
4. **Use environment-specific configuration.** Development, staging, and production environments should each have their own connection string pointing to the appropriate SQL Server instance.
5. **`TrustServerCertificate=True`** — this option is shown in the example template for convenience in development environments. Evaluate whether this is appropriate for your production SQL Server setup.
