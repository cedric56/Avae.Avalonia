using Avae.ViewModels;
using Avalonia.Controls;
using Avalonia.Threading;

namespace Avae.Avalonia;

public class ViewFor<TViewModel> : UserControl, IViewFor
{
    public object? Context
    {
        get => DataContext;
        set
        {
            if (Dispatcher.CheckAccess())
                DataContext = value;
            else
                Dispatcher.Invoke(() => DataContext = value);
        }
    }
}
