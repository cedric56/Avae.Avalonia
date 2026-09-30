using Avae.ViewModels;
using Avalonia.Controls;
using MsBox.Avalonia.Base;

namespace Avae.Avalonia
{
    /// <summary>
    /// The content view hosted inside a boxed message-box dialog, bridging the closeable view model's
    /// <see cref="ICloseableViewModel{TResult}.CloseRequested"/> event to the message box's close/result APIs.
    /// </summary>
    /// <typeparam name="T">The closeable view model type driving this dialog.</typeparam>
    /// <typeparam name="TResult">The result type produced when the dialog is closed.</typeparam>
    internal partial class ModalView<T, TResult> : UserControl, IFullApi<TResult?>, ISetCloseAction
        where T : ICloseableViewModel<TResult?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModalView{T, TResult}"/> class, binding
        /// <paramref name="viewModel"/> as the data context and wiring the underlying view model's
        /// close request to record the result and close the hosting window or message box.
        /// </summary>
        /// <param name="viewModel">The modal view model wrapping the closeable view model for this dialog.</param>
        public ModalView(ModalViewModel<T, TResult?> viewModel)
        {
            DataContext = viewModel;
            EventHandler<TResult>? closeRequested = null!;
            viewModel.ViewModel.CloseRequested += closeRequested = (sender, e) =>
            {
                viewModel.ViewModel.CloseRequested -= closeRequested;
                SetButtonResult(e);

                if (this.Parent is Window window)
                    window.Close();
                else
                    Close();
            };
        }

        /// <inheritdoc/>
        protected override Type StyleKeyOverride => typeof(DialogViewBase);

        /// <summary>
        /// The result value recorded when the view model requests a close, returned by <see cref="GetButtonResult"/>.
        /// </summary>
        private TResult? _buttonResult = default;

        /// <summary>
        /// The action invoked by <see cref="Close"/> to actually close the hosting message box, set via <see cref="SetCloseAction"/>.
        /// </summary>
        private Action _closeAction = () => { };

        /// <summary>
        /// Records the result to be returned to the caller of the dialog.
        /// </summary>
        /// <param name="bdName">The result value to record.</param>
        public void SetButtonResult(TResult? bdName)
        {
            _buttonResult = bdName;
        }

        /// <summary>
        /// Gets the result recorded via <see cref="SetButtonResult(TResult?)"/>.
        /// </summary>
        /// <returns>The recorded result value.</returns>
        public TResult? GetButtonResult()
        {
            return _buttonResult;
        }

        /// <summary>
        /// Copies the dialog's content to the clipboard. Currently a no-op; implementation is commented out.
        /// </summary>
        /// <returns>A completed task.</returns>
        public Task Copy()
        {
            //var text = (DataContext as AbstractMsBoxViewModel)?.ContentMessage;
            //TopLevel.GetTopLevel(this)?.Clipboard..SetTextAsync(text);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Closes the hosting message box by invoking the action registered via <see cref="SetCloseAction"/>.
        /// </summary>
        public void Close()
        {
            _closeAction?.Invoke();
        }

        /// <summary>
        /// Event handler that closes the dialog, for wiring to UI close-triggering events.
        /// </summary>
        /// <param name="sender">The event source.</param>
        /// <param name="eventArgs">The event arguments.</param>
        public void CloseWindow(object sender, EventArgs eventArgs)
        {
            ((IClose)this).Close();
        }

        /// <summary>
        /// Registers the action used to close the hosting message box.
        /// </summary>
        /// <param name="closeAction">The action to invoke when <see cref="Close"/> is called.</param>
        public void SetCloseAction(Action closeAction)
        {
            _closeAction = closeAction;
        }
    }
}