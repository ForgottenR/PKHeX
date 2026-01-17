using System;
using System.IO;
using System.Threading.Tasks;
using PKHeX.Core;

namespace PKHeX.Avalonia.Services;

/// <summary>
/// Service for managing the currently loaded save file
/// </summary>
public interface ISaveFileManager
{
    /// <summary>
    /// Currently loaded save file
    /// </summary>
    SaveFile? CurrentSaveFile { get; }
    
    /// <summary>
    /// Path to the currently loaded save file
    /// </summary>
    string? CurrentFilePath { get; }
    
    /// <summary>
    /// Whether a save file is currently loaded
    /// </summary>
    bool HasSaveFile { get; }
    
    /// <summary>
    /// Event raised when a save file is loaded
    /// </summary>
    event EventHandler<SaveFileEventArgs>? SaveFileChanged;
    
    /// <summary>
    /// Load a save file from the specified path
    /// </summary>
    Task<LoadResult> LoadSaveFileAsync(string filePath);
    
    /// <summary>
    /// Save the current save file to the specified path
    /// </summary>
    Task<SaveResult> SaveSaveFileAsync(string? filePath = null);
    
    /// <summary>
    /// Unload the current save file
    /// </summary>
    void UnloadSaveFile();
}

/// <summary>
/// Result of loading a save file
/// </summary>
public readonly record struct LoadResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public SaveFile? SaveFile { get; init; }
    public string? FilePath { get; init; }
    
    public static LoadResult SuccessResult(SaveFile saveFile, string filePath) => new()
    {
        Success = true,
        SaveFile = saveFile,
        FilePath = filePath
    };
    
    public static LoadResult FailureResult(string errorMessage) => new()
    {
        Success = false,
        ErrorMessage = errorMessage
    };
}

/// <summary>
/// Result of saving a save file
/// </summary>
public readonly record struct SaveResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public string? FilePath { get; init; }
    
    public static SaveResult SuccessResult(string filePath) => new()
    {
        Success = true,
        FilePath = filePath
    };
    
    public static SaveResult FailureResult(string errorMessage) => new()
    {
        Success = false,
        ErrorMessage = errorMessage
    };
}

/// <summary>
/// Event arguments for save file changes
/// </summary>
public class SaveFileEventArgs : EventArgs
{
    /// <summary>
    /// The save file that was loaded or changed
    /// </summary>
    public SaveFile? SaveFile { get; }
    
    /// <summary>
    /// The file path of the save file
    /// </summary>
    public string? FilePath { get; }
    
    public SaveFileEventArgs(SaveFile? saveFile, string? filePath = null)
    {
        SaveFile = saveFile;
        FilePath = filePath;
    }
}

/// <summary>
/// Implementation of save file manager
/// </summary>
public class SaveFileManager : ISaveFileManager
{
    private SaveFile? _currentSaveFile;
    private string? _currentFilePath;
    
    public SaveFile? CurrentSaveFile => _currentSaveFile;
    public string? CurrentFilePath => _currentFilePath;
    public bool HasSaveFile => _currentSaveFile != null;
    
    public event EventHandler<SaveFileEventArgs>? SaveFileChanged;
    
    public async Task<LoadResult> LoadSaveFileAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return LoadResult.FailureResult("File path cannot be empty");
            
            if (!File.Exists(filePath))
                return LoadResult.FailureResult($"File not found: {filePath}");
            
            // Try to detect and load the save file
            var fileData = await File.ReadAllBytesAsync(filePath);
            var saveFile = SaveUtil.GetSaveFile(fileData, filePath);
            
            if (saveFile == null)
                return LoadResult.FailureResult("Unable to recognize save file format");
            
            _currentSaveFile = saveFile;
            _currentFilePath = filePath;
            
            // Notify that save file has changed
            SaveFileChanged?.Invoke(this, new SaveFileEventArgs(saveFile, filePath));
            
            return LoadResult.SuccessResult(saveFile, filePath);
        }
        catch (UnauthorizedAccessException)
        {
            return LoadResult.FailureResult($"Access denied to file: {filePath}");
        }
        catch (Exception ex)
        {
            return LoadResult.FailureResult($"Error loading save file: {ex.Message}");
        }
    }
    
    public async Task<SaveResult> SaveSaveFileAsync(string? filePath = null)
    {
        try
        {
            if (_currentSaveFile == null)
                return SaveResult.FailureResult("No save file loaded");
            
            var targetPath = filePath ?? _currentFilePath;
            if (string.IsNullOrWhiteSpace(targetPath))
                return SaveResult.FailureResult("No file path specified");
            
            // Get the final data to write
            var data = _currentSaveFile.Write();
            
            // Create backup if file exists
            if (File.Exists(targetPath))
            {
                var backupPath = targetPath + ".bak";
                File.Copy(targetPath, backupPath, true);
            }
            
            // Write the file
            await File.WriteAllBytesAsync(targetPath, data);
            
            _currentFilePath = targetPath;
            return SaveResult.SuccessResult(targetPath);
        }
        catch (UnauthorizedAccessException)
        {
            return SaveResult.FailureResult($"Access denied to file: {filePath ?? _currentFilePath}");
        }
        catch (Exception ex)
        {
            return SaveResult.FailureResult($"Error saving save file: {ex.Message}");
        }
    }
    
    public void UnloadSaveFile()
    {
        _currentSaveFile = null;
        _currentFilePath = null;
        
        // Notify that save file has been unloaded
        SaveFileChanged?.Invoke(this, new SaveFileEventArgs(null, null));
    }
}