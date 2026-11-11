using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using StudentHub.Models;
namespace StudentHub.Views;

public partial class EditorWindow : Window
{
    readonly object entity;
    readonly Dictionary<string, Control> controls = [];
    public EditorWindow(object entity, string title)
    {
        InitializeComponent();
        this.entity = entity;
        Title = title;
        Heading.Text = title;
        switch (entity)
        {
            case Student s:
                Text("StudentNumber", "الرقم الجامعي *", s.StudentNumber, 30);
                Text("FullName", "الاسم الكامل *", s.FullName, 120);
                Text("Department", "القسم *", s.Department, 80);
                Text("AcademicLevel", "المستوى الأكاديمي (1–8) *", s.AcademicLevel.ToString(), 1);
                Text("Email", "البريد الإلكتروني", s.Email, 150);
                Text("PhoneNumber", "رقم الهاتف", s.PhoneNumber, 25);
                break;
            case Subject s:
                Text("Code", "رمز المادة *", s.Code, 20);
                Text("Name", "اسم المادة *", s.Name, 120);
                Text("CreditHours", "الساعات المعتمدة (1–6) *", s.CreditHours.ToString(), 1);
                break;
            case Attendance a:
                Selectors(a.StudentId, a.SubjectId);
                Add("Date", "التاريخ *", new DatePicker { SelectedDate = a.Date });
                Add("IsPresent", "حالة الحضور", new CheckBox { Content = "حاضر (أزل التحديد لتسجيل غائب)", IsChecked = a.IsPresent, Margin = new Thickness(0, 10, 0, 15) });
                break;
            case Grade g:
                Selectors(g.StudentId, g.SubjectId);
                Text("Score", "الدرجة من 100 *", g.Score.ToString(CultureInfo.CurrentCulture), 8);
                break;
        }
    }
    void Add(string key, string label, Control control)
    {
        Fields.Children.Add(new TextBlock { Text = label, Margin = new Thickness(0, 8, 0, 6), FontWeight = FontWeights.SemiBold });
        Fields.Children.Add(control);
        controls[key] = control;
    }
    void Text(string key, string label, string value, int max) => Add(key, label, new TextBox { Text = value, MaxLength = max });
    void Selectors(int student, int subject)
    {
        var service = ((App)Application.Current).Service;
        Add("StudentId", "الطالب *", new ComboBox { ItemsSource = service.Students(), DisplayMemberPath = "FullName", SelectedValuePath = "Id", SelectedValue = student });
        Add("SubjectId", "المادة *", new ComboBox { ItemsSource = service.Subjects(), DisplayMemberPath = "Name", SelectedValuePath = "Id", SelectedValue = subject });
    }
    void Save_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Parse every input before assigning it: invalid numeric input cannot silently become zero.
            var values = new Dictionary<string, object?>();
            foreach (var (key, control) in controls)
            {
                var property = entity.GetType().GetProperty(key)!;
                values[key] = control switch
                {
                    TextBox t when property.PropertyType == typeof(int) => int.Parse(t.Text, CultureInfo.CurrentCulture),
                    TextBox t when property.PropertyType == typeof(double) => double.Parse(t.Text, CultureInfo.CurrentCulture),
                    TextBox t => t.Text,
                    ComboBox c => c.SelectedValue ?? throw new FormatException(),
                    DatePicker d => d.SelectedDate ?? throw new FormatException(),
                    CheckBox c => c.IsChecked == true,
                    _ => throw new FormatException()
                };
            }
            foreach (var (key, value) in values)
                entity.GetType().GetProperty(key)!.SetValue(entity, value);
            var service = ((App)Application.Current).Service;
            switch (entity)
            {
                case Student s:
                    service.Save(s);
                    break;
                case Subject s:
                    service.Save(s);
                    break;
                case Attendance a:
                    service.Save(a);
                    break;
                case Grade g:
                    service.Save(g);
                    break;
            }
            DialogResult = true;
        }
        catch (FormatException) { ErrorText.Text = "تحقق من الأرقام واختر الطالب والمادة والتاريخ."; }
        catch (OverflowException) { ErrorText.Text = "القيمة الرقمية أكبر من المسموح."; }
        catch (System.ComponentModel.DataAnnotations.ValidationException ex) { ErrorText.Text = ex.Message; }
        catch (Exception ex) { System.Diagnostics.Trace.TraceError(ex.ToString()); ErrorText.Text = "تعذر الحفظ. تحقق من عدم تكرار البيانات ومن صلاحية قاعدة البيانات."; }
    }
    void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
