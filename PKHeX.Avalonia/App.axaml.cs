using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using PKHeX.Avalonia.Services;
using PKHeX.Avalonia.ViewModels;
using PKHeX.Avalonia.Views;

namespace PKHeX.Avalonia;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Setup dependency injection
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
            
            // Create main window with injected services
            var mainWindow = new MainWindow();
            var viewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();
            mainWindow.DataContext = viewModel;
            
            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    private static void ConfigureServices(IServiceCollection services)
    {
        // Register services
        services.AddSingleton<ISaveFileManager, SaveFileManager>();
        
        // Register window-specific services (will be resolved per window)
        services.AddTransient<IFileDialogService>(provider => 
            new FileDialogService(provider.GetRequiredService<MainWindow>()));
        
        // Register ViewModels
        services.AddTransient<MainWindowViewModel>();
        
        // Register Views
        services.AddTransient<MainWindow>();
    }
}