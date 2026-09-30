using Avae.ViewModels;
using Avalonia.Controls;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.ViewModels;

namespace Avae.Avalonia;

/// <summary>
/// Non-generic base for a boxed modal dialog's view model, exposing the button definitions,
/// original parameters, and content control needed to render the dialog.
/// </summary>
/// <param name="parameters">The modal parameters this view model is built from.</param>
internal abstract class ModalViewModelBase(ModalParameters parameters) :
    MsBoxCustomViewModel(parameters)
{
    /// <summary>
    /// Gets the configured button definitions, cast to <see cref="ModalButton"/>.
    /// </summary>
    public IEnumerable<ModalButton> Definitions
    {
        get
        {
            return ButtonDefinitions.Cast<ModalButton>();
        }
    }

    /// <summary>
    /// Gets the original modal parameters this view model was constructed from.
    /// </summary>
    protected ModalParameters Parameters { get; } = parameters;

    /// <summary>
    /// Gets the underlying view model as an untyped <see cref="object"/>. Always <see langword="null"/>
    /// on this base class; the strongly typed value is exposed by <see cref="ModalViewModel{T, TResult}.ViewModel"/>.
    /// </summary>
    public object? ViewModel { get; }

    /// <summary>
    /// Gets the control displayed as the dialog's content, taken from <see cref="Parameters"/>.
    /// </summary>
    public UserControl? Content { get; } = parameters.Content;
}

/// <summary>
/// Strongly typed boxed modal dialog view model, pairing the underlying closeable view model
/// with the dialog parameters built from it.
/// </summary>
/// <typeparam name="T">The closeable view model type driving the dialog.</typeparam>
/// <typeparam name="TResult">The result type produced when the dialog is closed.</typeparam>
/// <param name="parameters">The modal parameters this view model is built from.</param>
/// <param name="viewModel">The closeable view model backing the dialog.</param>
internal class ModalViewModel<T, TResult>(ModalParameters<T, TResult> parameters, T viewModel) :
    ModalViewModelBase(parameters), ISetFullApi<TResult>
    where T : ICloseableViewModel<TResult>
{
    /// <summary>
    /// Gets the strongly typed closeable view model backing this dialog, shadowing the base
    /// class's untyped <see cref="ModalViewModelBase.ViewModel"/>.
    /// </summary>
    public new T ViewModel { get; } = viewModel;

    /// <summary>
    /// Receives the message box's full API surface. Currently a no-op.
    /// </summary>
    /// <param name="fullApi">The full API instance supplied by the message box host.</param>
    public void SetFullApi(IFullApi<TResult> fullApi)
    {

    }
}