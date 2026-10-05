using Microsoft.Win32;

namespace FastDbExplorer.Wpf.Services;

public interface IFileDialogService
{
    /// <summary>Returns the chosen map file path, or null if the user cancelled.</summary>
    string? PickMapFile();
}

public sealed class FileDialogService : IFileDialogService
{
    public string? PickMapFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "باز کردن فایل نقشه",
            Filter = "فایل‌های نقشه|*.mbtiles;*.gmdb;*.pbf|همه‌ی فایل‌ها|*.*",
            CheckFileExists = true
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
