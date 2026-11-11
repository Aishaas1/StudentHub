using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using StudentHub.Data;
using StudentHub.Models;
namespace StudentHub.Services;

public class StudentHubService(string databasePath)
{
    public HubDbContext Open() => new(databasePath);
    public void Initialize()
    {
        using var db = Open();
        DatabaseInitializer.Initialize(db);
    }
    public List<Student> Students(string search = "")
    {
        using var db = Open();
        return db.Students.AsNoTracking().Where(s => s.FullName.Contains(search) || s.StudentNumber.Contains(search)).OrderByDescending(s => s.CreatedAt).ThenByDescending(s => s.Id).ToList();
    }
    public List<Subject> Subjects()
    {
        using var db = Open();
        return db.Subjects.AsNoTracking().OrderBy(s => s.Code).ToList();
    }
    public List<Attendance> Attendance()
    {
        using var db = Open();
        return db.Attendance.AsNoTracking().Include(x => x.Student).Include(x => x.Subject).OrderByDescending(x => x.Date).ToList();
    }
    public List<Grade> Grades()
    {
        using var db = Open();
        return db.Grades.AsNoTracking().Include(x => x.Student).Include(x => x.Subject).OrderBy(x => x.Student.FullName).ToList();
    }
    static void Require(bool condition, string message)
    {
        if (!condition)
            throw new ValidationException(message);
    }
    public void Save(Student s)
    {
        s.StudentNumber = s.StudentNumber.Trim();
        s.FullName = s.FullName.Trim();
        s.Department = s.Department.Trim();
        s.Email = s.Email.Trim();
        s.PhoneNumber = s.PhoneNumber.Trim();
        Require(s.StudentNumber.Length is > 0 and <= 30 && s.FullName.Length is > 0 and <= 120 && s.Department.Length is > 0 and <= 80, "أدخل الرقم الجامعي والاسم والقسم ضمن الأطوال المسموحة.");
        Require(s.AcademicLevel is >= 1 and <= 8, "المستوى الأكاديمي من 1 إلى 8.");
        Require(s.Email.Length <= 150 && (s.Email.Length == 0 || new EmailAddressAttribute().IsValid(s.Email)), "البريد الإلكتروني غير صحيح.");
        Require(s.PhoneNumber.Length <= 25 && (s.PhoneNumber.Length == 0 || s.PhoneNumber.All(c => char.IsDigit(c) || "+- ()".Contains(c))), "رقم الهاتف غير صحيح.");
        using var db = Open();
        Require(!db.Students.Any(x => x.StudentNumber == s.StudentNumber && x.Id != s.Id), "الرقم الجامعي موجود بالفعل.");
        if (s.Id == 0)
            db.Students.Add(s);
        else
            db.Students.Update(s);
        db.SaveChanges();
    }
    public void Save(Subject s)
    {
        s.Code = s.Code.Trim();
        s.Name = s.Name.Trim();
        Require(s.Code.Length is > 0 and <= 20 && s.Name.Length is > 0 and <= 120, "أدخل رمز المادة واسمها ضمن الأطوال المسموحة.");
        Require(s.CreditHours is >= 1 and <= 6, "الساعات من 1 إلى 6.");
        using var db = Open();
        Require(!db.Subjects.Any(x => x.Code == s.Code && x.Id != s.Id), "رمز المادة موجود بالفعل.");
        if (s.Id == 0)
            db.Subjects.Add(s);
        else
            db.Subjects.Update(s);
        db.SaveChanges();
    }
    public void Save(Attendance a)
    {
        a.Date = a.Date.Date;
        using var db = Open();
        Require(db.Students.Any(x => x.Id == a.StudentId) && db.Subjects.Any(x => x.Id == a.SubjectId), "اختر الطالب والمادة.");
        Require(!db.Attendance.Any(x => x.StudentId == a.StudentId && x.SubjectId == a.SubjectId && x.Date == a.Date && x.Id != a.Id), "تم تسجيل حضور هذا الطالب للمادة في هذا التاريخ مسبقًا.");
        a.Student = null!;
        a.Subject = null!;
        if (a.Id == 0)
            db.Attendance.Add(a);
        else
            db.Attendance.Update(a);
        db.SaveChanges();
    }
    public void Save(Grade g)
    {
        Require(double.IsFinite(g.Score) && g.Score is >= 0 and <= 100, "الدرجة من 0 إلى 100.");
        using var db = Open();
        Require(db.Students.Any(x => x.Id == g.StudentId) && db.Subjects.Any(x => x.Id == g.SubjectId), "اختر الطالب والمادة.");
        Require(!db.Grades.Any(x => x.StudentId == g.StudentId && x.SubjectId == g.SubjectId && x.Id != g.Id), "توجد درجة لهذا الطالب والمادة؛ استخدم تعديل.");
        g.Student = null!;
        g.Subject = null!;
        if (g.Id == 0)
            db.Grades.Add(g);
        else
            db.Grades.Update(g);
        db.SaveChanges();
    }
    public void Delete<T>(int id) where T : class
    {
        using var db = Open();
        var entity = db.Set<T>().Find(id);
        if (entity != null)
        {
            db.Remove(entity);
            db.SaveChanges();
        }
    }
    public static string Csv(IEnumerable<Grade> grades)
    {
        // Quote every cell and neutralize spreadsheet formulas in user-entered text.
        static string Cell(string s)
        {
            if (s.TrimStart().StartsWith('=') || s.TrimStart().StartsWith('+') || s.TrimStart().StartsWith('-') || s.TrimStart().StartsWith('@'))
                s = "'" + s;
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }
        var text = new StringBuilder("Student ID,Full Name,Subject Code,Subject,Score\r\n");
        foreach (var g in grades)
            text.AppendLine(string.Join(",", new[] { g.Student.StudentNumber, g.Student.FullName, g.Subject.Code, g.Subject.Name, g.Score.ToString(CultureInfo.InvariantCulture) }.Select(Cell)));
        return text.ToString();
    }
}
