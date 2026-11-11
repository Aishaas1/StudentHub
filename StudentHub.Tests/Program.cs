using StudentHub.Models;
using StudentHub.Services;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
var folder=Path.Combine(Path.GetTempPath(),"StudentHubTests-"+Guid.NewGuid()); Directory.CreateDirectory(folder);
var path=Path.Combine(folder,"test.db"); int passed=0;
void Check(bool value,string name) { if(!value) throw new Exception(name); Console.WriteLine("PASS: "+name); passed++; }
void Reject(Action action,string name) { try { action(); } catch(ValidationException) { Check(true,name); return; } throw new Exception("Expected validation: "+name); }
try {
 var s=new StudentHubService(path); s.Initialize(); Check(s.Students().Count==3 && s.Subjects().Count==3,"Demo data");
 s.Initialize(); Check(s.Students().Count==3,"No duplicate seeding");
 var student=new Student { StudentNumber="TEST001",FullName="اختبار طالب",Department="الحاسب",Email="test@example.com" }; s.Save(student);
 var subject=new Subject { Code="TEST",Name="اختبار" }; s.Save(subject);
 Check(s.Students("TEST001").Single().FullName==student.FullName,"Create and search");
 student.FullName="اسم معدل"; s.Save(student); Check(s.Students("اسم معدل").Count==1,"Update student");
 subject.Name="مادة معدلة"; s.Save(subject); Check(s.Subjects().Single(x=>x.Id==subject.Id).Name==subject.Name,"Update subject");
 Reject(()=>s.Save(new Student { StudentNumber="test001",FullName="Test",Department="CS" }),"Unique student number case insensitive");
 Reject(()=>s.Save(new Student()),"Required fields");
 Reject(()=>s.Save(new Student { StudentNumber="X",FullName="X",Department="X",AcademicLevel=9 }),"Level bounds");
 Reject(()=>s.Save(new Student { StudentNumber="X",FullName="X",Department="X",Email="invalid" }),"Email validation");
 Reject(()=>s.Save(new Subject { Code="test",Name="Duplicate" }),"Unique subject code");
 var a=new Attendance { StudentId=student.Id,SubjectId=subject.Id,Date=new DateTime(2026,1,1,10,0,0),IsPresent=true }; s.Save(a);
 Reject(()=>s.Save(new Attendance { StudentId=student.Id,SubjectId=subject.Id,Date=new DateTime(2026,1,1,20,0,0) }),"Attendance date normalization and duplicate prevention");
 a.IsPresent=false; s.Save(a); Check(!s.Attendance().Single(x=>x.Id==a.Id).IsPresent,"Update attendance");
 var g=new Grade { StudentId=student.Id,SubjectId=subject.Id,Score=80 }; s.Save(g);
 Reject(()=>s.Save(new Grade { StudentId=student.Id,SubjectId=subject.Id,Score=90 }),"Unique grade");
 foreach(var score in new[]{-1d,101d,double.NaN,double.PositiveInfinity}) Reject(()=>s.Save(new Grade { StudentId=student.Id,SubjectId=subject.Id,Score=score }),"Invalid grade "+score);
 Reject(()=>s.Save(new Grade { StudentId=99999,SubjectId=subject.Id,Score=50 }),"Foreign key validation");
 g.Score=100; s.Save(g); Check(s.Grades().Single(x=>x.Id==g.Id).Score==100,"Update grade");
 var second=new Subject { Code="SECOND",Name="Second" }; s.Save(second); s.Save(new Grade { StudentId=student.Id,SubjectId=second.Id,Score=50 });
 Check(s.Grades().Where(x=>x.StudentId==student.Id).Average(x=>x.Score)==75,"Student average");
 var reopened=new StudentHubService(path); reopened.Initialize(); Check(reopened.Students("TEST001").Count==1 && reopened.Grades().Any(x=>x.Id==g.Id),"Persistence in new context/service");
 var csv=StudentHubService.Csv(new[]{new Grade { Student=new Student { StudentNumber="=1+1",FullName="اسم, \"خاص\"" },Subject=subject,Score=95.5 }});
 Check(csv.Contains("'=1+1") && csv.Contains("95.5") && csv.Contains("\"\"خاص\"\""),"CSV escaping, formula protection and decimal format");
 using(var db=s.Open()) { db.Grades.Add(new Grade { StudentId=student.Id,SubjectId=subject.Id,Score=101 }); try { db.SaveChanges(); throw new Exception("Database constraints failed"); } catch(DbUpdateException) { Check(true,"Database constraint enforcement"); } }
 s.Delete<Attendance>(a.Id); Check(!s.Attendance().Any(x=>x.Id==a.Id),"Delete attendance");
 s.Delete<Grade>(g.Id); Check(!s.Grades().Any(x=>x.Id==g.Id),"Delete grade");
 s.Delete<Subject>(second.Id); Check(!s.Grades().Any(x=>x.SubjectId==second.Id),"Subject cascade");
 s.Save(new Attendance { StudentId=student.Id,SubjectId=subject.Id }); s.Save(new Grade { StudentId=student.Id,SubjectId=subject.Id,Score=0 });
 s.Delete<Student>(student.Id); Check(!s.Attendance().Any(x=>x.StudentId==student.Id) && !s.Grades().Any(x=>x.StudentId==student.Id),"Student cascade");
 s.Delete<Subject>(subject.Id); Check(!s.Subjects().Any(x=>x.Id==subject.Id),"Delete subject");
 foreach(var x in s.Students()) s.Delete<Student>(x.Id); foreach(var x in s.Subjects()) s.Delete<Subject>(x.Id);
 s.Initialize(); Check(s.Students().Count==0 && s.Subjects().Count==0,"Deleted demo data stays deleted");
 Console.WriteLine($"{passed} checks passed.");
} finally { Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools(); Directory.Delete(folder,true); }
