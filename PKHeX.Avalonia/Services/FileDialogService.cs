using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace PKHeX.Avalonia.Services;

/// <summary>
/// Service for handling file dialogs (open/save)
/// </summary>
public interface IFileDialogService
{
    /// <summary>
    /// Show open file dialog and return selected file path
    /// </summary>
    Task<string?> ShowOpenFileDialogAsync(string title = "Open File", string? defaultPath = null);
    
    /// <summary>
    /// Show save file dialog and return selected file path
    /// </summary>
    Task<string?> ShowSaveFileDialogAsync(string title = "Save File", string? defaultFileName = null, string? defaultPath = null);
}

/// <summary>
/// Implementation of file dialog service using Avalonia's storage API
/// </summary>
public class FileDialogService : IFileDialogService
{
    private readonly Window _parentWindow;
    
    public FileDialogService(Window parentWindow)
    {
        _parentWindow = parentWindow ?? throw new ArgumentNullException(nameof(parentWindow));
    }
    
    public async Task<string?> ShowOpenFileDialogAsync(string title = "Open File", string? defaultPath = null)
    {
        try
        {
            var options = new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = GetSaveFileFilters()
            };
            
            if (!string.IsNullOrEmpty(defaultPath) && Directory.Exists(defaultPath))
            {
                options.SuggestedStartLocation = await _parentWindow.StorageProvider.TryGetFolderFromPathAsync(defaultPath);
            }
            
            var results = await _parentWindow.StorageProvider.OpenFilePickerAsync(options);
            
            return results.Count > 0 ? results[0].Path.LocalPath : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error showing open file dialog: {ex.Message}");
            return null;
        }
    }
    
    public async Task<string?> ShowSaveFileDialogAsync(string title = "Save File", string? defaultFileName = null, string? defaultPath = null)
    {
        try
        {
            var options = new FilePickerSaveOptions
            {
                Title = title,
                SuggestedFileName = defaultFileName,
                FileTypeChoices = GetSaveFileFilters()
            };
            
            if (!string.IsNullOrEmpty(defaultPath) && Directory.Exists(defaultPath))
            {
                options.SuggestedStartLocation = await _parentWindow.StorageProvider.TryGetFolderFromPathAsync(defaultPath);
            }
            
            var result = await _parentWindow.StorageProvider.SaveFilePickerAsync(options);
            
            return result?.Path.LocalPath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error showing save file dialog: {ex.Message}");
            return null;
        }
    }
    
    private static FilePickerFileType[] GetSaveFileFilters()
    {
        return
        [
            new("All Save Files")
            {
                Patterns = ["*.sav", "*.dsv", "*.dat", "*.gci", "*.sav.*"],
                AppleUniformTypeIdentifiers = ["public.data"],
                MimeTypes = ["application/octet-stream"]
            },
            new("All Files")
            {
                Patterns = ["*.*"],
                AppleUniformTypeIdentifiers = ["public.data"],
                MimeTypes = ["application/octet-stream"]
            }
        ];
    }
}