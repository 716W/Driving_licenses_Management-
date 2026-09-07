# DVLD — Driving & Vehicle License Department Management System

A desktop management system for the Driving & Vehicle License Department (DVLD), built with **Windows Forms** on **.NET 9**.
The application manages the full lifecycle of driving license operations: people, drivers, license applications, tests, issued licenses, detentions, international licenses, and system users.

---

## Table of Contents

- [Project Overview](#project-overview)
- [Key Features](#key-features)
- [Architecture](#architecture)
- [Technology Stack](#technology-stack)
- [Requirements](#requirements)
- [Quick Start](#quick-start)
- [Build](#build)
- [Publish](#publish)
- [Documentation Index](#documentation-index)

---

## Project Overview

| Property | Value |
|---|---|
| **Application Name** | DVLD – Driving & Vehicle License Department |
| **Application Type** | Windows Desktop (WinForms) |
| **Primary Language** | C# |
| **Target Framework** | .NET 9 (`net9.0-windows`) |
| **Database** | Microsoft SQL Server |
| **SQL Client Package** | `Microsoft.Data.SqlClient` 6.0.3 |
| **Solution File** | `DVLD/DVLD.sln` |

The system was originally developed targeting **.NET Framework** and was subsequently migrated to **.NET 9** SDK-style projects.

---

## Key Features

The following features are verified from the source code:

- **User management** — add, update, delete system users; change passwords; activate/deactivate accounts
- **Person management** — full CRUD for individuals (name, national ID, date of birth, gender, address, phone, email, nationality, photo)
- **Driver management** — link persons to driver records; view all drivers and their licenses
- **Local driving license applications** — create new applications per license class; manage status (New / Cancelled / Completed)
- **Three-stage driving tests** — Vision Test → Written Test → Street Test; scheduling and result recording; prerequisite enforcement
- **License issuance** — first-time issuance after passing all three tests; license class and validity period management
- **License renewal** — renew an existing local driving license with a new expiration date
- **License replacement** — replace a lost or damaged driving license
- **Detained licenses** — detain a license (with fine fees); release a detained license via a release application
- **International driving licenses** — issue an international license based on an existing local license
- **Application types management** — manage application type definitions and fees
- **Test types management** — manage Vision, Written, and Street test type definitions and fees
- **License classes management** — manage license class definitions (minimum age, validity length, fees)
- **Person license history** — view the complete license history for a person
- **Signed-in user session** — display the logged-in user; sign-out; view/edit current user profile

> **Note:** Vehicle license services are not implemented. The main form shows a "Not Implemented Yet" message for that menu item.

---

## Architecture

The solution contains **three projects** in a strict layered arrangement:

```
DVLD  (WinForms UI)
  │  references
  ↓
DVLD_Buisness  (Business Logic)
  │  references
  ↓
DVLD_DataAccess  (Data Access / SQL)
  │  connects
  ↓
SQL Server (external database)
```

| Project | Role |
|---|---|
| `DVLD` | Windows Forms UI — forms, user controls, program entry point |
| `DVLD_Buisness` | Business entities, business rules, and orchestration |
| `DVLD_DataAccess` | SQL queries, `SqlConnection`, `SqlCommand`, result mapping |

See [docs/architecture.md](docs/architecture.md) for a full description.

---

## Technology Stack

| Component | Details |
|---|---|
| Runtime | .NET 9 (`net9.0-windows`) |
| UI Framework | Windows Forms (WinForms) |
| Language | C# |
| Database | Microsoft SQL Server |
| SQL Client | `Microsoft.Data.SqlClient` 6.0.3 |
| Build System | MSBuild / `dotnet` CLI |
| IDE | Visual Studio 2022+ or compatible |

---

## Requirements

### Development Environment

- .NET 9 SDK (any OS — Linux/macOS require `-p:EnableWindowsTargeting=true` to build)
- SQL Server (local, remote, or Docker) accessible from the development machine
- Visual Studio 2022 (Windows) **or** any editor + `dotnet` CLI

### Runtime Environment

> **Windows is required to run the application.**
> WinForms targets `net9.0-windows` and requires the Windows Desktop runtime or a self-contained publish.
> The application cannot run natively on Linux or macOS.

- Windows 10 or Windows 11
- .NET 9 Windows Desktop Runtime (for framework-dependent deployments)
- SQL Server accessible from the Windows machine

---

## Quick Start

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Driving_licenses_Management-
   ```

2. **Configure the database connection**
   ```bash
   cp dvld.env.example dvld.env
   # Edit dvld.env and set your real connection string
   ```

3. **Restore NuGet packages**
   ```bash
   dotnet restore DVLD/DVLD.sln
   ```

4. **Build** (on Windows):
   ```bash
   dotnet build DVLD/DVLD.sln
   ```
   On Linux/macOS:
   ```bash
   dotnet build DVLD/DVLD.sln -p:EnableWindowsTargeting=true
   ```

5. **Run** — on Windows, open `DVLD/DVLD.sln` in Visual Studio and press F5, or run the published executable.

---

## Build

### Debug Build

```bash
dotnet build DVLD/DVLD.sln
```

On Linux (cross-targeting required):
```bash
dotnet build DVLD/DVLD.sln -p:EnableWindowsTargeting=true
```

### Release Build

```bash
dotnet build DVLD/DVLD.sln -c Release -p:EnableWindowsTargeting=true
```

See [docs/build-and-publish.md](docs/build-and-publish.md) for full details.

---

## Publish

### Framework-Dependent (requires .NET 9 Windows Desktop Runtime on target machine)

```bash
dotnet publish DVLD/DVLD.csproj -c Release -r win-x64 --no-self-contained \
  -p:EnableWindowsTargeting=true \
  -o publish/win-x64-framework-dependent
```

### Self-Contained (bundles the runtime; no .NET installation needed on target)

```bash
dotnet publish DVLD/DVLD.csproj -c Release -r win-x64 --self-contained \
  -p:EnableWindowsTargeting=true \
  -o publish/win-x64-self-contained
```

See [docs/build-and-publish.md](docs/build-and-publish.md) for full details.

---

## Documentation Index

| Document | Description |
|---|---|
| [docs/architecture.md](docs/architecture.md) | Layered architecture, project responsibilities, dependency flow |
| [docs/project-structure.md](docs/project-structure.md) | Repository tree, folder layout, key classes |
| [docs/database.md](docs/database.md) | Database technology, connection configuration, entities, operations, SQL bug history |
| [docs/configuration.md](docs/configuration.md) | Environment variables, dvld.env, configuration precedence, security rules |
| [docs/authentication-security.md](docs/authentication-security.md) | Authentication flow, Remember Me, password storage, security limitations |
| [docs/build-and-publish.md](docs/build-and-publish.md) | Build prerequisites, debug/release builds, FDD and SCD publish |
| [docs/development-guide.md](docs/development-guide.md) | Repository setup, development workflow, code organization guidelines |
| [docs/migration-to-dotnet-9.md](docs/migration-to-dotnet-9.md) | Complete .NET 9 migration history, phases, changes, and status |
| [docs/testing.md](docs/testing.md) | Static validation, runtime testing requirements, current test coverage |
| [docs/troubleshooting.md](docs/troubleshooting.md) | Common errors and resolutions |
| [docs/known-limitations.md](docs/known-limitations.md) | Platform, database, security, and architectural limitations |
| [docs/changelog.md](docs/changelog.md) | Chronological record of major changes |
