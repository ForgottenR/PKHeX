using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using PKHeX.Core;

namespace PKHeX.Avalonia.Services;

/// <summary>
/// Event arguments for when a save file is loaded
/// </summary>
public class SaveFileLoadedEventArgs : EventArgs
{
    /// <summary>
    /// The loaded save file
    /// </summary>
    public SaveFile SaveFile { get; }

    /// <summary>
    /// The path from which the file was loaded
    /// </summary>
    public string FilePath { get; }

    public SaveFileLoadedEventArgs(SaveFile saveFile, string filePath)
    {
        SaveFile = saveFile;
        FilePath = filePath;
    }
}

/// <summary>
/// Event arguments for when a save file is saved
/// </summary>
public class SaveFileSavedEventArgs : EventArgs
{
    /// <summary>
    /// The saved save file
    /// </summary>
    public SaveFile SaveFile { get; }

    /// <summary>
    /// The path to which the file was saved
    /// </summary>
    public string FilePath { get; }

    public SaveFileSavedEventArgs(SaveFile saveFile, string filePath)
    {
        SaveFile = saveFile;
        FilePath = filePath;
    }
}

/// <summary>
/// Event arguments for when a save operation fails
/// </summary>
public class SaveFileErrorEventArgs : EventArgs
{
    /// <summary>
    /// Error message describing what went wrong
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// The exception that caused the error, if any
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// The operation that failed ("Load" or "Save")
    /// </summary>
    public string Operation { get; }

    public SaveFileErrorEventArgs(string errorMessage, string operation, Exception? exception = null)
    {
        ErrorMessage = errorMessage;
        Exception = exception;
        Operation = operation;
    }
}

/// <summary>
/// Manages save file operations including loading, saving, and maintaining
/// the current SaveFile instance with proper event notifications
/// </summary>
public sealed class SaveManager : ObservableObject
{
    private SaveFile? _currentSaveFile;
    private string? _currentFilePath;

    /// <summary>
    /// Gets the currently loaded save file
    /// </summary>
    public SaveFile? CurrentSaveFile
    {
        get => _currentSaveFile;
        private set
        {
            if (ReferenceEquals(_currentSaveFile, value))
                return;

            _currentSaveFile = value;

            OnPropertyChanged();
            OnPropertyChanged(nameof(IsSaveFileLoaded));
            OnPropertyChanged(nameof(CurrentFileName));
            OnPropertyChanged(nameof(HasUnsavedChanges));
        }
    }

    /// <summary>
    /// Gets the current file path
    /// </summary>
    public string? CurrentFilePath
    {
        get => _currentFilePath;
        private set
        {
            if (_currentFilePath == value)
                return;

            _currentFilePath = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CurrentFileName));
        }
    }

    /// <summary>
    /// Gets the name of the current file without the directory path
    /// </summary>
    public string CurrentFileName => Path.GetFileName(CurrentFilePath ?? string.Empty);

    /// <summary>
    /// Gets whether a save file is currently loaded
    /// </summary>
    public bool IsSaveFileLoaded => CurrentSaveFile is not null;

    /// <summary>
    /// Gets whether the current save file has unsaved changes
    /// </summary>
    public bool HasUnsavedChanges => CurrentSaveFile?.State.Edited ?? false;

    /// <summary>
    /// Gets or sets the <see cref="BinaryExportSetting"/> used when saving files
    /// </summary>
    public BinaryExportSetting SaveExportSettings { get; set; } = BinaryExportSetting.None;

    /// <summary>
    /// Occurs when a save file is successfully loaded
    /// </summary>
    public event EventHandler<SaveFileLoadedEventArgs>? FileLoaded;

    /// <summary>
    /// Occurs when a save file is successfully saved
    /// </summary>
    public event EventHandler<SaveFileSavedEventArgs>? FileSaved;

    /// <summary>
    /// Occurs when an error occurs during load or save operations
    /// </summary>
    public event EventHandler<SaveFileErrorEventArgs>? ErrorOccurred;

    public SaveManager()
    {
    }

    /// <summary>
    /// Loads a save file from the specified path
    /// </summary>
    /// <param name="path">The path to the save file</param>
    /// <returns><c>true</c> if the file was loaded successfully; otherwise, <c>false</c></returns>
    public bool Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            OnErrorOccurred("File path cannot be empty.", "Load");
            return false;
        }

        if (!File.Exists(path))
        {
            OnErrorOccurred($"File not found: {path}", "Load");
            return false;
        }

        try
        {
            var fileData = File.ReadAllBytes(path);
            var saveFile = SaveUtil.GetSaveFile(fileData, path);
            if (saveFile is null)
            {
                OnErrorOccurred($"Unable to recognize save file format: {path}", "Load");
                return false;
            }

            CurrentSaveFile = saveFile;
            CurrentFilePath = Path.GetFullPath(path);

            OnFileLoaded(saveFile, CurrentFilePath);
            return true;
        }
        catch (Exception ex)
        {
            OnErrorOccurred($"Error loading save file: {ex.Message}", "Load", ex);
            return false;
        }
    }

    /// <summary>
    /// Loads a save file from the specified path asynchronously
    /// </summary>
    /// <param name="path">The path to the save file</param>
    /// <returns><c>true</c> if the file was loaded successfully; otherwise, <c>false</c></returns>
    public Task<bool> LoadAsync(string path) => Task.Run(() => Load(path));

    /// <summary>
    /// Saves the current save file to the specified path
    /// </summary>
    /// <param name="path">The path where to save the file</param>
    /// <returns><c>true</c> if the file was saved successfully; otherwise, <c>false</c></returns>
    public bool Save(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            OnErrorOccurred("Save path cannot be empty.", "Save");
            return false;
        }

        if (CurrentSaveFile is null)
        {
            OnErrorOccurred("No save file is currently loaded.", "Save");
            return false;
        }

        try
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var data = CurrentSaveFile.Write(SaveExportSettings);
            File.WriteAllBytes(path, data.ToArray());

            CurrentFilePath = Path.GetFullPath(path);
            CurrentSaveFile.State.Edited = false;

            OnFileSaved(CurrentSaveFile, CurrentFilePath);
            return true;
        }
        catch (Exception ex)
        {
            OnErrorOccurred($"Error saving save file: {ex.Message}", "Save", ex);
            return false;
        }
    }

    /// <summary>
    /// Saves the current save file to the specified path asynchronously
    /// </summary>
    /// <param name="path">The path where to save the file</param>
    /// <returns><c>true</c> if the file was saved successfully; otherwise, <c>false</c></returns>
    public Task<bool> SaveAsync(string path) => Task.Run(() => Save(path));

    /// <summary>
    /// Saves the current save file back to its original location
    /// </summary>
    /// <returns><c>true</c> if the file was saved successfully; otherwise, <c>false</c></returns>
    public bool Save()
    {
        if (string.IsNullOrEmpty(CurrentFilePath))
        {
            OnErrorOccurred("No file path is set. Use Save(path) to specify a location.", "Save");
            return false;
        }

        return Save(CurrentFilePath);
    }

    /// <summary>
    /// Saves the current save file back to its original location asynchronously
    /// </summary>
    /// <returns><c>true</c> if the file was saved successfully; otherwise, <c>false</c></returns>
    public Task<bool> SaveAsync() => Task.Run(() => Save());

    /// <summary>
    /// Unloads the current save file
    /// </summary>
    public void Unload()
    {
        CurrentSaveFile = null;
        CurrentFilePath = null;
        
        // Note: We're not raising events for unload as it's an explicit action
    }

    private void OnFileLoaded(SaveFile saveFile, string filePath)
    {
        FileLoaded?.Invoke(this, new SaveFileLoadedEventArgs(saveFile, filePath));
    }

    private void OnFileSaved(SaveFile saveFile, string filePath)
    {
        FileSaved?.Invoke(this, new SaveFileSavedEventArgs(saveFile, filePath));
    }

    private void OnErrorOccurred(string errorMessage, string operation, Exception? exception = null)
    {
        ErrorOccurred?.Invoke(this, new SaveFileErrorEventArgs(errorMessage, operation, exception));
    }
}
