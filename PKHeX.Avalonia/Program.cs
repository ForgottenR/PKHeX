using Avalonia;
using System;

namespace PKHeX.Avalonia;

class Program
{
    // 应用程序入口点，初始化 AvaloniaUI 和应用程序
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Application crashed: {ex}");
            throw;
        }
    }

    // Avalonia 配置，使用 App.xaml 配置
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseSkia();
}
