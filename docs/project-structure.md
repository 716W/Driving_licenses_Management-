# Project Structure

This document describes the repository layout and the role of every significant directory and class, as verified from the source code.

---

## Repository Root

```
Driving_licenses_Management-/
├── .git/                        # Git repository metadata
├── .gitattributes               # Line ending and diff settings
├── .gitignore                   # Excludes build artifacts, dvld.env, data.txt, etc.
├── README.md                    # Project entry point
├── dvld.env.example             # Template for local database configuration
├── publish/                     # Output directory for published builds (gitignored)
│   ├── win-x64-framework-dependent/
│   └── win-x64-self-contained/
├── docs/                        # Documentation (this directory)
├── DVLD/                        # WinForms UI project
├── DVLD_Buisness/               # Business logic project
└── DVLD_DataAccess/             # Data access project
```

> **Note:** `publish/` is excluded from source control by `.gitignore`.

---

## DVLD — WinForms UI Project

**Solution file:** `DVLD/DVLD.sln`
**Project file:** `DVLD/DVLD.csproj`
**Framework:** `net9.0-windows`
**Output type:** `WinExe`

```
DVLD/
├── DVLD.sln                     # Visual Studio solution (references all 3 projects)
├── DVLD.csproj                  # SDK-style project file; references DVLD_Buisness
├── Program.cs                   # Application entry point
├── frmMain.cs / .Designer.cs    # Main application window with menu navigation
├── frmMain.resx
│
├── Login/
│   └── frmLogin.cs              # Login form — authenticates via clsUser.FindByUsernameAndPassword
│
├── Global Classes/
│   ├── clsGlobal.cs             # Session state (CurrentUser), Remember-Me file handling
│   └── util.cs                  # Utility helpers
│
├── People/
│   ├── frmListPeople.cs         # Person list/search with DataGridView
│   ├── frmAddUpdatePerson.cs    # Add/edit person details
│   ├── frmFindPerson.cs         # Person search dialog
│   ├── frmShowPersonInfo.cs     # Read-only person info display
│   └── Controls/
│       ├── ctrlPersonCard.cs          # Reusable person info display control
│       └── ctrlPersonCardWithFilter.cs # Person card with search filter
│
├── Drivers/
│   └── frmListDrivers.cs        # Driver list with DataGridView
│
├── User/
│   ├── frmListUsers.cs          # User list with DataGridView
│   ├── frmAddUpdateUser.cs      # Add/edit system users
│   ├── frmChangePassword.cs     # Change password form
│   ├── frmUserInfo.cs           # Read-only user info form (wrapper)
│   └── ctrlUserCard.cs          # Reusable user info display control
│
├── Applications/
│   ├── Application Types/
│   │   └── frmManageApplicationTypes.cs  # Manage application type fees
│   ├── Controls/
│   │   └── ctrlApplicationBasicInfo.cs   # Reusable application info control
│   ├── Local Driving License/
│   │   ├── frmAddUpdateLocalDrivingLicesnseApplication.cs  # New local DL application
│   │   ├── frmListLocalDrivingLicesnseApplications.cs      # List/manage local DL applications
│   │   └── ctrlDrivingLicenseApplicationInfo.cs             # Application info control
│   ├── Renew Local License/
│   │   └── frmRenewLocalDrivingLicenseApplication.cs
│   ├── ReplaceLostOrDamagedLicense/
│   │   └── frmReplaceLostOrDamagedLicenseApplication.cs
│   ├── International License/
│   │   ├── frmNewInternationalLicenseApplication.cs
│   │   └── frmListInternationalLicesnseApplications.cs
│   ├── Detain License/
│   │   └── frmDetainLicenseApplication.cs
│   └── Rlease Detained License/
│       └── frmReleaseDetainedLicenseApplication.cs
│
├── Licenses/
│   ├── Controls/
│   │   └── ctrlDriverLicenses.cs          # Reusable driver licenses list control
│   ├── Local Licenses/
│   │   ├── Controls/
│   │   │   ├── ctrlDriverLicenseInfo.cs          # Local license details control
│   │   │   └── ctrlDriverLicenseInfoWithFilter.cs # License info with search filter
│   ├── International Licenses/
│   │   ├── frmListInternationalLicesnseApplications.cs (also under Applications)
│   │   └── Controls/
│   │       └── ctrlDriverInternationalLicenseInfo.cs
│   ├── Detain License/ (forms for detaining)
│   └── frmShowPersonLicenseHistory.cs     # Full license history for a person
│
├── Tests/
│   ├── frmListTestAppointments.cs   # List of test appointments
│   ├── frmScheduleTest.cs           # Schedule a test appointment
│   ├── frmTakeTest.cs               # Record test result (pass/fail)
│   ├── Test Types/
│   │   └── frmListTestTypes.cs      # Manage test types and fees
│   └── Controls/
│       ├── crlScheduleTest.cs       # Schedule test control
│       └── ctrlSecheduledTest.cs    # Scheduled test display control
│
├── Properties/                  # Application properties
└── Resources/                   # Embedded image/icon resources
```

### Entry Point

`Program.cs` configures the application and launches `frmLogin`:

```csharp
Application.SetHighDpiMode(HighDpiMode.SystemAware);
Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(false);
Application.Run(new frmLogin());
```

### Session State

`clsGlobal` (in `Global Classes/`) holds the static `CurrentUser` property of type `clsUser`, available application-wide after login.

---

## DVLD_Buisness — Business Logic Project

**Project file:** `DVLD_Buisness/DVLD_Buisness.csproj`
**Framework:** `net9.0`
**References:** `DVLD_DataAccess`

```
DVLD_Buisness/
├── DVLD_Buisness.csproj
├── clsApplication.cs               # Base application entity + application types enum
├── clsApplicationType.cs           # Application type lookup entity
├── clsCountry.cs                   # Country lookup entity
├── clsDetainedLicense.cs           # Detained license entity
├── clsDriver.cs                    # Driver entity (linked to clsPerson)
├── clsInternationalLicense.cs      # International license (extends clsApplication)
├── clsLicense.cs                   # Local driving license entity
├── clsLicenseClass.cs              # License class definition entity
├── clsLocalDrivingLicenseApplication.cs  # Local DL application (extends clsApplication)
├── clsPerson.cs                    # Person entity
├── clsTest.cs                      # Test result entity
├── clsTestAppointment.cs           # Test appointment entity
├── clsTestType.cs                  # Test type entity (Vision/Written/Street)
└── clsUser.cs                      # System user entity
```

### Entity Relationships (from code)

```
clsPerson
  └── has one clsUser (optional)
  └── has one clsDriver (optional, created on first license issuance)
      └── has many clsLicense (local driving licenses)
          └── may have one clsDetainedLicense (active detention)
      └── has many clsInternationalLicense
clsApplication (base)
  ├── clsLocalDrivingLicenseApplication (sub-application, one per license class application)
  │     └── has many clsTestAppointment
  │         └── each appointment has one clsTest result
  └── clsInternationalLicense (sub-application)
clsLicenseClass — defines valid license categories (referenced by clsLicense)
clsTestType    — VisionTest(1), WrittenTest(2), StreetTest(3)
clsApplicationType — NewDrivingLicense(1), RenewDrivingLicense(2), ReplaceLost(3),
                     ReplaceDamaged(4), ReleaseDetained(5), NewInternational(6), RetakeTest(7)
```

---

## DVLD_DataAccess — Data Access Project

**Project file:** `DVLD_DataAccess/DVLD_DataAccess.csproj`
**Framework:** `net9.0`
**Package:** `Microsoft.Data.SqlClient` 6.0.3

```
DVLD_DataAccess/
├── DVLD_DataAccess.csproj
├── clsDataAccessSettings.cs         # Connection string resolver
├── clsUserData.cs                   # CRUD for Users table
├── clsPersonData.cs                 # CRUD for People table
├── clsDriver.cs                     # CRUD for Drivers table
├── clsApplication.cs                # CRUD for Applications table
├── clsLocalDrivingLicenseApplicationData.cs  # CRUD for LocalDrivingLicenseApplications
├── clsLicense.cs                    # CRUD for Licenses table
├── clsInternationalLicense.cs       # CRUD for InternationalLicenses table
├── clsDetainedLicense.cs            # CRUD for DetainedLicenses table + view
├── clsTestType.cs                   # CRUD for TestTypes table
├── clsTest.cs                       # CRUD for Tests table
├── clsTestAppointment.cs            # CRUD for TestAppointments table
├── LicenseClass.cs                  # CRUD for LicenseClasses table (class: clsLicenseClassData)
├── ApplicationType.cs               # CRUD for ApplicationTypes table (class: clsApplicationTypeData)
└── clsCountryData.cs                # Read-only for Countries table
```

### Connection String Resolution

`clsDataAccessSettings` resolves the connection string in this order:
1. Environment variable `DVLD_CONNECTION_STRING`
2. `dvld.env` file next to the executable, then in the current working directory
3. Throws `InvalidOperationException` if neither source is available

See [configuration.md](configuration.md) for usage instructions.
