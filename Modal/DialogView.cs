using Avae.Services;
using Avae.ViewModels;
using Avalonia.Controls;
using MsBox.Avalonia;

namespace Avae.Avalonia;

/// <summary>
/// Non-generic base class for dialog views, allowing dialog controls to be referenced or
/// grouped without knowing their view model or result types.
/// </summary>
public abstract class DialogViewBase : UserControl
{

}

/// <summary>
/// Base view for a modal dialog backed by a closeable view model, supporting both FluentAvalonia
/// content dialogs and boxed message-box-style dialogs depending on <see cref="TypeDialog"/>.
/// </summary>
/// <typeparam name="TViewModel">The closeable view model type driving this dialog.</typeparam>
/// <typeparam name="TResult">The result type produced when the dialog is closed.</typeparam>
public class DialogView<TViewModel, TResult> : DialogViewBase,
    IModalFor<TViewModel, TResult?>
    where TViewModel : class, ICloseableViewModel<TResult?>
{
    static ContentDialogService service;
    static DialogView()
    {
        service = new ContentDialogService();
    }

    /// <summary>
    /// Gets or sets the dialog's data context, exposed as an untyped <see cref="object"/> for <see cref="IModalFor{TViewModel, TResult}"/>.
    /// </summary>
    public object? Context { get => DataContext; set => DataContext = value; }

    /// <summary>
    /// Gets the icon identifier used for the boxed dialog implementation. Empty by default.
    /// </summary>
    protected virtual string Icon { get; } = "";

    /// <summary>
    /// Gets the strongly typed view model bound to this dialog's <see cref="DataContext"/>, or <see langword="null"/> if not set or of the wrong type.
    /// </summary>
    protected TViewModel? ViewModel { get { return DataContext as TViewModel; } }

    /// <summary>
    /// Gets which dialog implementation this view uses. Defaults to <see cref="TypeDialog.Box"/>.
    /// </summary>
    protected virtual bool IsFluent { get; } = false;

    /// <summary>
    /// Builds the FluentAvalonia content dialog parameters for this dialog, wiring up primary,
    /// secondary, and (if more than two commands are present) close button text and commands from
    /// the view model's <see cref="ICloseableViewModel{TResult}.Commands"/>, and preventing the
    /// dialog from closing if the corresponding command reports it cannot execute.
    /// </summary>
    /// <param name="parameters">The modal parameters supplying the button command definitions.</param>
    /// <returns>The constructed <see cref="ContentDialogParams"/>.</returns>
    private ContentDialogParams CreateContentDialogParams(ModalParameters<TViewModel, TResult?> parameters)
    {
        ContentDialogParams? @params = null;

        @params = new ContentDialogParams
        {
            Content = this,
            Title = ViewModel?.Title,
            PrimaryButtonText = parameters.Definitions.ElementAt(0).Name,
            PrimaryButtonCommand = parameters.Definitions.ElementAt(0).Command,
            SecondaryButtonText = parameters.Definitions.ElementAtOrDefault(1)?.Name,
            SecondaryButtonCommand = parameters.Definitions.ElementAtOrDefault(1)?.Command,
            Closing = result =>
            {
                bool value = true;
                if (result == "Primary")
                    value = @params!.PrimaryButtonCommand?.CanExecute(@params.PrimaryButtonCommandParameter) ?? true;
                else if (result == "Secondary")
                    value = @params!.SecondaryButtonCommand?.CanExecute(@params.SecondaryButtonCommandParameter) ?? true;
                else
                    value = @params!.CloseButtonCommand?.CanExecute(@params.CloseButtonCommandParameter) ?? true;

                return !value;
            }
        };

        if (ViewModel?.Commands.Count > 2)
        {
            @params.CloseButtonText = parameters.Definitions.LastOrDefault()?.Name;
            @params.CloseButtonCommand = parameters.Definitions.LastOrDefault()?.Command;
        }

        return @params;
    }

    /// <summary>
    /// Shows this dialog modally—using a FluentAvalonia content dialog or a boxed message box
    /// depending on <see cref="TypeDialog"/>—and awaits its result.
    /// </summary>
    /// <returns>
    /// The result produced by the view model when the dialog closes, after being passed through
    /// <see cref="OnValidate(TResult?)"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <see cref="ViewModel"/> is <see langword="null"/> (i.e. <see cref="Context"/> was not set to a <typeparamref name="TViewModel"/> instance).</exception>
    public async Task<TResult?> ShowModalAsync()
    {
        TResult? result = default;
        var viewModel = ViewModel;
        if (viewModel is null)
            throw new ArgumentNullException(nameof(viewModel));

        var modalParams = new ModalParameters<TViewModel, TResult?>(Icon, viewModel)
        {
            Content = this,
            ContentTitle = viewModel.Title,
            CloseOnClickAway = true
        };

        if (IsFluent)
        {
            var contentDialogParams = CreateContentDialogParams(modalParams);
            EventHandler<TResult>? closeRequested = null!;
            viewModel.CloseRequested += closeRequested = (sender, e) =>
            {
                viewModel.CloseRequested -= closeRequested;
                result = e;
            };            
            await service.ShowAsync(contentDialogParams);
        }
        else
        {
            var modalViewModel = new ModalViewModel<TViewModel, TResult?>(modalParams, viewModel);
            var modalView = new ModalView<TViewModel, TResult?>(modalViewModel);
            var box = new MsBox<ModalView<TViewModel, TResult?>, ModalViewModel<TViewModel, TResult?>, TResult?>(modalView, modalViewModel);
            if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                result = await box.ShowWindowDialogAsync((Window)TopLevelStateManager.Default.GetActive()!);
            else
                result = await box.ShowAsync();
        }

        await OnValidate(result);
        return result;
    }

    /// <summary>
    /// Override to perform custom validation logic if needed.
    /// </summary>
    protected virtual Task OnValidate(TResult? result)
    {
        return Task.CompletedTask;
    }
}