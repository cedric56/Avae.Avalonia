using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace Avae.Avalonia;

/// <summary>
/// Manager object that manages top-level states on Windows.
/// </summary>
public interface ITopLevelStateManager
{
    /// <summary>
    /// Occurs when the application's active top-level changed.
    /// </summary>
    event EventHandler ActiveChanged;

    /// <summary>
    /// Gets the application's currently active top-level.
    /// </summary>
    /// <returns>The application's currently active <see cref="TopLevel"/> object.</returns>
    TopLevel? GetActive();

    /// <summary>
    /// Sets the new active top-level that can be retrieved with <see cref="GetActive"/>.
    /// </summary>
    /// <param name="topLevel">The <see cref="TopLevel"/> object that is activated.</param>
    void OnActivated(TopLevel topLevel);
}

/// <summary>
/// Manager object that manages top-level states on Windows.
/// </summary>
public static class TopLevelStateManager
{
    static ITopLevelStateManager? defaultImplementation;

    /// <summary>
    /// Provides the default implementation for static usage of this API.
    /// </summary>
    public static ITopLevelStateManager Default =>
        defaultImplementation ??= new TopLevelStateManagerImplementation();

    internal static void SetDefault(ITopLevelStateManager? implementation) =>
        defaultImplementation = implementation;

    internal static void Initialize()
    {
        SetDefault(new TopLevelStateManagerImplementation());
        TopLevel.LoadedEvent.AddClassHandler(typeof(TopLevel), (sender, args) =>
        {
            Default.OnActivated((TopLevel)sender!);
        });
        TopLevel.GotFocusEvent.AddClassHandler(typeof(TopLevel), (sender, args) =>
        {
            Default.OnActivated((TopLevel)sender!);
        });
    }
}

static class TopLevelStateManagerExtensions
{
    /// <summary>
    /// Gets the application's currently active top-level.
    /// </summary>
    /// <param name="manager">The object to invoke this method on.</param>
    /// <param name="throwOnNull">Throws an exception if no current <see cref="TopLevel"/> can be found and this value is set to <see langword="true"/>, otherwise this method returns <see langword="null"/>.</param>
    /// <returns>The application's currently active <see cref="TopLevel"/> object.</returns>
    /// <exception cref="NullReferenceException">Thrown if no current <see cref="TopLevel"/> can be found and <paramref name="throwOnNull"/> is set to <see langword="true"/>.</exception>
    public static TopLevel? GetActive(this ITopLevelStateManager manager, bool throwOnNull)
    {
        var topLevel = manager.GetActive();
        if (throwOnNull && topLevel == null)
            throw new NullReferenceException("The active TopLevel cannot be detected. Ensure that you have called Init in your Application class.");

        return topLevel;
    }

    /// <summary>
    /// Gets the application's currently active top-level's pointer.
    /// </summary>
    /// <param name="manager">The object to invoke this method on.</param>
    /// <param name="throwOnNull">Throws an exception if no current <see cref="TopLevel"/> can be found and this value is set to <see langword="true"/>, otherwise this method returns <see cref="IntPtr.Zero"/>.</param>
    /// <returns>The application's currently active top-level's <see cref="IntPtr"/>.</returns>
    /// <exception cref="NullReferenceException">Thrown if no current <see cref="TopLevel"/> can be found and <paramref name="throwOnNull"/> is set to <see langword="true"/>.</exception>
    public static IntPtr GetActiveHandle(this ITopLevelStateManager manager, bool throwOnNull)
    {
        return manager.GetActive(throwOnNull)?.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
    }
}

class TopLevelStateManagerImplementation : ITopLevelStateManager
{
    TopLevel? _active;

    public event EventHandler? ActiveChanged;

    public void OnActivated(TopLevel topLevel)
    {
        if (_active == topLevel)
            return;

        _active = topLevel;

        ActiveChanged?.Invoke(topLevel, EventArgs.Empty);
    }

    public TopLevel? GetActive()
    {
        var lifetime = Application.Current?.ApplicationLifetime;

        var active = lifetime switch
        {
            IClassicDesktopStyleApplicationLifetime desktop => _active ?? TopLevel.GetTopLevel(desktop.MainWindow),
            ISingleViewApplicationLifetime singleView => _active ?? TopLevel.GetTopLevel(singleView.MainView),
            _ => _active ?? TopLevel.GetTopLevel(null)
        };

        return active ?? TopLevel.GetTopLevel(null);
    }
}
