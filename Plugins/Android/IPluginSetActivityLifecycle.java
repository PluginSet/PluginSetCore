package com.pluginset.core;

import android.content.Context;
import android.content.Intent;
import android.content.res.Configuration;
import android.os.Bundle;
import android.view.KeyEvent;
import android.view.MotionEvent;

public interface IPluginSetActivityLifecycle {
    void onCreate(Context context, Bundle savedInstanceState);

    void onUnityPlayerUnloaded(Context context);

    void onUnityPlayerQuitted(Context context);

    void onNewIntent(Context context, Intent intent);

    // Quit Unity
    void onDestroy (Context context);

    // Pause Unity
    void onPause(Context context);

    // Resume Unity
    void onResume(Context context);

    void onStart(Context context);

    void onRestart(Context context);

    void onStop(Context context);

    // Low Memory Unity
    void onLowMemory(Context context);

    // Trim Memory Unity
    void onTrimMemory(Context context, int level);

    // This ensures the layout will be correct.
    void onConfigurationChanged(Context context, Configuration newConfig);

    // Notify Unity of the focus change.
    void onWindowFocusChanged(Context context, boolean hasFocus);

    // For some reason the multiple keyevent type is not supported by the ndk.
    // Force event injection by overriding dispatchKeyEvent().
    boolean dispatchKeyEvent(Context context, KeyEvent event);

    // Pass any events not handled by (unfocused) views straight to UnityPlayer
    boolean onKeyUp(Context context, int keyCode, KeyEvent event);
    boolean onKeyDown(Context context, int keyCode, KeyEvent event);
    boolean onTouchEvent(Context context, MotionEvent event);

    void onActivityResult(Context context, int requestCode, int resultCode, Intent data);
}
