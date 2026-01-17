using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PKHeX.Avalonia.ViewModels;

namespace PKHeX.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    // 可以在这里添加窗口事件处理
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        
        // 窗口加载完成后的初始化
        if (DataContext is MainWindowViewModel vm)
        {
            vm.StatusText = "PKHeX AvaloniaUI Ready";
        }
    }
}
