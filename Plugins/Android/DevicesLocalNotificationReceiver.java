package com.pluginset.devices;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.os.PowerManager;
import android.util.Log;

/**
 * 用于延迟触发的本地通知 Alarm。
 */
public class DevicesLocalNotificationReceiver extends BroadcastReceiver {
    private static final String TAG = "DevicesLocalNotificationRx";

    @Override
    public void onReceive(Context context, Intent intent) {
        if (context == null || intent == null) {
            return;
        }
        Log.i(TAG, "onReceive notificationId=" + intent.getIntExtra("notificationId", 0));
        PowerManager pm = (PowerManager) context.getSystemService(Context.POWER_SERVICE);
        PowerManager.WakeLock wakeLock = null;
        if (pm != null) {
            wakeLock = pm.newWakeLock(PowerManager.PARTIAL_WAKE_LOCK, "PluginSet:LocalNotification");
            wakeLock.acquire(15_000L);
        }
        try {
            DevicesNotification.showLocalNotificationFromIntent(context, intent);
        } finally {
            if (wakeLock != null && wakeLock.isHeld()) {
                wakeLock.release();
            }
        }
    }
}
