using Microsoft.EntityFrameworkCore;
using StudentHub.Models;
namespace StudentHub.Data;
public class HubDbContext : DbContext {
 private readonly string path;
 public HubDbContext(string path) => this.path = path;
 public DbSet<Student> Students => Set<Student>();
 public DbSet<Subject> Subjects => Set<Subject>();
 public DbSet<Attendance> Attendance => Set<Attendance>();
 public DbSet<Grade> Grades => Set<Grade>();
 protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite(new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder { DataSource = path, ForeignKeys = true }.ToString());
 protected override void OnModelCreating(ModelBuilder b) {
  b.Entity<Student>().HasIndex(x => x.StudentNumber).IsUnique();
  b.Entity<Student>().Property(x => x.StudentNumber).UseCollation("NOCASE");
  b.Entity<Student>().ToTable(t => { t.HasCheckConstraint("CK_Student_Level", "AcademicLevel BETWEEN 1 AND 8"); t.HasCheckConstraint("CK_Student_Required", "length(trim(StudentNumber)) > 0 AND length(trim(FullName)) > 0 AND length(trim(Department)) > 0"); });
  b.Entity<Subject>().HasIndex(x => x.Code).IsUnique();
  b.Entity<Subject>().Property(x => x.Code).UseCollation("NOCASE");
  b.Entity<Subject>().ToTable(t => { t.HasCheckConstraint("CK_Subject_Hours", "CreditHours BETWEEN 1 AND 6"); t.HasCheckConstraint("CK_Subject_Required", "length(trim(Code)) > 0 AND length(trim(Name)) > 0"); });
  b.Entity<Attendance>().Ignore(x => x.Status);
  b.Entity<Attendance>().HasIndex(x => new { x.StudentId, x.SubjectId, x.Date }).IsUnique();
  b.Entity<Grade>().HasIndex(x => new { x.StudentId, x.SubjectId }).IsUnique();
  b.Entity<Grade>().ToTable(t => t.HasCheckConstraint("CK_Grade_Score", "Score >= 0 AND Score <= 100"));
  b.Entity<Attendance>().HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Cascade);
  b.Entity<Attendance>().HasOne(x => x.Subject).WithMany().HasForeignKey(x => x.SubjectId).OnDelete(DeleteBehavior.Cascade);
  b.Entity<Grade>().HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Cascade);
  b.Entity<Grade>().HasOne(x => x.Subject).WithMany().HasForeignKey(x => x.SubjectId).OnDelete(DeleteBehavior.Cascade);
 }
}
