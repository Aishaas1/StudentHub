# Verification status

## Executed in the supplied Linux environment
- SDK: .NET 10.0.404.
- Release build of the complete solution, including WPF XAML compilation: **passed, zero warnings and zero errors**.
- Executable integration harness using actual SQLite files and production source: **29 checks passed**.
- Checks cover demo seeding, CRUD for all four entities, required fields, levels, email, unique student/subject identifiers, date normalization, duplicate attendance, duplicate grades, invalid and nonfinite scores, foreign keys, student averages, persistence through fresh contexts, CSV quoting/formula protection, database checks, cascading deletion and absence of reseeding after deleting demo data.
- Updated EF Core and the native SQLite bundle after the initial dependency audit warning; final restore/build reports no warnings.

## Windows automated verification
The repository includes `.github/workflows/windows-build.yml`, which builds the solution and runs the same database checks on `windows-latest`. The actual run status is available in the repository Actions tab. A successful build does not verify rendered WPF controls.

## Required Windows interactive acceptance
The supplied execution environment cannot run WPF. **The GUI has not been launched or visually verified here.** No running-application screenshots have been captured. Complete these checks before final university submission:

- [ ] Open StudentHub.sln in Visual Studio 2026 with .NET desktop workload; select StudentHub and press F5.
- [ ] Verify all six sidebar destinations and correct Arabic/RTL rendering.
- [ ] Resize the window from its minimum size to maximized; check table scrolling, headers, dialogs and text clipping at Windows display scales 100% and 150%.
- [ ] Add a student and subject; edit both; cancel an edit and verify unchanged values.
- [ ] Verify required-field, invalid-number, email and duplicate-number messages in the editor.
- [ ] Record present and absent attendance; try a duplicate on the same date, then edit and delete a record.
- [ ] Add grades 0 and 100; reject -1 and 101; modify a grade and verify the selected student's mean.
- [ ] Export to CSV and inspect Arabic names, quotes and decimal values in Excel or a text editor; cancel the Save dialog.
- [ ] Close and reopen the whole application; verify all saved changes persist.
- [ ] Confirm dashboard totals and latest students after additions/deletions.
- [ ] Cancel deletion, then confirm deletion; verify the warning and linked attendance/grade removal.
- [ ] Capture actual screenshots of each page for presentation after completing the checks.

# حالة التحقق
نجح البناء على Linux دون تحذيرات أو أخطاء ونجحت 29 حالة تحقق فعلية من قاعدة البيانات. لم تُشغّل واجهة WPF في هذه البيئة، ولذلك تبقى قائمة فحص Windows أعلاه شرطًا قبل إعلان اكتمال الاختبار المرئي أو جاهزية التسليم النهائية.
