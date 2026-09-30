using Avae.Services;
using Avalonia;
using Avalonia.Styling;

namespace Avae.Avalonia;

public class RequestThemeService : IRequestedThemeService
{
    /// <summary>
    /// Applies the specified theme to the application by setting <see cref="Application.RequestedThemeVariant"/>.
    /// </summary>
    /// <param name="theme">The requested theme.</param>
    public void Request(RequestedTheme theme)
    {
        Application.Current?.RequestedThemeVariant = theme switch
        {
            RequestedTheme.Light => ThemeVariant.Light,
            RequestedTheme.Dark => ThemeVariant.Dark,
            _ => ThemeVariant.Default,
        };
    }
}
