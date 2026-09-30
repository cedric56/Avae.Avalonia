using Avae.Services;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;

namespace Avae.Avalonia;

public class DialogService(string iconUrl) : IDialogService
{
    public string IconUrl { get; private set; } = iconUrl;

    public async Task<int> ShowMessage(string message, string title, string buttonText)
    {
        var buttons = buttonText.ToEnum<ButtonEnum>();
        var splits = buttonText.SplitOnCapitals();
        var @params = GetParams(title, message, buttons);
        var result = await ShowMessage(@params);
        var possibilities = splits.Select(s => s.ToEnum<ButtonResult>()).ToList();
        var index = possibilities.IndexOf(result);
        return index;
    }

    private static Task<ButtonResult> ShowMessage(MessageBoxStandardParams @params)
    {
        return Dispatcher.UIThread.Invoke(async () =>
        {
            var box = MessageBoxManager.GetMessageBoxStandard(@params);
            if (TopLevelStateManager.Default.GetActive() is Window owner)
            {
                return await box.ShowWindowDialogAsync(owner);
            }

            //var top = CurrentDialogHost;
            //var result = top != null ? await box.ShowAsPopupAsync(top) : await box.ShowAsync();
            var result = await box.ShowAsync();
            return result;
        });
    }

    public static ContentControl? CurrentDialogHost
    {
        get
        {
            TopLevel? topLevel = TopLevelStateManager.Default.GetActive();
            var fluent = topLevel?.GetVisualDescendants().OfType<FluentAvalonia.UI.Controls.FADialogHost>().LastOrDefault();
            if (fluent != null) return fluent;
            return null;
        }
    }

    private MessageBoxStandardParams GetParams(string title, string message, ButtonEnum buttonEnum)
    {
        var @params = new MessageBoxStandardParams
        {
            Topmost = true,
            ButtonDefinitions = buttonEnum,
            ContentTitle = title,
            ContentMessage = message,
            MinWidth = 300,
            ShowInCenter = true,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,

        };
        if (!string.IsNullOrWhiteSpace(IconUrl))
            @params.WindowIcon = GetIcon(IconUrl)!;
        return @params;
    }

    public static WindowIcon? GetIcon(string url)
    {
        try
        {
            return new WindowIcon(AssetLoader.Open(new Uri(url)));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }

    public Task ShowErrorAsync(Exception ex, string title = "Error")
    {
        return ShowMessage(ex.Message, title, "Ok");
    }

    public Task ShowOkAsync(string message, string title = "Title")
    {
        return ShowMessage(message, title, "Ok");
    }

    public async Task<bool> ShowYesNoAsync(string message, string title = "Title")
    {
        return await ShowMessage(message, title, "YesNo") == 0;
    }

    public async Task<bool> ShowOkCancelAsync(string message, string title = "Title")
    {
        return await ShowMessage(message, title, "OkCancel") == 0;
    }

    public async Task<bool> ShowOkAbortAsync(string message, string title = "Title")
    {
        return await ShowMessage(message, title, "OkAbort") == 0;
    }

    public Task<int> ShowYesNoCancelAsync(string message, string title = "Title")
    {
        return ShowMessage(message, title, "YesNoCancel");
    }

    public Task<int> ShowYesNoAbortAsync(string message, string title = "Title")
    {
        return ShowMessage(message, title, "YesNoAbort");
    }
}
