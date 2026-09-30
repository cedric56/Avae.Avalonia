using Avae.Services;
using Avalonia.Threading;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using ContentDialogResult = Avae.Services.ContentDialogResult;

namespace Avae.Avalonia;

public class ContentDialogService : IContentDialogService, IDialogService
{
    public async Task<ContentDialogResult> ShowAsync(ContentDialogParams @params)
    {
        return await Dispatcher.UIThread.Invoke(async () =>
        {
            var topLevel = TopLevelStateManager.Default.GetActive(throwOnNull: true);
            var dialog = GetContentDialog(@params);
            var result = await dialog.ShowAsync(topLevel);
            return Enum.TryParse(result.ToString(), out ContentDialogResult dialogResult)
                ? dialogResult
                : ContentDialogResult.None;
        });
    }

    private static FAContentDialog GetContentDialog(ContentDialogParams @params)
    {
        var dialog = new FAContentDialog();
        if (@params != null)
        {
            TypedEventHandler<FAContentDialog, FAContentDialogClosingEventArgs>? closing = null;
            TypedEventHandler<FAContentDialog, EventArgs>? opening = null;
            TypedEventHandler<FAContentDialog, EventArgs>? opened = null;
            TypedEventHandler<FAContentDialog, FAContentDialogClosedEventArgs>? closed = null;
            TypedEventHandler<FAContentDialog, FAContentDialogButtonClickEventArgs>? primaryButtonClick = null;
            TypedEventHandler<FAContentDialog, FAContentDialogButtonClickEventArgs>? secondaryButtonClick = null;
            TypedEventHandler<FAContentDialog, FAContentDialogButtonClickEventArgs>? closeButtonClick = null;

            dialog.Content = @params.Content;
            //IsPrimaryButtonEnabled = @params.IsPrimaryButtonEnabled,
            //IsSecondaryButtonEnabled = @params.IsSecondaryButtonEnabled,
            dialog.PrimaryButtonText = @params.PrimaryButtonText;
            dialog.SecondaryButtonText = @params.SecondaryButtonText;
            dialog.CloseButtonText = @params.CloseButtonText;
            dialog.Title = @params.Title;
            dialog.CloseButtonCommand = @params.CloseButtonCommand;
            dialog.CloseButtonCommandParameter = @params.CloseButtonCommandParameter;
            dialog.FullSizeDesired = @params.FullSizeDesired;
            dialog.PrimaryButtonCommand = @params.PrimaryButtonCommand;
            dialog.PrimaryButtonCommandParameter = @params.PrimaryButtonCommandParameter;
            dialog.SecondaryButtonCommand = @params.SecondaryButtonCommand;
            dialog.SecondaryButtonCommandParameter = @params.SecondaryButtonCommandParameter;
            dialog.Opening += opening = (sender, args) => @params.Opening?.Invoke();
            dialog.Closing += closing = (sender, args) => args.Cancel = @params.Closing?.Invoke(args.Result.ToString()) ?? false;
            dialog.PrimaryButtonClick += primaryButtonClick = (sender, args) => @params.PrimaryButtonClick?.Invoke();
            dialog.SecondaryButtonClick += secondaryButtonClick = (sender, args) => @params.SecondaryButtonClick?.Invoke();
            dialog.CloseButtonClick += closeButtonClick = (sender, args) => @params.CloseButtonClick?.Invoke();
            dialog.Opened += opened = (sender, args) =>
            {
                @params.Opened?.Invoke();
            };
            dialog.Closed += closed = (sender, args) =>
            {
                @params.Closed?.Invoke();
                dialog.Opened -= opened;
                dialog.Opening -= opening;
                dialog.Closing -= closing;
                dialog.PrimaryButtonClick -= primaryButtonClick;
                dialog.SecondaryButtonClick -= secondaryButtonClick;
                dialog.CloseButtonClick -= closeButtonClick;
                dialog.Closed -= closed;
            };
        }

        return dialog;
    }

    public Task ShowErrorAsync(Exception ex, string title = "Error")
    {
        return ShowAsync(new ContentDialogParams()
        {
            CloseButtonText = "Ok",
            Title = title,
            Content = ex.Message
        });
    }

    public Task ShowOkAsync(string message, string title = "Title")
    {
        return ShowAsync(new ContentDialogParams()
        {
            CloseButtonText = "Ok",
            Title = title,
            Content = message
        });
    }

    public async Task<bool> ShowYesNoAsync(string message, string title = "Title")
    {
        return await ShowAsync(new ContentDialogParams()
        {
            CloseButtonText = "No",
            PrimaryButtonText = "Yes",
            Title = title,
            Content = message

        }) == ContentDialogResult.Primary;
    }

    public async Task<bool> ShowOkCancelAsync(string message, string title = "Title")
    {
        return await ShowAsync(new ContentDialogParams()
        {
            CloseButtonText = "Cancel",
            PrimaryButtonText = "Ok",
            Title = title,
            Content = message

        }) == ContentDialogResult.Primary;
    }

    public async Task<bool> ShowOkAbortAsync(string message, string title = "Title")
    {
        return await ShowAsync(new ContentDialogParams()
        {
            CloseButtonText = "Abort",
            PrimaryButtonText = "Ok",
            Title = title,
            Content = message

        }) == ContentDialogResult.Primary;
    }

    public async Task<int> ShowYesNoCancelAsync(string message, string title = "Title")
    {
        var result = await ShowAsync(new ContentDialogParams()
        {
            CloseButtonText = "Cancel",
            SecondaryButtonText = "No",
            PrimaryButtonText = "Yes",
            Title = title,
            Content = message

        });

        return result switch
        {
            ContentDialogResult.Primary => 0,
            ContentDialogResult.Secondary => 1,
            _ => 2
        };
    }

    public async Task<int> ShowYesNoAbortAsync(string message, string title = "Title")
    {
        var result = await ShowAsync(new ContentDialogParams()
        {
            CloseButtonText = "Abort",
            SecondaryButtonText = "No",
            PrimaryButtonText = "Yes",
            Title = title,
            Content = message

        });

        return result switch
        {
            ContentDialogResult.Primary => 0,
            ContentDialogResult.Secondary => 1,
            _ => 2
        };
    }
}
