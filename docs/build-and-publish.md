# Build and Publish

This document describes how to build and publish the DVLD application.

---

## Prerequisites

### .NET SDK

- **.NET 9 SDK** must be installed on the build machine
- Download: https://dotnet.microsoft.com/download/dotnet/9.0
- Verify: `dotnet --version` should output `9.x.x`

### Operating System for Building

The application targets `net9.0-windows` (WinForms). You can build from any OS:
- **Windows** — builds and runs natively
- **Linux / macOS** — can build but requires `-p:EnableWindowsTargeting=true`; the output cannot be executed on Linux/macOS (see [Windows Runtime Requirement](#windows-runtime-requirement))

---

## Restore

Restore NuGet packages before building:

```bash
dotnet restore DVLD/DVLD.sln
```

This downloads `Microsoft.Data.SqlClient` 6.0.3 (the only external dependency).

---

## Debug Build

### On Windows

```bash
dotnet build DVLD/DVLD.sln
```

### On Linux / macOS

```bash
dotnet build DVLD/DVLD.sln -p:EnableWindowsTargeting=true
```

### Why `-p:EnableWindowsTargeting=true` on Linux

The UI project (`DVLD`) targets `net9.0-windows`. On non-Windows systems, the .NET SDK does not include the `Microsoft.WindowsDesktop.App` framework by default. Without `EnableWindowsTargeting=true`, MSBuild raises:

```
error NETSDK1100: Windows is required to build Windows desktop applications.
```

Setting this property allows the SDK to resolve Windows-specific TFMs on Linux without having the Windows Desktop runtime installed. The resulting binaries still require Windows to execute.

---

## Release Build

```bash
# Windows
dotnet build DVLD/DVLD.sln -c Release

# Linux / macOS
dotnet build DVLD/DVLD.sln -c Release -p:EnableWindowsTargeting=true
```

---

## Publish

Publishing produces a deployable output directory. The solution file is not used for publish — target the DVLD project directly.

### Framework-Dependent Publish (FDD)

The output requires the **.NET 9 Windows Desktop Runtime** to be installed on the target machine. Smaller output size.

```bash
dotnet publish DVLD/DVLD.csproj \
  -c Release \
  -r win-x64 \
  --no-self-contained \
  -p:EnableWindowsTargeting=true \
  -o publish/win-x64-framework-dependent
```

**Target machine requirement:** Must have .NET 9 Windows Desktop Runtime installed.
**Output location:** `publish/win-x64-framework-dependent/`

> Note: The `publish/` directory is excluded from source control by `.gitignore`.

---

### Self-Contained Publish (SCD)

The output includes the .NET runtime. The target machine does not need .NET installed. Larger output size.

```bash
dotnet publish DVLD/DVLD.csproj \
  -c Release \
  -r win-x64 \
  --self-contained \
  -p:EnableWindowsTargeting=true \
  -o publish/win-x64-self-contained
```

**Target machine requirement:** Windows 10 or Windows 11 (x64). No .NET installation required.
**Output location:** `publish/win-x64-self-contained/`

---

## Windows Runtime Requirement

> Building and publishing a WinForms application from Linux is possible with `EnableWindowsTargeting=true`.
> However, **executing the application requires Windows**.

This is because:
- The project targets `net9.0-windows`, which relies on `Microsoft.WindowsDesktop.App`
- `Microsoft.WindowsDesktop.App` is only available on Windows
- WinForms itself relies on Win32 APIs that do not exist on Linux/macOS

For runtime execution you need:
- **Framework-dependent:** Windows 10/11 + .NET 9 Windows Desktop Runtime
- **Self-contained:** Windows 10/11 only (runtime is bundled)

---

## After Publishing

1. Copy the contents of the publish output directory to the target Windows machine
2. Place `dvld.env` next to the executable (or set the `DVLD_CONNECTION_STRING` environment variable on the target machine)
3. Ensure the target machine can reach the SQL Server (correct host, port, firewall rules)
4. Run `DVLD.exe`

---

## Build Verification Checklist

| Step | Command |
|---|---|
| Restore | `dotnet restore DVLD/DVLD.sln` |
| Debug build (Linux) | `dotnet build DVLD/DVLD.sln -p:EnableWindowsTargeting=true` |
| Release build (Linux) | `dotnet build DVLD/DVLD.sln -c Release -p:EnableWindowsTargeting=true` |
| FDD publish | `dotnet publish DVLD/DVLD.csproj -c Release -r win-x64 --no-self-contained -p:EnableWindowsTargeting=true` |
| SCD publish | `dotnet publish DVLD/DVLD.csproj -c Release -r win-x64 --self-contained -p:EnableWindowsTargeting=true` |
