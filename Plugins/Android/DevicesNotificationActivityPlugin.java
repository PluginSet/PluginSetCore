package com.pluginset.devices;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.os.Bundle;

import com.pluginset.core.IPluginSetActivityLifecycle;

/**
 * 将通知点击 Intent 交给 {@link DevicesNotification}，并在 warm start 时更新 Activity intent。
 */
public class DevicesNotificationActivityPlugin implements IPluginSetActivityLifecycle {
    @Override
    public void onCreate(Context context, Bundle savedInstanceState) {
        if (context instanceof Activity) {
            Activity activity = (Activity) context;
            DevicesNotification.ensureInitialized(activity);
            DevicesNotification.onNewIntent(activity.getIntent());
        }
    }

    @Override
    public void onNewIntent(Context context, Intent intent) {
        if (context instanceof Activity && intent != null) {
            Activity activity = (Activity) context;
            activity.setIntent(intent);
            DevicesNotification.onNewIntent(intent);
        }
    }

    @Override
    public void onDestroy(Context context) {
    }

    @Override
    public void onPause(Context context) {
    }

    @Override
    public void onResume(Context context) {
    }

    @Override
    public void onStart(Context context) {
    }

    @Override
    public void onStop(Context context) {
    }

    @Override
    public void onRestart(Context context) {
    }

    @Override
    public void onActivityResult(Context context, int requestCode, int resultCode, Intent data) {
    }

    @Override
    public void onLowMemory(Context context) {
    }

    @Override
    public void onTrimMemory(Context context, int level) {
    }

    @Override
    public void onConfigurationChanged(Context context, android.content.res.Configuration newConfig) {
    }

    @Override
    public void onWindowFocusChanged(Context context, boolean hasFocus) {
    }

    @Override
    public boolean dispatchKeyEvent(Context context, android.view.KeyEvent event) {
        return false;
    }

    @Override
    public boolean onKeyUp(Context context, int keyCode, android.view.KeyEvent event) {
        return false;
    }

    @Override
    public boolean onKeyDown(Context context, int keyCode, android.view.KeyEvent event) {
        return false;
    }

    @Override
    public boolean onTouchEvent(Context context, android.view.MotionEvent event) {
        return false;
    }

    @Override
    public void onUnityPlayerUnloaded(Context context) {
    }

    @Override
    public void onUnityPlayerQuitted(Context context) {
    }
}
