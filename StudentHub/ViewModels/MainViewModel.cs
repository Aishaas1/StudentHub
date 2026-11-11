using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentHub.Models;
using StudentHub.Services;
namespace StudentHub.ViewModels;
public partial class MainViewModel : ObservableObject {
 readonly StudentHubService service;
 readonly DialogService dialogs;
 [ObservableProperty] int page;
 [ObservableProperty] string search="";
 [ObservableProperty] string status="جاهز";
 [ObservableProperty] Student? selectedStudent;
 [ObservableProperty] Subject? selectedSubject;
 [ObservableProperty] Attendance? selectedAttendance;
 [ObservableProperty] Grade? selectedGrade;
 [ObservableProperty] string studentAverage="اختر طالبًا لعرض متوسطه";
 public ObservableCollection<Student> Students { get; }=[];
 public ObservableCollection<Student> RecentStudents { get; }=[];
 public ObservableCollection<Subject> Subjects { get; }=[];
 public ObservableCollection<Attendance> Attendance { get; }=[];
 public ObservableCollection<Grade> Grades { get; }=[];
 [ObservableProperty] int studentCount;
 [ObservableProperty] int subjectCount;
 [ObservableProperty] int attendanceCount;
 [ObservableProperty] string attendanceDetail="";
 [ObservableProperty] string average="0";
 public MainViewModel(StudentHubService service,DialogService dialogs) { this.service=service; this.dialogs=dialogs; Refresh(); }
 partial void OnSearchChanged(string value) => Run(()=>Replace(Students,service.Students(value)));
 partial void OnSelectedGradeChanged(Grade? value) {
  if(value==null) { StudentAverage="اختر طالبًا لعرض متوسطه"; return; }
  StudentAverage=$"متوسط {value.Student.FullName}: {Grades.Where(g=>g.StudentId==value.StudentId).Average(g=>g.Score):0.##} / 100";
 }
 void Run(Action action) { try { action(); } catch(System.ComponentModel.DataAnnotations.ValidationException e) { dialogs.Error(e.Message); } catch(Exception e) { System.Diagnostics.Trace.TraceError(e.ToString()); dialogs.Error("تعذر إتمام العملية. تحقق من صلاحية الملف وعدم استخدامه بواسطة برنامج آخر ثم أعد المحاولة."); } }
 static void Replace<T>(ObservableCollection<T> target,IEnumerable<T> values) { target.Clear(); foreach(var v in values) target.Add(v); }
 [RelayCommand] void Navigate(string value) => Page=int.Parse(value);
 [RelayCommand] void Refresh() => Run(()=> {
  var all=service.Students(); var grades=service.Grades(); var attendance=service.Attendance();
  Replace(Students,service.Students(Search)); Replace(RecentStudents,all.Take(5)); Replace(Subjects,service.Subjects()); Replace(Grades,grades); Replace(Attendance,attendance);
  StudentCount=all.Count; SubjectCount=Subjects.Count; AttendanceCount=attendance.Count;
  AttendanceDetail=$"{attendance.Count(a=>a.IsPresent)} حاضر · {attendance.Count(a=>!a.IsPresent)} غائب";
  Average=grades.Count==0 ? "—" : grades.Average(g=>g.Score).ToString("0.##");
  SelectedStudent=null; SelectedSubject=null; SelectedAttendance=null; SelectedGrade=null;
 });
 // Edit detached copies so Cancel never changes visible or persisted data.
 [RelayCommand] void AddStudent() => Edit(new Student(),"إضافة طالب");
 [RelayCommand] void EditStudent() { if(SelectedStudent is not {} s) { dialogs.Error("اختر طالبًا أولًا."); return; } Edit(new Student { Id=s.Id,StudentNumber=s.StudentNumber,FullName=s.FullName,Department=s.Department,AcademicLevel=s.AcademicLevel,Email=s.Email,PhoneNumber=s.PhoneNumber,CreatedAt=s.CreatedAt },"تعديل الطالب"); }
 [RelayCommand] void AddSubject() => Edit(new Subject(),"إضافة مادة");
 [RelayCommand] void EditSubject() { if(SelectedSubject is not {} s) { dialogs.Error("اختر مادة أولًا."); return; } Edit(new Subject { Id=s.Id,Code=s.Code,Name=s.Name,CreditHours=s.CreditHours },"تعديل المادة"); }
 [RelayCommand] void AddAttendance() => Edit(new Attendance(),"تسجيل الحضور");
 [RelayCommand] void EditAttendance() { if(SelectedAttendance is not {} a) { dialogs.Error("اختر سجلًا أولًا."); return; } Edit(new Attendance { Id=a.Id,StudentId=a.StudentId,SubjectId=a.SubjectId,Date=a.Date,IsPresent=a.IsPresent },"تعديل الحضور"); }
 [RelayCommand] void AddGrade() => Edit(new Grade(),"إضافة درجة");
 [RelayCommand] void EditGrade() { if(SelectedGrade is not {} g) { dialogs.Error("اختر درجة أولًا."); return; } Edit(new Grade { Id=g.Id,StudentId=g.StudentId,SubjectId=g.SubjectId,Score=g.Score },"تعديل الدرجة"); }
 void Edit(object entity,string title) { if(dialogs.Edit(entity,title)) { Refresh(); Status="تم حفظ البيانات بنجاح"; } }
 [RelayCommand] void DeleteStudent() => Delete<Student>(SelectedStudent?.Id,true);
 [RelayCommand] void DeleteSubject() => Delete<Subject>(SelectedSubject?.Id,true);
 [RelayCommand] void DeleteAttendance() => Delete<Attendance>(SelectedAttendance?.Id,false);
 [RelayCommand] void DeleteGrade() => Delete<Grade>(SelectedGrade?.Id,false);
 void Delete<T>(int? id,bool related) where T:class { if(id==null) { dialogs.Error("اختر صفًا أولًا."); return; } if(!dialogs.Confirm(related ? "حذف السجل وجميع سجلات الحضور والدرجات المرتبطة به؟" : "هل تريد حذف السجل المحدد؟")) return; Run(()=>{service.Delete<T>(id.Value); Refresh(); Status="تم الحذف بنجاح";}); }
 [RelayCommand] void Export() => Run(()=> { var path=dialogs.ExportPath(); if(path==null) return; File.WriteAllText(path,StudentHubService.Csv(service.Grades()),new UTF8Encoding(true)); Status="تم تصدير النتائج بنجاح"; });
}
