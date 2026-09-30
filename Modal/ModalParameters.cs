using Avae.Services;
using Avae.ViewModels;
using Avalonia.Controls;
using Avalonia.Platform;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Models;
using System.Windows.Input;

namespace Avae.Avalonia;

/// <summary>
/// A message-box button definition that also carries the <see cref="ICommand"/> to execute when
/// the button is pressed, sourced from a view model's <see cref="ICloseableViewModel{TResult}.Commands"/>.
/// </summary>
internal class ModalButton : ButtonDefinition
{
    /// <summary>
    /// Gets or sets the command executed when this button is pressed.
    /// </summary>
    public ICommand? Command { get; set; }
}

/// <summary>
/// Non-generic base for boxed modal dialog parameters, exposing the button definitions as
/// strongly typed <see cref="ModalButton"/>s and the content to display.
/// </summary>
internal abstract class ModalParameters : MessageBoxCustomParams
{
    /// <summary>
    /// Gets the configured button definitions, cast to <see cref="ModalButton"/>.
    /// </summary>
    public IEnumerable<ModalButton> Definitions
    {
        get { return ButtonDefinitions.Cast<ModalButton>(); }
    }

    /// <summary>
    /// Gets or sets the control displayed as the dialog's content.
    /// </summary>
    public UserControl? Content { get; set; }
}

/// <summary>
/// Boxed modal dialog parameters built from a closeable view model's commands: the first command
/// becomes the default (Enter-triggered) button, the last becomes the cancel (Escape-triggered) button,
/// and standard dialog appearance/behavior settings are applied.
/// </summary>
/// <typeparam name="T">The closeable view model type driving the dialog.</typeparam>
/// <typeparam name="TResult">The result type produced when the dialog is closed.</typeparam>
internal class ModalParameters<T, TResult> : ModalParameters
    where T : ICloseableViewModel<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModalParameters{T, TResult}"/> class, building
    /// button definitions from <paramref name="viewModel"/>'s commands and configuring standard
    /// dialog appearance and behavior.
    /// </summary>
    /// <param name="icon">
    /// The URI of an asset to use as the window icon, resolved via <see cref="AssetLoader"/>.
    /// Ignored if empty/whitespace or if the asset cannot be found.
    /// </param>
    /// <param name="viewModel">The view model whose <see cref="ICloseableViewModel{TResult}.Commands"/> become the dialog's buttons.</param>
    public ModalParameters(string icon, T viewModel)
    {
        var definitions = new List<ButtonDefinition>();
        foreach (var command in viewModel.Commands)
        {
            var bd = new ModalButton
            {
                Command = command.Command,
                Name = command.Name,
                IsDefault = viewModel.Commands.IndexOf(command) == 0,
                IsCancel = viewModel.Commands.IndexOf(command) == viewModel.Commands.Count - 1,
            };
            definitions.Add(bd);
        }
        ButtonDefinitions = definitions;
        CloseOnClickAway = true;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        CanResize = false;
        MaxWidth = 500;
        MaxHeight = 800;
        SizeToContent = SizeToContent.WidthAndHeight;
        ShowInCenter = true;
        Topmost = true;

        if (!string.IsNullOrWhiteSpace(icon))
        {
            var uri = new Uri(icon);
            if (AssetLoader.Exists(uri))
                WindowIcon = new WindowIcon(AssetLoader.Open(uri));
        }
    }
}