# Avae.Avalonia

A small Avalonia UI support library for applications built around the Avae services and view-model packages.

It provides reusable application bootstrapping, dialogs, notifications, theme switching, modal views, top-level window tracking, and a few Avalonia behaviors.

## Features

- **Application bootstrap** — `AvaeBuilder.CreateAvaloniaApp<TApp>()` wires the Avae services into an Avalonia `AppBuilder`.
- **Dialogs** — standard message boxes, FluentAvalonia content dialogs, and task dialogs.
- **Modal views** — generic `DialogView<TViewModel, TResult>` support for closeable view models.
- **Notifications** — Avalonia `WindowNotificationManager` integration.
- **Theme switching** — `RequestThemeService` maps Avae requested themes to Avalonia theme variants.
- **Top-level tracking** — `TopLevelStateManager` tracks the currently active Avalonia top level.
- **Behaviors** — attached behaviors such as double-click command execution.
- **Null conversion** — `NullConverter` for bindings that need a null/non-null boolean.

## Requirements

The project currently targets **.NET 11** and uses **Avalonia 12.1.2**. The package also depends on:

- Avae.Services 1.0.0-preview.1
- Avae.ViewModels 1.0.0-preview.1
- FluentAvaloniaUI 3.1.0
- MessageBox.Avalonia 12.0.0

.NET 11 is currently a preview-era target; use an SDK that supports `net11.0` when building this revision.

## Installation

From a consuming application:

```bash
dotnet add package Avae.Avalonia
```

If you are working from source:

```bash
git clone https://github.com/cedric56/Avae.Avalonia.git
cd Avae.Avalonia
dotnet restore
dotnet build
```

## Basic setup

The library is designed to be used from an Avalonia application's startup code:

```csharp
using Avae.Avalonia;

var appBuilder = AvaeBuilder.CreateAvaloniaApp<MyApp>(
    runtime: runtime,
    icon: "avares://MyApp/Assets/app.ico",
    isFluent: true,
    appFactory: services => new MyApp());

return appBuilder
    .UsePlatformDetect()
    .LogToTrace()
    .StartWithClassicDesktopLifetime(args);
```

Register application-specific services with the optional service callback:

```csharp
var appBuilder = AvaeBuilder.CreateAvaloniaApp<MyApp>(
    runtime,
    "avares://MyApp/Assets/app.ico",
    isFluent: true,
    appFactory: services => new MyApp(),
    configureExternalServices: services =>
    {
        services.AddSingleton<IMyService, MyService>();
    });
```

> The exact startup shape depends on the Avae.Services and Avae.ViewModels versions used by your application.

## Dialogs

The package exposes implementations of the Avae dialog interfaces through dependency injection. Once registered by `AvaeBuilder`, application services can request the appropriate dialog service.

For example:

```csharp
public sealed class MyViewModel
{
    private readonly IDialogService _dialogs;

    public MyViewModel(IDialogService dialogs)
    {
        _dialogs = dialogs;
    }

    public Task<bool> ConfirmAsync()
        => _dialogs.ShowYesNoAsync(
            "Do you want to continue?",
            "Confirm");
}
```

For FluentAvalonia applications, `IContentDialogService` and `ITaskDialogService` are also registered.

## Notifications

The registered notification service uses the active Avalonia top level:

```csharp
notificationService.Show(
    "Saved",
    "Your changes were saved.");
```

Notifications are ignored when no active top level can be detected.

## Theme switching

`RequestThemeService` maps:

- `RequestedTheme.Light` → `ThemeVariant.Light`
- `RequestedTheme.Dark` → `ThemeVariant.Dark`
- other values → `ThemeVariant.Default`

## Repository layout

| Path | Purpose |
| --- | --- |
| `AvaeBuilder.cs` | Avalonia application/service bootstrap |
| `TopLevelStateManager.cs` | Active Avalonia top-level tracking |
| `Services/` | Dialog, notification, task-dialog, and theme services |
| `Modal/` | Modal dialog views, view models, parameters, and styles |
| `Behaviors/` | Avalonia attached behaviors |
| `Themes/` | Reusable Avalonia styles |
| `ViewFor.cs` | Generic view implementation for Avae view models |
| `NullConverter.cs` | Null-check value converter |

## Development

The repository uses central package version management via `Directory.Packages.props`.

Build from the solution:

```bash
dotnet restore Avae.Avalonia.slnx
dotnet build Avae.Avalonia.slnx
```

There are currently no test projects in the repository, so behavioral regressions should be covered with tests as the library evolves.

## Known issues

See the repository's GitHub issues for the current bug audit. In particular, the active top-level tracking and double-click command behavior need attention before relying on them in multi-window applications.

## License

No license file is currently present in the repository. Add an explicit license before distributing the project as a reusable package.
