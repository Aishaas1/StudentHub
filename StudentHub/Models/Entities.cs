namespace StudentHub.Models;
public class Student {
 public int Id { get; set; }
 public string StudentNumber { get; set; } = "";
 public string FullName { get; set; } = "";
 public string Department { get; set; } = "";
 public int AcademicLevel { get; set; } = 1;
 public string Email { get; set; } = "";
 public string PhoneNumber { get; set; } = "";
 public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
public class Subject {
 public int Id { get; set; }
 public string Code { get; set; } = "";
 public string Name { get; set; } = "";
 public int CreditHours { get; set; } = 3;
}
public class Attendance {
 public int Id { get; set; }
 public int StudentId { get; set; }
 public Student Student { get; set; } = null!;
 public int SubjectId { get; set; }
 public Subject Subject { get; set; } = null!;
 public DateTime Date { get; set; } = DateTime.Today;
 public bool IsPresent { get; set; } = true;
 public string Status => IsPresent ? "حاضر" : "غائب";
}
public class Grade {
 public int Id { get; set; }
 public int StudentId { get; set; }
 public Student Student { get; set; } = null!;
 public int SubjectId { get; set; }
 public Subject Subject { get; set; } = null!;
 public double Score { get; set; }
}
