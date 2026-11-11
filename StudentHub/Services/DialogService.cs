using System.Windows;
using Microsoft.Win32;
using StudentHub.Views;
namespace StudentHub.Services;

public class DialogService
{
    public bool Edit(object value, string title) => new EditorWindow(value, title) { Owner = Application.Current.MainWindow }.ShowDialog() == true;
    public bool Confirm(string message) => MessageBox.Show(message, "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
    public void Error(string message) => MessageBox.Show(message, "StudentHub", MessageBoxButton.OK, MessageBoxImage.Error);
    public string? ExportPath()
    {
        var dialog = new SaveFileDialog { Filter = "CSV (*.csv)|*.csv", FileName = "StudentHub-Results.csv" };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
