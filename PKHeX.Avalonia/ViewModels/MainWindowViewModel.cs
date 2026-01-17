using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PKHeX.Avalonia.Services;
using PKHeX.Core;

namespace PKHeX.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ISaveFileManager _saveFileManager;
    private readonly IFileDialogService _fileDialogService;
    
    /// <summary>
    /// 窗口标题
    /// </summary>
    [ObservableProperty]
    private string _title = "PKHeX";

    /// <summary>
    /// 状态栏文本
    /// </summary>
    [ObservableProperty]
    private string _statusText = "Ready";

    /// <summary>
    /// 当前加载的存档文件路径
    /// </summary>
    [ObservableProperty]
    private string? _currentFilePath;
    
    /// <summary>
    /// 当前加载的存档文件
    /// </summary>
    [ObservableProperty] 
    private SaveFile? _currentSaveFile;
    
    /// <summary>
    /// 语言选择索引
    /// </summary>
    [ObservableProperty]
    private int _languageIndex;

    /// <summary>
    /// 构造函数
    /// </summary>
    public MainWindowViewModel(ISaveFileManager saveFileManager, IFileDialogService fileDialogService)
    {
        _saveFileManager = saveFileManager ?? throw new ArgumentNullException(nameof(saveFileManager));
        _fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
        
        // Register for save file changes
        _saveFileManager.SaveFileChanged += OnSaveFileChanged;
        
        // Set initial state
        UpdateViewModelFromManager();
    }
    
    /// <summary>
    /// 默认构造函数（用于设计时支持）
    /// </summary>
    public MainWindowViewModel()
    {
        _saveFileManager = new SaveFileManager();
        _fileDialogService = new Services.FileDialogService(new global::Avalonia.Controls.Window());
        StatusText = "PKHeX AvaloniaUI Ready (Design Mode)";
    }

    /// <summary>
    /// 打开文件命令（Menu: File > Open）
    /// </summary>
    [RelayCommand]
    private async Task MainMenuOpen()
    {
        try
        {
            StatusText = "Opening file...";
            
            var filePath = await _fileDialogService.ShowOpenFileDialogAsync("Open Save File", Path.GetDirectoryName(CurrentFilePath));
            if (string.IsNullOrEmpty(filePath))
            {
                StatusText = "Ready";
                return;
            }
            
            var result = await _saveFileManager.LoadSaveFileAsync(filePath);
            if (result.Success)
            {
                UpdateViewModelFromManager();
                var fileName = Path.GetFileName(filePath);
                Title = $"PKHeX - {fileName}";
                StatusText = $"Successfully loaded: {fileName}";
            }
            else
            {
                StatusText = $"Error loading file: {result.ErrorMessage}";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Unexpected error: {ex.Message}";
        }
    }
    
    /// <summary>
    /// 保存PKM命令（Menu: File > Save PKM...）
    /// WinForms中导出单个PKM文件的功能
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteWithSaveFile))]
    private async Task MainMenuSave()
    {
        StatusText = "Save PKM functionality not yet implemented";
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// 导出SAV命令（Menu: File > Export SAV...）
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteWithSaveFile))]
    private async Task ClickExportSAV()
    {
        try
        {
            StatusText = "Saving file...";
            
            string? targetPath = _saveFileManager.CurrentFilePath;
            if (string.IsNullOrEmpty(targetPath))
            {
                targetPath = await _fileDialogService.ShowSaveFileDialogAsync("Save Save File", "save.sav");
                if (string.IsNullOrEmpty(targetPath))
                {
                    StatusText = "Save cancelled";
                    return;
                }
            }
            
            var result = await _saveFileManager.SaveSaveFileAsync(targetPath);
            if (result.Success)
            {
                CurrentFilePath = result.FilePath;
                var fileName = Path.GetFileName(result.FilePath);
                Title = $"PKHeX - {fileName}";
                StatusText = $"Successfully saved: {fileName}";
            }
            else
            {
                StatusText = $"Error saving file: {result.ErrorMessage}";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Unexpected error: {ex.Message}";
        }
    }
    
    /// <summary>
    /// 退出应用程序（Menu: File > Quit）
    /// </summary>
    [RelayCommand]
    private void MainMenuExit()
    {
        System.Environment.Exit(0);
    }
    
    /// <summary>
    /// Showdown导入（Menu: Tools > Showdown > Import Set from Clipboard）
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteWithSaveFile))]
    private async Task ClickShowdownImportPKM()
    {
        StatusText = "Showdown Import not yet implemented";
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// Showdown导出宝可梦（Menu: Tools > Showdown > Export Set to Clipboard）
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteWithSaveFile))]
    private async Task ClickShowdownExportPKM()
    {
        StatusText = "Showdown Export PKM not yet implemented";
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// Showdown导出队伍（Menu: Tools > Showdown > Export Party to Clipboard）
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteWithSaveFile))]
    private async Task ClickShowdownExportParty()
    {
        StatusText = "Showdown Export Party not yet implemented";
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// Showdown导出当前盒子（Menu: Tools > Showdown > Export Current Box to Clipboard）
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteWithSaveFile))]
    private async Task ClickShowdownExportCurrentBox()
    {
        StatusText = "Showdown Export Current Box not yet implemented";
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// 加载盒子（Menu: Tools > Data > Load Boxes）
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteWithSaveFile))]
    private async Task MainMenuBoxLoad()
    {
        StatusText = "Load Boxes not yet implemented";
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// 导出所有盒子（Menu: Tools > Data > Dump Boxes）
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteWithSaveFile))]
    private async Task MainMenuBoxDump()
    {
        StatusText = "Dump Boxes not yet implemented";
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// 导出单个盒子（Menu: Tools > Data > Dump Box）
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteWithSaveFile))]
    private async Task MainMenuBoxDumpSingle()
    {
        StatusText = "Dump Box not yet implemented";
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// 盒子数据报告（Menu: Tools > Data > Box Data Report）
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteWithSaveFile))]
    private async Task MainMenuBoxReport()
    {
        StatusText = "Box Data Report not yet implemented";
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// PKM数据库（Menu: Tools > Data > PKM Database）
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteWithSaveFile))]
    private async Task MainMenuDatabase()
    {
        StatusText = "PKM Database not yet implemented";
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// 神秘礼物数据库（Menu: Tools > Data > Mystery Gift Database）
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteWithSaveFile))]
    private async Task MainMenuMysteryDB()
    {
        StatusText = "Mystery Gift Database not yet implemented";
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// 遭遇数据库（Menu: Tools > Data > Encounter Database）
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteWithSaveFile))]
    private async Task Menu_EncDatabase()
    {
        StatusText = "Encounter Database not yet implemented";
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// 批量编辑器（Menu: Tools > Data > Batch Editor）
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteWithSaveFile))]
    private async Task MainMenuBatchEditor()
    {
        StatusText = "Batch Editor not yet implemented";
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// 打开文件夹（Menu: Tools > Open Folder）
    /// </summary>
    [RelayCommand]
    private async Task MainMenuFolder()
    {
        StatusText = "Open Folder not yet implemented";
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// 撤销（Menu: Options > Undo Last Change）
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAlwaysExecute))]
    private async Task ClickUndo()
    {
        StatusText = "Undo not yet implemented";
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// 重做（Menu: Options > Redo Last Change）
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAlwaysExecute))]
    private async Task ClickRedo()
    {
        StatusText = "Redo not yet implemented";
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// 设置（Menu: Options > Settings）
    /// </summary>
    [RelayCommand]
    private async Task MainMenuSettings()
    {
        StatusText = "Settings not yet implemented";
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// 关于（Menu: Options > About PKHeX）
    /// </summary>
    [RelayCommand]
    private async Task MainMenuAbout()
    {
        StatusText = "About not yet implemented";
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// 判断是否可以执行需要存档文件的命令
    /// </summary>
    private bool CanExecuteWithSaveFile() => CurrentSaveFile != null;
    
    /// <summary>
    /// 始终可以执行
    /// </summary>
    private bool CanAlwaysExecute() => true;
    
    /// <summary>
    /// 从存档文件管理器更新视图模型
    /// </summary>
    private void UpdateViewModelFromManager()
    {
        CurrentSaveFile = _saveFileManager.CurrentSaveFile;
        CurrentFilePath = _saveFileManager.CurrentFilePath;
    }
    
    /// <summary>
    /// 存档文件变更处理
    /// </summary>
    private void OnSaveFileChanged(object? sender, SaveFileEventArgs e)
    {
        UpdateViewModelFromManager();
    }
}
