using System.IO;
using System.Windows;
using StudentHub.Services;
using StudentHub.ViewModels;
namespace StudentHub;

public partial class App : Application
{
    public StudentHubService Service { get; private set; } = null!;
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "StudentHub");
            Directory.CreateDirectory(folder);
            Service = new StudentHubService(Path.Combine(folder, "studenthub.db"));
            Service.Initialize();
            MainWindow = new MainWindow { DataContext = new MainViewModel(Service, new DialogService()) };
            MainWindow.Show();
        }
        catch (Exception ex) { System.Diagnostics.Trace.TraceError(ex.ToString()); MessageBox.Show("تعذر فتح قاعدة البيانات. تحقق من صلاحيات مجلد StudentHub داخل LocalAppData.", "StudentHub", MessageBoxButton.OK, MessageBoxImage.Error); Shutdown(1); }
    }
}
