using Avae.ViewModels;
using Avalonia.Controls;

namespace Avae.Avalonia;

public class ViewFor<TViewModel> : UserControl, IViewFor
{
    public object? Context
    {
        get => DataContext;
        set => DataContext = value;
    }
}
