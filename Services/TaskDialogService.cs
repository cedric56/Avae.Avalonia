using Avae.Services;
using Avalonia.Threading;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;

namespace Avae.Avalonia;

public class TaskDialogService : ITaskDialogService
{
    public async Task<TaskDialogStandardResult> ShowAsync(TaskDialogParams @params, params TaskDialogStandardResult[] results)
    {
        if (@params != null)
        {
            return await Dispatcher.UIThread.Invoke(async () =>
            {
                try
                {
                    TypedEventHandler<FATaskDialog, EventArgs>? opening = null;
                    TypedEventHandler<FATaskDialog, EventArgs>? opened = null;
                    TypedEventHandler<FATaskDialog, FATaskDialogClosingEventArgs>? closing = null;
                    TypedEventHandler<FATaskDialog, EventArgs>? closed = null;

                    var taskDialog = new FATaskDialog()
                    {
                        Buttons = CreateDialogButtons(results),
                        Content = @params.Content,
                        Title = @params.Title,
                        Header = @params.Header,
                        SubHeader = @params.SubHeader,
                        IconSource = @params.IconSource as FAIconSource,
                        ShowProgressBar = @params.ShowProgressBar,
                        FooterVisibility = Enum.Parse<FATaskDialogFooterVisibility>(@params.FooterVisibility.ToString()),
                        IsFooterExpanded = @params.IsFooterExpanded,
                        Footer = @params.Footer,
                        XamlRoot = TopLevelStateManager.Default.GetActive(throwOnNull: true)
                    };

                    taskDialog.Opening += opening = (sender, args) => @params.Opening?.Invoke();
                    taskDialog.Opened += opened = (sender, args) => @params.Opening?.Invoke();
                    taskDialog.Closing += closing = (sender, args) => args.Cancel = @params.Closing?.Invoke() ?? false;
                    taskDialog.Closed += closed = (sender, args) =>
                    {
                        @params.Closed?.Invoke();

                        taskDialog.Opened -= opened;
                        taskDialog.Opening -= opening;
                        taskDialog.Closing -= closing;
                        taskDialog.Closed -= closed;
                    };
                    var result = await taskDialog.ShowAsync();
                    return (TaskDialogStandardResult)result;
                }
                catch
                {
                    return TaskDialogStandardResult.None;
                }
            });
        }

        return TaskDialogStandardResult.None;
    }

    private static List<FATaskDialogButton> CreateDialogButtons(TaskDialogStandardResult[] results)
    {
        var buttons = new List<FATaskDialogButton>();
        foreach (var result in results)
        {
            FATaskDialogButton? button = result switch
            {
                TaskDialogStandardResult.OK => FATaskDialogButton.OKButton,
                TaskDialogStandardResult.Retry => FATaskDialogButton.RetryButton,
                TaskDialogStandardResult.Yes => FATaskDialogButton.YesButton,
                TaskDialogStandardResult.No => FATaskDialogButton.NoButton,
                TaskDialogStandardResult.Cancel => FATaskDialogButton.CancelButton,
                TaskDialogStandardResult.Close => FATaskDialogButton.CloseButton,
                _ => null
            };
            if (button != null)
            {
                buttons.Add(button);
            }
        }
        return buttons;
    }
}
