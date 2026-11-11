# StudentHub

[الشرح بالعربية](README.md)
Arabic RTL WPF desktop application for students, subjects, attendance and grades. A small university project with a modern navy/white interface, local SQLite storage and a simple MVVM design.

## Run on Windows
1. Install **Visual Studio 2026** with the **.NET desktop development** workload and **.NET 10 LTS SDK**. Visual Studio 2022 does not support targeting .NET 10.
2. Open `StudentHub.sln`, allow NuGet restore, set `StudentHub` as startup project and press F5.
3. The database and sample records are created automatically on first launch in `%LocalAppData%\StudentHub\studenthub.db`. No database server or runtime internet connection is required.

## Features
- Dashboard totals, present/absent counts, mean of all recorded grades, latest five students.
- Student CRUD with name/student-number search and validation. Email and phone are optional.
- Subject CRUD with code/name and 1–6 credit hours.
- Attendance CRUD, date-only normalization, unique student/subject/day.
- Grade CRUD, 0–100 validation, one result per student/subject. Select a grade row to see that student's arithmetic mean (unweighted).
- CSV export of all results with UTF-8 BOM for Arabic Excel, quoted cells and formula protection.
- Confirmation before deletion. Deleting a student or subject deletes its linked attendance and grades; the dialog explicitly warns about this.

## Design and explanation
`Models` defines four entities. `Data` configures EF Core relationships, unique indexes, SQLite checks and initial seed. `Services/StudentHubService.cs` contains validation and database operations, using a short-lived context per operation. `ViewModels/MainViewModel.cs` exposes observable state and Toolkit commands; views bind to it. `EditorWindow` builds a small form for each entity and delegates persistence to the service; its code-behind handles only form input/dialog behavior. Detached copies make Cancel safe. A second lightweight console project links the actual model/data/service source so integration checks can run without WPF on Linux.

Internal integer keys are separate from the visible university student number. Student numbers and subject codes are unique case-insensitively. Academic level is 1–8. Attendance allows historical or future dates. Blank grids represent empty results. All sample data is fictional.

## Build and verification
```powershell
dotnet restore StudentHub.sln
dotnet build StudentHub.sln -c Release
dotnet run --project StudentHub.Tests -c Release
```
The checks use a temporary real SQLite file and cover CRUD, validation, duplicates, relationships, persistence, averages and CSV. This is an executable test harness: use `dotnet run`, not `dotnet test`.

See `VALIDATION.md` for actual results and the remaining Windows UI checklist. Cross-compiling WPF does not demonstrate successful GUI execution. No UI screenshots are supplied unless captured from the running Windows application.

## Data lifecycle
`EnsureCreated` is deliberately used for this fixed academic schema; there are no migrations. Seed data is inserted only when the database is first created, so deleting demo records does not recreate them on restart. For future schema changes, add a planned migration strategy. To reset the demo, close the application, back up the database, then remove its database file and reopen. For backup, close all StudentHub instances and copy the entire data directory. Do not commit personal student data or database files.

## Publish (optional, Windows)
```powershell
dotnet publish StudentHub/StudentHub.csproj -c Release -r win-x64 --self-contained true -o artifacts/StudentHub
```
Distribute the entire output folder; keep the writable database in LocalAppData. Arabic interface text is intentional; the documentation is bilingual.
