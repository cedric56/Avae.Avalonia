using Avae.ViewModels;
using Avalonia.Controls;
using Avalonia.Threading;

namespace Avae.Avalonia;

public class ViewFor<TViewModel> : UserControl, IViewFor
{
    public object? Context
    {
        get => Dispatcher.Invoke(() => DataContext);
        set => Dispatcher.Invoke(() => DataContext = value);
    }
}
