namespace Template.MobileApp.Components;

using Android.App;
using Android.Content;

using AndroidX.Core.App;

using AndroidNotificationManager = Android.App.NotificationManager;

// スケジュールした通知を発火するレシーバ (アプリのプロセスが無くても起動される)
[BroadcastReceiver(Name = "template.mobileapp.NotificationAlarmReceiver", Exported = false)]
public sealed class NotificationAlarmReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if ((context is null) || (intent is null))
        {
            return;
        }

        NotificationService.ShowScheduled(context, intent);
    }
}

#pragma warning disable CA1822
public sealed partial class NotificationService
{
    private const string ExtraId = "notification_id";

    private const string ExtraAction = "notification_action";

    private const string ExtraPayload = "notification_payload";

    private const string ExtraTitle = "notification_title";

    private const string ExtraMessage = "notification_message";

    public partial bool CanScheduleExact
    {
        get
        {
            if (!OperatingSystem.IsAndroidVersionAtLeast(31))
            {
                return true;
            }

            return ResolveAlarmManager()?.CanScheduleExactAlarms() ?? false;
        }
    }

    // MainActivity の起動 / 再起動 Intent に通知タップの情報があれば取り出す
    public static void HandleIntent(Intent? intent)
    {
        if ((intent is null) || !intent.HasExtra(ExtraId))
        {
            return;
        }

        var args = new NotificationTappedEventArgs(intent.GetIntExtra(ExtraId, 0), intent.GetStringExtra(ExtraAction), intent.GetStringExtra(ExtraPayload));
        // 画面の再生成で同じ Intent が再処理されないよう消す
        intent.RemoveExtra(ExtraId);

        // ボタンのタップは通知が残るため消す (本体のタップは AutoCancel)
        if (args.Action is not null)
        {
            using var compat = NotificationManagerCompat.From(Application.Context);
            compat?.Cancel(args.Id);
        }

        if (IPlatformApplication.Current?.Services.GetService<INotificationService>() is NotificationService service)
        {
            service.RaiseTapped(args);
        }
    }

    public static void ShowScheduled(Context context, Intent intent)
    {
        var id = intent.GetIntExtra(ExtraId, 0);
        using var builder = CreateBuilder(context, id, intent.GetStringExtra(ExtraTitle) ?? string.Empty, intent.GetStringExtra(ExtraMessage) ?? string.Empty, intent.GetStringExtra(ExtraPayload));
        Notify(context, id, builder);
    }

    public partial void Show(int id, string title, string message, string? payload, IReadOnlyList<NotificationAction>? actions)
    {
        var context = Application.Context;
        using var builder = CreateBuilder(context, id, title, message, payload);
        if (actions is not null)
        {
            for (var i = 0; i < actions.Count; i++)
            {
                using var actionIntent = CreateContentIntent(context, id, i + 1, actions[i].Id, payload);
                builder.AddAction(0, actions[i].Title, actionIntent);
            }
        }

        Notify(context, id, builder);
    }

    public partial void Schedule(int id, string title, string message, TimeSpan delay, string? payload)
    {
        var manager = ResolveAlarmManager();
        if (manager is null)
        {
            return;
        }

        using var pendingIntent = CreateAlarmIntent(Application.Context, id, title, message, payload);
        var triggerAt = Java.Lang.JavaSystem.CurrentTimeMillis() + (long)delay.TotalMilliseconds;
        if (CanScheduleExact)
        {
            manager.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, triggerAt, pendingIntent);
        }
        else
        {
            manager.SetAndAllowWhileIdle(AlarmType.RtcWakeup, triggerAt, pendingIntent);
        }
    }

    public partial void Cancel(int id)
    {
        var context = Application.Context;
        using var compat = NotificationManagerCompat.From(context);
        compat?.Cancel(id);

        var manager = ResolveAlarmManager();
        if (manager is not null)
        {
            using var pendingIntent = CreateAlarmIntent(context, id, string.Empty, string.Empty, null);
            manager.Cancel(pendingIntent);
        }
    }

    public partial void OpenExactAlarmSettings()
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(31))
        {
            return;
        }

        using var intent = new Intent(Android.Provider.Settings.ActionRequestScheduleExactAlarm);
        intent.AddFlags(ActivityFlags.NewTask);
        Application.Context.StartActivity(intent);
    }

    private static AlarmManager? ResolveAlarmManager() =>
        (AlarmManager?)Application.Context.GetSystemService(Context.AlarmService);

    private static NotificationCompat.Builder CreateBuilder(Context context, int id, string title, string message, string? payload)
    {
        EnsureChannel(context);

        using var style = new NotificationCompat.BigTextStyle();
        style.BigText(message);
        using var contentIntent = CreateContentIntent(context, id, 0, null, payload);
        var builder = new NotificationCompat.Builder(context, ChannelId);
        builder.SetSmallIcon(_Microsoft.Android.Resource.Designer.ResourceConstant.Mipmap.appicon);
        builder.SetContentTitle(title);
        builder.SetContentText(message);
        builder.SetStyle(style);
        builder.SetPriority(NotificationCompat.PriorityDefault);
        builder.SetAutoCancel(true);
        builder.SetContentIntent(contentIntent);
        return builder;
    }

    private static void Notify(Context context, int id, NotificationCompat.Builder builder)
    {
        using var notification = builder.Build();
        using var compat = NotificationManagerCompat.From(context);
        compat?.Notify(id, notification);
    }

    private static void EnsureChannel(Context context)
    {
        var manager = (AndroidNotificationManager?)context.GetSystemService(Context.NotificationService);
        if (manager is null)
        {
            return;
        }

        using var channel = new NotificationChannel(ChannelId, "通知", NotificationImportance.Default);
        channel.Description = "アプリからの通知";
        manager.CreateNotificationChannel(channel);
    }

    // 通知本体とアクションボタンのタップで MainActivity を開く (requestCode で Intent の extras を分ける)
    private static PendingIntent CreateContentIntent(Context context, int id, int requestIndex, string? action, string? payload)
    {
        using var intent = new Intent(context, typeof(MainActivity));
        intent.PutExtra(ExtraId, id);
        if (action is not null)
        {
            intent.PutExtra(ExtraAction, action);
        }

        if (payload is not null)
        {
            intent.PutExtra(ExtraPayload, payload);
        }

        return PendingIntent.GetActivity(context, (id << 4) + requestIndex, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)!;
    }

    private static PendingIntent CreateAlarmIntent(Context context, int id, string title, string message, string? payload)
    {
        using var intent = new Intent(context, typeof(NotificationAlarmReceiver));
        intent.PutExtra(ExtraId, id);
        intent.PutExtra(ExtraTitle, title);
        intent.PutExtra(ExtraMessage, message);
        if (payload is not null)
        {
            intent.PutExtra(ExtraPayload, payload);
        }

        return PendingIntent.GetBroadcast(context, id, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)!;
    }
}
#pragma warning restore CA1822
