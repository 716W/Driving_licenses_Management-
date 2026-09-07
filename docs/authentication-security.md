# Authentication & Security

This document describes the authentication flow, Remember-Me behavior, password storage, and security characteristics of the DVLD application, as verified from the source code.

---

## Current Authentication Flow

```
frmLogin  (DVLD/Login/frmLogin.cs)
    │
    │  clsUser.FindByUsernameAndPassword(username, password)
    ↓
clsUser  (DVLD_Buisness/clsUser.cs)
    │
    │  clsUserData.GetUserInfoByUsernameAndPassword(username, password, ...)
    ↓
clsUserData  (DVLD_DataAccess/clsUserData.cs)
    │
    │  SELECT * FROM Users WHERE Username = @Username AND Password = @Password
    ↓
SQL Server  (Users table)
```

### Step-by-Step

1. User enters username and password in `frmLogin`
2. `btnLogin_Click` calls `clsUser.FindByUsernameAndPassword(txtUserName.Text.Trim(), txtPassword.Text.Trim())`
3. `clsUser.FindByUsernameAndPassword` delegates to `clsUserData.GetUserInfoByUsernameAndPassword`
4. The Data Access layer executes a parameterized SELECT against the `Users` table, matching both `UserName` and `Password`
5. If a row is returned, a `clsUser` object is constructed and returned; otherwise `null` is returned
6. If the user object is non-null but `IsActive == false`, a message box is shown and login is rejected
7. If the user is active, `clsGlobal.CurrentUser` is set to the returned `clsUser` object
8. `frmLogin` is hidden and `frmMain` is shown with a reference back to `frmLogin` (for sign-out)

### SQL Query Used for Authentication

```sql
SELECT * FROM Users WHERE Username = @Username AND Password = @Password;
```

Both parameters are bound via `command.Parameters.AddWithValue(...)` — the query is parameterized and not vulnerable to SQL injection.

---

## Session Management

- The logged-in user is stored in the static property `clsGlobal.CurrentUser` (type `clsUser`)
- This property is accessible throughout the application (UI project scope)
- On sign-out (`signOutToolStripMenuItem_Click` in `frmMain`), `clsGlobal.CurrentUser` is set to `null` and the login form is re-shown
- There is no token-based or server-side session — the application is a desktop process; session lifetime equals the process lifetime

---

## Remember Me

### Current Implementation (verified)

The Remember-Me feature in `frmLogin` (controlled by `chkRememberMe`) works as follows:

**When Remember-Me is checked at login:**
- `clsGlobal.RememberUsernameAndPassword(username, password)` is called
- The `Password` parameter is **intentionally ignored** inside `RememberUsernameAndPassword`
- Only the `username` string is written to `data.txt` in the current working directory

**When Remember-Me is unchecked at login:**
- `clsGlobal.RememberUsernameAndPassword("", "")` is called with empty strings
- `data.txt` is deleted if it exists

**On application startup (frmLogin_Load):**
- `clsGlobal.GetStoredCredential(ref UserName, ref Password)` is called
- `Password` is always returned as empty string — the stored file is never read for a password
- If a username is found in `data.txt`, it is pre-filled in `txtUserName`; `txtPassword` remains empty
- `chkRememberMe` is checked

### Security Characteristic

> The password is **never persisted to disk** in the current implementation.
> The user must always type their password on every login, even when Remember-Me is checked.

The code comment in `clsGlobal.cs` states:
```csharp
// NOTE: the Password parameter is intentionally ignored for security.
// Plaintext passwords must never be written to disk.
```

This was a security improvement made during the .NET 9 migration. In the original implementation, both username and password were stored in `data.txt`.

---

## Password Storage

### Current State

> **Passwords are stored as plaintext in the `Users` table.**

The Data Access layer stores and retrieves the password column directly as a string. There is no hashing, salting, or encryption applied.

The authentication query compares the plaintext password directly:
```sql
SELECT * FROM Users WHERE Username = @Username AND Password = @Password;
```

### Implication

Any user with direct database access can read all passwords. Password reuse across other systems is a risk if users share passwords.

### Pending Improvement

Password hashing (e.g., PBKDF2 with SHA-256) has **not yet been implemented** in the current codebase.

A future improvement would:
1. Hash new and changed passwords before storing them (e.g., using `Rfc2898DeriveBytes`)
2. During login, hash the entered password and compare the hash
3. Implement a migration strategy for existing plaintext passwords (e.g., require password reset on first login after the upgrade)

This remains an open security limitation. See [known-limitations.md](known-limitations.md).

---

## Connection String Security

The database connection string is never hardcoded in source code.

Resolution mechanism (see [configuration.md](configuration.md)):
1. `DVLD_CONNECTION_STRING` environment variable (preferred for production)
2. `dvld.env` file (git-ignored, for local development)
3. Fail-fast exception if neither is available

No credentials appear in any committed source file. `dvld.env.example` contains only placeholders.

---

## SQL Injection Protection

All SQL queries in `DVLD_DataAccess` use parameterized queries:
```csharp
command.Parameters.AddWithValue("@ParamName", userSuppliedValue);
```

No dynamic SQL string concatenation of user input was found in the codebase.

---

## Security Controls Summary

| Control | Status |
|---|---|
| Parameterized SQL (anti-injection) | ✅ Implemented |
| Connection string externalized (no hardcoded credentials) | ✅ Implemented |
| `dvld.env` excluded from source control | ✅ Implemented |
| Remember-Me does not persist passwords to disk | ✅ Implemented (migration improvement) |
| Password hashing at rest | ❌ Not implemented — plaintext storage |
| Role-based access control | ❌ Not implemented — `IsActive` flag only |
| Audit logging | ❌ Not implemented |
| HTTPS / transport encryption | N/A — desktop application; database transport security depends on SQL Server configuration |

---

## Security Limitations

See [known-limitations.md](known-limitations.md) for a full list. Key security limitations:

- **Plaintext passwords** — passwords are stored and compared as plaintext in the database
- **No RBAC** — all active users have equal access; there is no role or permission system beyond the `IsActive` flag
- **Local configuration model** — the application is a desktop process; security depends on the security of the Windows machine and the SQL Server instance it connects to
- **`data.txt` username file** — stored in the current working directory; while passwords are not stored, the username file could reveal valid usernames to anyone with local file access
