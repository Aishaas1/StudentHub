using StudentHub.Models;
namespace StudentHub.Data;

public static class DatabaseInitializer
{
    public static void Initialize(HubDbContext db)
    {
        // EnsureCreated is sufficient for this fixed, small academic schema. Do not mix with migrations.
        if (!db.Database.EnsureCreated())
            return;
        using var transaction = db.Database.BeginTransaction();
        var students = new[] {
   new Student { StudentNumber="2026001", FullName="سارة أحمد", Department="علوم الحاسب", AcademicLevel=3, Email="sara@example.com", PhoneNumber="0501234567" },
   new Student { StudentNumber="2026002", FullName="محمد خالد", Department="نظم المعلومات", AcademicLevel=2, Email="mohammed@example.com" },
   new Student { StudentNumber="2026003", FullName="نورة علي", Department="علوم الحاسب", AcademicLevel=4, Email="noura@example.com" }
  };
        var subjects = new[] { new Subject { Code = "CS201", Name = "البرمجة الكائنية" }, new Subject { Code = "CS202", Name = "قواعد البيانات" }, new Subject { Code = "IS101", Name = "تحليل النظم" } };
        db.AddRange(students);
        db.AddRange(subjects);
        db.SaveChanges();
        for (int i = 0; i < 3; i++)
        {
            db.Attendance.Add(new Attendance { StudentId = students[i].Id, SubjectId = subjects[0].Id, Date = DateTime.Today, IsPresent = i != 1 });
            db.Grades.Add(new Grade { StudentId = students[i].Id, SubjectId = subjects[0].Id, Score = 85 + i * 5 });
        }
        db.SaveChanges();
        transaction.Commit();
    }
}
