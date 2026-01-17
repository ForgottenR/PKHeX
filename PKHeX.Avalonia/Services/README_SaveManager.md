# SaveManager Service

The `SaveManager` service provides comprehensive save file management with event-driven notifications for PKHeX.Avalonia.

## Features

- **ObservableObject base**: Integrates with CommunityToolkit.Mvvm for automatic property change notifications
- **Event-driven architecture**: Raises events for load, save, and error operations
- **Edited state tracking**: Automatically tracks and notifies when save file is modified
- **Synchronous and asynchronous methods**: Provides both sync and async operations
- **Comprehensive error handling**: Detailed error information via events

## Comparison with SaveFileManager

| Feature | SaveManager (New) | SaveFileManager (Existing) |
|---------|-------------------|---------------------------|
| Base class | `ObservableObject` | None |
| Notifications | Events + PropertyChanged | Return values |
| Edited state tracking | Yes | No |
| Async support | Yes | Yes |
| Event pattern | FileLoaded, FileSaved, ErrorOccurred | Result objects |
| Data binding | Full support | Manual updates required |

## Usage Examples

### Basic Setup

```csharp
using PKHeX.Avalonia.Services;
using PKHeX.Core;

public class MainWindowViewModel : ViewModelBase
{
    private readonly SaveManager _saveManager;
    
    public MainWindowViewModel()
    {
        _saveManager = new SaveManager();
        
        // Subscribe to events
        _saveManager.FileLoaded += OnSaveFileLoaded;
        _saveManager.FileSaved += OnSaveFileSaved;
        _saveManager.ErrorOccurred += OnErrorOccurred;
        
        // Bind to properties
        _currentSaveFile = _saveManager.CurrentSaveFile;
        _saveManager.PropertyChanged += (s, e) => 
        {
            if (e.PropertyName == nameof(SaveManager.CurrentSaveFile))
            {
                // Save file changed
                OnPropertyChanged(nameof(CurrentSaveFile));
            }
        };
    }
    
    private void OnSaveFileLoaded(object? sender, SaveFileLoadedEventArgs e)
    {
        Console.WriteLine($"Loaded save from {e.FilePath}");
        Console.WriteLine($"Game: {e.SaveFile.Version}");
        Console.WriteLine($"Trainer: {e.SaveFile.OT}");
    }
    
    private void OnSaveFileSaved(object? sender, SaveFileSavedEventArgs e)
    {
        Console.WriteLine($"Saved to {e.FilePath}");
    }
    
    private void OnErrorOccurred(object? sender, SaveFileErrorEventArgs e)
    {
        Console.WriteLine($"Error during {e.Operation}: {e.ErrorMessage}");
        if (e.Exception != null)
        {
            Console.WriteLine($"Exception: {e.Exception.Message}");
        }
    }
}
```

### Loading a Save File

```csharp
// Simple synchronous load
if (_saveManager.Load("path/to/save.sav"))
{
    // Load successful, CurrentSaveFile is now set
    var currentFile = _saveManager.CurrentSaveFile;
}

// Asynchronous load
var success = await _saveManager.LoadAsync("path/to/save.sav");

// Check if load was successful
if (_saveManager.IsSaveFileLoaded)
{
    var saveFile = _saveManager.CurrentSaveFile;
}
```

### Saving a Save File

```csharp
// Save to current location (if file was loaded from disk)
if (_saveManager.Save())
{
    Console.WriteLine("Save successful!");
}

// Save to specific location
if (_saveManager.Save("path/to/new/location.sav"))
{
    Console.WriteLine("Save successful!");
}

// Async save
var success = await _saveManager.SaveAsync("path/to/save.sav");
```

### Listening for Changes

The SaveManager automatically tracks when the save file is edited:

```csharp
// CurrentSaveFile.State.Edited is automatically updated
if (_saveManager.CurrentSaveFile?.State.Edited == true)
{
    Console.WriteLine("Save file has unsaved changes");
}

// Subscribe to property changes to detect edits
_saveManager.PropertyChanged += (s, e) =>
{
    if (e.PropertyName == nameof(SaveManager.CurrentSaveFile))
    {
        var edited = _saveManager.CurrentSaveFile?.State.Edited ?? false;
        Console.WriteLine($"Save file edited state: {edited}");
    }
};
```

### Complete Example with Data Binding

```csharp
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly SaveManager _saveManager;
    
    [ObservableProperty]
    private SaveFile? _currentSaveFile;
    
    [ObservableProperty]
    private string? _currentFilePath;
    
    [ObservableProperty]
    private string _statusText = "Ready";
    
    [ObservableProperty]
    private bool _hasUnsavedChanges;
    
    public MainWindowViewModel()
    {
        _saveManager = new SaveManager();
        
        // Subscribe to events
        _saveManager.FileLoaded += (s, e) =>
        {
            CurrentFilePath = e.FilePath;
            StatusText = $"Loaded: {Path.GetFileName(e.FilePath)}";
        };
        
        _saveManager.FileSaved += (s, e) =>
        {
            StatusText = $"Saved: {Path.GetFileName(e.FilePath)}";
        };
        
        _saveManager.ErrorOccurred += (s, e) =>
        {
            StatusText = $"Error: {e.ErrorMessage}";
        };
        
        // Bind to property changes
        _saveManager.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(SaveManager.CurrentSaveFile))
            {
                CurrentSaveFile = _saveManager.CurrentSaveFile;
                HasUnsavedChanges = CurrentSaveFile?.State.Edited ?? false;
            }
        };
    }
    
    [RelayCommand]
    public async Task OpenFileAsync()
    {
        var filePath = await ShowOpenFileDialog();
        if (!string.IsNullOrEmpty(filePath))
        {
            await _saveManager.LoadAsync(filePath);
        }
    }
    
    [RelayCommand(CanExecute = nameof(CanSave))]
    public async Task SaveFileAsync()
    {
        await _saveManager.SaveAsync();
    }
    
    private bool CanSave() => _saveManager.IsSaveFileLoaded;
}
```

## Properties and Events

### Properties

- `CurrentSaveFile`: The currently loaded `SaveFile` instance (null if none loaded)
- `CurrentFilePath`: The full path to the loaded file
- `CurrentFileName`: Just the filename without directory path
- `IsSaveFileLoaded`: Boolean indicating if a save file is loaded
- `SaveExportSettings`: Settings for binary export (headers, footers, etc.)

### Events

- `FileLoaded`: Raised when a save file is successfully loaded
- `FileSaved`: Raised when a save file is successfully saved
- `ErrorOccurred`: Raised when an error occurs during load or save

### Methods

- `Load(string path)`: Synchronously load a save file
- `LoadAsync(string path)`: Asynchronously load a save file
- `Save(string path)`: Synchronously save to specific path
- `Save()`: Synchronously save to current location
- `SaveAsync(string path)`: Asynchronously save to specific path
- `SaveAsync()`: Asynchronously save to current location
- `Unload()`: Unload the current save file

## Integration with Existing Services

The `SaveManager` can coexist with the existing `SaveFileManager`. It's recommended to use:

- `SaveFileManager` for simple load/save operations using result objects
- `SaveManager` for MVVM applications requiring data binding and event notifications

Both services can be registered in the DI container:

```csharp
// In App.axaml.cs or your DI setup
services.AddSingleton<ISaveFileManager, SaveFileManager>();
services.AddSingleton<SaveManager>();
```
