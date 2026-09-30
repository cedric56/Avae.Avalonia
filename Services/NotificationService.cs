using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;

namespace Avae.Avalonia;

internal class NotificationService : Avae.Services.INotificationService
{
    private WindowNotificationManager? _manager;

    public int MaxItems { get => 5; }
    public Avae.Services.NotificationPosition Position => Avae.Services.NotificationPosition.TopCenter;

    public void Show(string title, string message, Avae.Services.NotificationType type = default,
                     TimeSpan? expiration = null, Action? onClick = null, Action? onClose = null)
    {
        var topLevel = TopLevelStateManager.Default.GetActive();
        if (topLevel == null)
            return;

        // Always check and update manager
        if (_manager == null || TopLevel.GetTopLevel(_manager) != topLevel)
        {
            _manager?.TemplateApplied -= Ready;
            _manager = new WindowNotificationManager(topLevel);
            _manager.TemplateApplied += Ready;
        }
        else
        {
            Display();
        }

        void Ready(object? sender, TemplateAppliedEventArgs e)
        {
            _manager.TemplateApplied -= Ready;
            Display();
        }

        void Display()
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                var notification = new Notification(title, message, (NotificationType)type,
                                                   expiration, onClick, onClose);
                _manager.Show(notification);
            });
        }
    }
}
