package com.pluginset.core;

import android.content.Intent;
import android.content.res.Configuration;
import android.os.Bundle;
import android.util.Log;
import android.view.KeyEvent;
import android.view.MotionEvent;

import com.unity3d.player.UnityPlayerActivity;

public class PluginSetBaseActivity extends UnityPlayerActivity {
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        Log.i("AndroidPluginManager", "PluginSetBaseActivity attachBaseContext");
        AndroidPluginManager.GetInstance().onCreate(this, savedInstanceState);
    }

    @Override
    public void onUnityPlayerUnloaded() {
        super.onUnityPlayerUnloaded();
        Log.i("AndroidPluginManager", "PluginSetBaseActivity onUnityPlayerUnloaded");
        AndroidPluginManager.GetInstance().onUnityPlayerUnloaded(this);
    }

    @Override
    public void onUnityPlayerQuitted() {
        super.onUnityPlayerQuitted();
        AndroidPluginManager.GetInstance().onUnityPlayerQuitted(this);
    }

    @Override
    protected void onNewIntent(Intent intent) {
        super.onNewIntent(intent);
        AndroidPluginManager.GetInstance().onNewIntent(this, intent);
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        AndroidPluginManager.GetInstance().onDestroy(this);
    }

    @Override
    protected void onPause() {
        AndroidPluginManager.GetInstance().onPause(this);
        super.onPause();
    }

    @Override
    protected void onResume() {
        super.onResume();
        AndroidPluginManager.GetInstance().onResume(this);
    }

    @Override
    protected void onStart() {
        super.onStart();
        AndroidPluginManager.GetInstance().onStart(this);
    }

    @Override
    protected void onRestart() {
        super.onRestart();
        AndroidPluginManager.GetInstance().onRestart(this);
    }

    @Override
    protected void onStop() {
        super.onStop();
        AndroidPluginManager.GetInstance().onStop(this);
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        AndroidPluginManager.GetInstance().onActivityResult(this, requestCode, resultCode, data);
    }

    @Override
    public void onLowMemory() {
        super.onLowMemory();
        AndroidPluginManager.GetInstance().onLowMemory(this);
    }

    @Override
    public void onTrimMemory(int level) {
        super.onTrimMemory(level);
        AndroidPluginManager.GetInstance().onTrimMemory(this, level);
    }

    @Override
    public void onConfigurationChanged(Configuration newConfig) {
        super.onConfigurationChanged(newConfig);
        AndroidPluginManager.GetInstance().onConfigurationChanged(this, newConfig);
    }

    @Override
    public void onWindowFocusChanged(boolean hasFocus) {
        super.onWindowFocusChanged(hasFocus);
        AndroidPluginManager.GetInstance().onWindowFocusChanged(this, hasFocus);
    }

    @Override
    public boolean dispatchKeyEvent(KeyEvent event) {
        if (AndroidPluginManager.GetInstance().dispatchKeyEvent(this, event))
            return true;
        return super.dispatchKeyEvent(event);
    }

    @Override
    public boolean onKeyUp(int keyCode, KeyEvent event) {
        if (AndroidPluginManager.GetInstance().onKeyUp(this, keyCode, event))
            return true;
        return super.onKeyUp(keyCode, event);
    }

    @Override
    public boolean onKeyDown(int keyCode, KeyEvent event) {
        if (AndroidPluginManager.GetInstance().onKeyDown(this, keyCode, event))
            return true;
        return super.onKeyDown(keyCode, event);
    }

    @Override
    public boolean onTouchEvent(MotionEvent event) {
        if (AndroidPluginManager.GetInstance().onTouchEvent(this, event))
            return true;
        return super.onTouchEvent(event);
    }
}
