package com.pluginset.devices;

import android.app.Activity;
import android.app.AlarmManager;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.net.Uri;
import android.os.Build;
import android.os.PowerManager;
import android.provider.Settings;
import android.text.TextUtils;
import android.util.Log;

import androidx.core.app.ActivityCompat;
import androidx.core.app.NotificationCompat;
import androidx.core.content.ContextCompat;

import org.json.JSONObject;

import java.util.ArrayList;
import java.util.HashSet;
import java.util.Set;

/**
 * 系统通知栏能力（与 FCM/APNs SDK 无关）。
 */
public final class DevicesNotification {
    private static final String TAG = "DevicesNotification";
    private static final int REQUEST_POST_NOTIFICATIONS = 0x504e;

    private static final Set<Integer> scheduledNotificationIds = new HashSet<>();

    private static Context appContext;
    private static String defaultChannelId = "pluginset_push_default";
    private static String defaultChannelName = "Push Notifications";
    private static boolean requestPermissionOnRequest = true;
    private static String pendingOpenPayload;

    private DevicesNotification() {
    }

    public static void init(Context context) {
        if (context == null) {
            return;
        }
        appContext = context.getApplicationContext();
        createNotificationChannel(defaultChannelId, defaultChannelName);
        captureLaunchIntentIfNeeded(context);
    }

    public static void configureChannel(String channelId, String channelName, boolean requestPermission) {
        if (!TextUtils.isEmpty(channelId)) {
            defaultChannelId = channelId;
        }
        if (!TextUtils.isEmpty(channelName)) {
            defaultChannelName = channelName;
        }
        requestPermissionOnRequest = requestPermission;
        createNotificationChannel(defaultChannelId, defaultChannelName);
    }

    public static void requestPostNotificationPermission(Activity activity) {
        if (activity == null || Build.VERSION.SDK_INT < Build.VERSION_CODES.TIRAMISU) {
            return;
        }
        if (ContextCompat.checkSelfPermission(activity, android.Manifest.permission.POST_NOTIFICATIONS)
                == android.content.pm.PackageManager.PERMISSION_GRANTED) {
            return;
        }
        if (requestPermissionOnRequest) {
            ActivityCompat.requestPermissions(
                    activity,
                    new String[]{android.Manifest.permission.POST_NOTIFICATIONS},
                    REQUEST_POST_NOTIFICATIONS);
        }
    }

    /** Android 12+：是否允许精确闹钟（未授权时延迟通知可能被系统/MIUI 大幅推迟）。 */
    public static boolean canScheduleExactAlarms() {
        if (appContext == null || Build.VERSION.SDK_INT < Build.VERSION_CODES.S) {
            return true;
        }
        AlarmManager alarm = (AlarmManager) appContext.getSystemService(Context.ALARM_SERVICE);
        return alarm != null && alarm.canScheduleExactAlarms();
    }

    /** 打开系统「闹钟和提醒」授权页（Android 12+）。 */
    public static void openExactAlarmSettings(Activity activity) {
        if (activity == null || Build.VERSION.SDK_INT < Build.VERSION_CODES.S) {
            return;
        }
        try {
            Intent intent = new Intent(Settings.ACTION_REQUEST_SCHEDULE_EXACT_ALARM);
            intent.setData(Uri.parse("package:" + activity.getPackageName()));
            activity.startActivity(intent);
        } catch (Exception e) {
            Log.w(TAG, "openExactAlarmSettings failed", e);
        }
    }

    public static boolean areNotificationsEnabled() {
        if (appContext == null) {
            return false;
        }
        NotificationManager manager =
                (NotificationManager) appContext.getSystemService(Context.NOTIFICATION_SERVICE);
        if (manager == null) {
            return false;
        }
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.N) {
            return manager.areNotificationsEnabled();
        }
        return true;
    }

    public static void createNotificationChannel(String channelId, String channelName) {
        if (appContext == null || Build.VERSION.SDK_INT < Build.VERSION_CODES.O) {
            return;
        }
        if (TextUtils.isEmpty(channelId)) {
            return;
        }
        NotificationManager manager =
                (NotificationManager) appContext.getSystemService(Context.NOTIFICATION_SERVICE);
        if (manager == null) {
            return;
        }
        NotificationChannel channel = new NotificationChannel(
                channelId,
                TextUtils.isEmpty(channelName) ? channelId : channelName,
                NotificationManager.IMPORTANCE_DEFAULT);
        manager.createNotificationChannel(channel);
    }

    public static void removeAllDeliveredNotifications() {
        if (appContext == null) {
            return;
        }
        NotificationManager manager =
                (NotificationManager) appContext.getSystemService(Context.NOTIFICATION_SERVICE);
        if (manager != null) {
            manager.cancelAll();
        }
    }

    public static void removeDeliveredNotifications(String[] ids) {
        if (appContext == null || ids == null) {
            return;
        }
        NotificationManager manager =
                (NotificationManager) appContext.getSystemService(Context.NOTIFICATION_SERVICE);
        if (manager == null) {
            return;
        }
        for (String id : ids) {
            try {
                manager.cancel(Integer.parseInt(id));
            } catch (NumberFormatException e) {
                Log.w(TAG, "invalid notification id: " + id);
            }
        }
    }

    public static void setBadgeNumber(int count) {
        Log.d(TAG, "setBadgeNumber not supported on Android, count=" + count);
    }

    public static int getBadgeNumber() {
        return 0;
    }

    public static String consumeInitialNotificationJson() {
        String payload = pendingOpenPayload;
        pendingOpenPayload = null;
        return payload != null ? payload : "";
    }

    public static void onNewIntent(Intent intent) {
        captureIntent(intent, false);
    }

    public static void ensureInitialized(Activity activity) {
        if (appContext == null && activity != null) {
            init(activity.getApplicationContext());
        }
    }

    public static void postLocalNotification(Activity activity, String notificationId, String channelId,
            String title, String body, int delaySeconds, String dataJson, String actionId) {
        ensureInitialized(activity);
        if (appContext == null) {
            Log.w(TAG, "postLocalNotification: appContext is null");
            return;
        }
        String ch = TextUtils.isEmpty(channelId) ? defaultChannelId : channelId;
        createNotificationChannel(ch, defaultChannelName);
        int id = toNotificationId(notificationId);
        String payloadJson = TextUtils.isEmpty(dataJson) ? "{}" : dataJson;
        String action = actionId != null ? actionId : "";
        if (delaySeconds <= 0) {
            Log.i(TAG, "postLocalNotification immediate id=" + id);
            showLocalNotificationImmediate(ch, id, title, body, payloadJson, action);
            return;
        }
        Log.i(TAG, "postLocalNotification schedule id=" + id + " delaySeconds=" + delaySeconds);
        scheduleLocalNotification(activity, id, ch, title, body, delaySeconds, payloadJson, action);
    }

    public static void cancelLocalNotification(String notificationId) {
        if (appContext == null) {
            return;
        }
        int id = toNotificationId(notificationId);
        NotificationManager manager =
                (NotificationManager) appContext.getSystemService(Context.NOTIFICATION_SERVICE);
        if (manager != null) {
            manager.cancel(id);
        }
        cancelScheduledLocalNotification(id);
    }

    public static void cancelAllPendingLocalNotifications() {
        if (appContext == null) {
            return;
        }
        NotificationManager manager =
                (NotificationManager) appContext.getSystemService(Context.NOTIFICATION_SERVICE);
        if (manager != null) {
            manager.cancelAll();
        }
        synchronized (scheduledNotificationIds) {
            for (Integer id : new ArrayList<>(scheduledNotificationIds)) {
                cancelScheduledLocalNotification(id);
            }
            scheduledNotificationIds.clear();
        }
    }

    static void showLocalNotificationFromIntent(Context context, Intent intent) {
        if (context == null || intent == null) {
            return;
        }
        if (appContext == null) {
            appContext = context.getApplicationContext();
        }
        String channelId = intent.getStringExtra("channelId");
        if (TextUtils.isEmpty(channelId)) {
            channelId = defaultChannelId;
        }
        int id = intent.getIntExtra("notificationId", 0);
        synchronized (scheduledNotificationIds) {
            scheduledNotificationIds.remove(id);
        }
        showLocalNotificationImmediate(
                channelId,
                id,
                intent.getStringExtra("title"),
                intent.getStringExtra("body"),
                intent.getStringExtra("dataJson"),
                intent.getStringExtra("actionId"));
    }

    private static void showLocalNotificationImmediate(
            String channelId, int notificationId, String title, String body, String dataJson, String actionId) {
        NotificationManager manager =
                (NotificationManager) appContext.getSystemService(Context.NOTIFICATION_SERVICE);
        if (manager == null) {
            return;
        }
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.N && !manager.areNotificationsEnabled()) {
            Log.w(TAG, "showLocalNotificationImmediate: notifications disabled by user id=" + notificationId);
            return;
        }
        int smallIcon = appContext.getApplicationInfo().icon;
        NotificationCompat.Builder builder = new NotificationCompat.Builder(appContext, channelId)
                .setSmallIcon(smallIcon)
                .setContentTitle(title != null ? title : "")
                .setContentText(body != null ? body : "")
                .setAutoCancel(true)
                .setPriority(NotificationCompat.PRIORITY_DEFAULT);
        PendingIntent contentIntent = createNotificationContentIntent(
                notificationId, title, body, dataJson, actionId);
        if (contentIntent != null) {
            builder.setContentIntent(contentIntent);
        } else {
            Log.w(TAG, "showLocalNotificationImmediate: no content intent, tap will not open app");
        }
        manager.notify(notificationId, builder.build());
        Log.i(TAG, "showLocalNotificationImmediate posted id=" + notificationId);
    }

    private static PendingIntent createNotificationContentIntent(
            int notificationId, String title, String body, String dataJson, String actionId) {
        if (appContext == null) {
            return null;
        }
        Intent launchIntent = appContext.getPackageManager().getLaunchIntentForPackage(appContext.getPackageName());
        if (launchIntent == null) {
            Log.w(TAG, "createNotificationContentIntent: getLaunchIntentForPackage returned null");
            return null;
        }
        launchIntent.setFlags(
                Intent.FLAG_ACTIVITY_NEW_TASK
                        | Intent.FLAG_ACTIVITY_CLEAR_TOP
                        | Intent.FLAG_ACTIVITY_SINGLE_TOP);
        launchIntent.putExtra("push_title", title != null ? title : "");
        launchIntent.putExtra("push_body", body != null ? body : "");
        launchIntent.putExtra("push_data", TextUtils.isEmpty(dataJson) ? "{}" : dataJson);
        launchIntent.putExtra("push_action_id", actionId != null ? actionId : "");
        int flags = PendingIntent.FLAG_UPDATE_CURRENT;
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
            flags |= PendingIntent.FLAG_IMMUTABLE;
        }
        return PendingIntent.getActivity(appContext, notificationId, launchIntent, flags);
    }

    private static void scheduleLocalNotification(Activity activity, int notificationId, String channelId,
            String title, String body, int delaySeconds, String dataJson, String actionId) {
        Context context = activity.getApplicationContext();
        Intent intent = new Intent(context, DevicesLocalNotificationReceiver.class);
        intent.putExtra("notificationId", notificationId);
        intent.putExtra("channelId", channelId);
        intent.putExtra("title", title != null ? title : "");
        intent.putExtra("body", body != null ? body : "");
        intent.putExtra("dataJson", TextUtils.isEmpty(dataJson) ? "{}" : dataJson);
        intent.putExtra("actionId", actionId != null ? actionId : "");
        int flags = PendingIntent.FLAG_UPDATE_CURRENT;
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
            flags |= PendingIntent.FLAG_IMMUTABLE;
        }
        PendingIntent pending = PendingIntent.getBroadcast(context, notificationId, intent, flags);
        AlarmManager alarm = (AlarmManager) context.getSystemService(Context.ALARM_SERVICE);
        if (alarm == null) {
            return;
        }
        long triggerAt = System.currentTimeMillis() + delaySeconds * 1000L;
        PendingIntent showIntent = createNotificationContentIntent(
                notificationId, title, body, dataJson, actionId);
        scheduleAlarm(alarm, triggerAt, pending, showIntent);
        synchronized (scheduledNotificationIds) {
            scheduledNotificationIds.add(notificationId);
        }
        Log.i(TAG, "scheduleLocalNotification id=" + notificationId + " triggerAt=" + triggerAt
                + " exactAlarms=" + (Build.VERSION.SDK_INT < Build.VERSION_CODES.S || alarm.canScheduleExactAlarms())
                + " notificationsEnabled=" + areNotificationsEnabled());
    }

    private static void scheduleAlarm(AlarmManager alarm, long triggerAt, PendingIntent pending, PendingIntent showIntent) {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP && showIntent != null) {
            boolean exactAllowed = Build.VERSION.SDK_INT < Build.VERSION_CODES.S || alarm.canScheduleExactAlarms();
            if (exactAllowed) {
                try {
                    alarm.setAlarmClock(new AlarmManager.AlarmClockInfo(triggerAt, showIntent), pending);
                    Log.i(TAG, "scheduleAlarm setAlarmClock triggerAt=" + triggerAt);
                    return;
                } catch (SecurityException e) {
                    Log.w(TAG, "setAlarmClock failed, trying setExact", e);
                }
            }
        }
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.S) {
            if (alarm.canScheduleExactAlarms()) {
                alarm.setExactAndAllowWhileIdle(AlarmManager.RTC_WAKEUP, triggerAt, pending);
                Log.i(TAG, "scheduleAlarm setExactAndAllowWhileIdle triggerAt=" + triggerAt);
                return;
            }
            Log.w(TAG, "canScheduleExactAlarms=false, fallback to setAndAllowWhileIdle (MIUI may cache/skip alarm)");
        }
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
            alarm.setAndAllowWhileIdle(AlarmManager.RTC_WAKEUP, triggerAt, pending);
            Log.i(TAG, "scheduleAlarm setAndAllowWhileIdle triggerAt=" + triggerAt);
        } else {
            alarm.set(AlarmManager.RTC_WAKEUP, triggerAt, pending);
            Log.i(TAG, "scheduleAlarm set triggerAt=" + triggerAt);
        }
    }

    private static void cancelScheduledLocalNotification(int notificationId) {
        if (appContext == null) {
            return;
        }
        Intent intent = new Intent(appContext, DevicesLocalNotificationReceiver.class);
        int flags = PendingIntent.FLAG_UPDATE_CURRENT;
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
            flags |= PendingIntent.FLAG_IMMUTABLE;
        }
        PendingIntent pending = PendingIntent.getBroadcast(
                appContext, notificationId, intent, flags | PendingIntent.FLAG_NO_CREATE);
        if (pending != null) {
            AlarmManager alarm = (AlarmManager) appContext.getSystemService(Context.ALARM_SERVICE);
            if (alarm != null) {
                alarm.cancel(pending);
            }
            pending.cancel();
        }
        synchronized (scheduledNotificationIds) {
            scheduledNotificationIds.remove(notificationId);
        }
    }

    private static int toNotificationId(String notificationId) {
        if (TextUtils.isEmpty(notificationId)) {
            return (int) (System.currentTimeMillis() & 0x7fffffff);
        }
        return Math.abs(notificationId.hashCode());
    }

    private static void captureLaunchIntentIfNeeded(Context context) {
        if (!(context instanceof Activity)) {
            return;
        }
        captureIntent(((Activity) context).getIntent(), true);
    }

    private static void captureIntent(Intent intent, boolean isInitialLaunch) {
        if (intent == null) {
            return;
        }
        String title = intent.getStringExtra("push_title");
        String body = intent.getStringExtra("push_body");
        String dataJson = intent.getStringExtra("push_data");
        String actionId = intent.getStringExtra("push_action_id");
        boolean hasData = !TextUtils.isEmpty(dataJson) && !"{}".equals(dataJson);
        if (TextUtils.isEmpty(title) && TextUtils.isEmpty(body) && !hasData && TextUtils.isEmpty(actionId)) {
            return;
        }
        try {
            JSONObject data = TextUtils.isEmpty(dataJson) ? new JSONObject() : new JSONObject(dataJson);
            JSONObject payload = new JSONObject();
            payload.put("title", title != null ? title : "");
            payload.put("body", body != null ? body : "");
            payload.put("actionId", actionId != null ? actionId : "");
            payload.put("data", data);
            payload.put("isInitialLaunch", isInitialLaunch);
            pendingOpenPayload = payload.toString();
        } catch (Exception e) {
            Log.w(TAG, "captureIntent error", e);
        }
        intent.removeExtra("push_title");
        intent.removeExtra("push_body");
        intent.removeExtra("push_data");
        intent.removeExtra("push_action_id");
    }
}
