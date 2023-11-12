package com.pluginset.devices;

import android.app.AlarmManager;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.graphics.BitmapFactory;
import android.os.Build;
import android.os.SystemClock;

import androidx.core.app.NotificationCompat;

public class Notification extends BroadcastReceiver {

    private static final String CHANNEL = "barrett";

    /**
     * 推送一条信息
     *
     * @param message 消息内容
     */
    public static void NotificationMessage(Context context, String message) {
//        Utils.LogI("show message:" + message);
//        Intent intent = new Intent(context , UnityPlayerActivity.class);
//        PendingIntent pi = PendingIntent.getActivity(context,0,intent,0);
//
//        NotificationManager manager = (NotificationManager)context.getSystemService(Context.NOTIFICATION_SERVICE);
//
//        // 此处必须兼容android O设备，否则系统版本在O以上可能不展示通知栏
//        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
//            NotificationChannel channel = new NotificationChannel(CHANNEL, context.getPackageName(), NotificationManager.IMPORTANCE_DEFAULT);
//            manager.createNotificationChannel(channel);
//        }
//
//        android.app.Notification notification = new NotificationCompat.Builder(context,CHANNEL)
//                .setContentTitle(context.getString(R.string.app_name))  //设置标题
//                .setContentText(message) //设置内容
//                .setSmallIcon(R.drawable.app_notify_icon)  //设置小图标  只能使用alpha图层的图片进行设置
//                .setLargeIcon(BitmapFactory.decodeResource(context.getResources(),R.mipmap.app_icon))   //设置大图标
//                .setContentIntent(pi)
//                .setAutoCancel(true)
//                .build();
//
//        manager.notify(1,notification);
    }

    /**
     * 取消所有通知
     */
    public static void CleanAllExistNotification() {
//        Context ctx = UnityPlayer.currentActivity;
//        NotificationManager manager = (NotificationManager)ctx.getSystemService(Context.NOTIFICATION_SERVICE);
//        manager.cancelAll();
    }

    // Setup activity layout
    @Override
    public void onReceive(Context context, Intent intent) {

//        String action = intent.getAction();
//        util.PlatformLog("receive!!!! action:" + action);
//        if (!action.equals("com.sagi.barrett.push")) {
//            return;
//        }
//
//        String message = intent.getStringExtra("message");
//        NotificationHelper.NotificationMessage(context,message);
    }

    /**
     * 设置一个闹钟消息
     *
     * @param message
     * @param millsec
     */
    public static void SetAlarmOneToNotification(String message, int millsec) {
        Utils.LogI("SetAlarmOneToNotification not implemented");
//        util.PlatformLog("send timer event!!!!");
//        util.PlatformLog("message:"+message);
//        util.PlatformLog("after:"+millsec);
//        Context ctx = UnityPlayer.currentActivity;
//
//        // 获得闹钟服务
//        AlarmManager alarmManager = (AlarmManager) ctx.getSystemService(Context.ALARM_SERVICE);
//
//        Intent intent = new Intent();
//        intent.setAction("com.sagi.barrett.push");
//        intent.setPackage(ctx.getPackageName());
//        intent.putExtra("message", message);
//        PendingIntent pendingIntent = PendingIntent.getBroadcast(ctx,0,intent,PendingIntent.FLAG_UPDATE_CURRENT);
//
//        long now = SystemClock.elapsedRealtime();
//        util.PlatformLog("now:" + now + " after:"+millsec);
//        long triggerAtTime = now + millsec;
//
//        // 区分版本23以上和以下
//        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.M) {
//            alarmManager.setExact(AlarmManager.ELAPSED_REALTIME_WAKEUP , triggerAtTime, pendingIntent);
//        }else{
//            alarmManager.setExactAndAllowWhileIdle(AlarmManager.ELAPSED_REALTIME_WAKEUP , triggerAtTime, pendingIntent);
//        }
    }

    /**
     * 清除所有闹钟
     */
    public static void ClearAllAlarm() {
        Utils.LogI("ClearAllAlarm not implemented");
//        util.PlatformLog("cancel all alarm");
//        Context ctx = UnityPlayer.currentActivity;
//
//        // 用于取消的pi
//        Intent intent = new Intent();
//        intent.setAction("com.sagi.barrett.push");
//        intent.setPackage(ctx.getPackageName());
//        PendingIntent pendingIntent = PendingIntent.getBroadcast(ctx,0,intent,0);
//
//        // 获得闹钟服务
//        AlarmManager alarmManager = (AlarmManager) ctx.getSystemService(Context.ALARM_SERVICE);
//        alarmManager.cancel(pendingIntent);
    }

}
