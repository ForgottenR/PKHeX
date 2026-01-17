using CommunityToolkit.Mvvm.ComponentModel;

namespace PKHeX.Avalonia.ViewModels;

/// <summary>
/// 所有 ViewModel 的基类，继承 ObservableObject 提供属性更改通知
/// </summary>
public abstract class ViewModelBase : ObservableObject
{
    // 可以在这里添加所有 ViewModel 共享的逻辑
    protected ViewModelBase()
    {
    }
}
