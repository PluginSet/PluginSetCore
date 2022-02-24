package com.pluginset.core;

import android.app.Application;
import android.content.Context;
import android.util.Log;

public class PluginSetBaseApplication extends Application {
    @Override
    public void onCreate() {
        super.onCreate();
        Log.i("AndroidPluginManager", "PluginSetBaseApplication onCreate");
        AndroidPluginManager.GetInstance().onApplicationCreate(this);
    }

    @Override
    protected void attachBaseContext(Context base) {
        super.attachBaseContext(base);
        Log.i("AndroidPluginManager", "PluginSetBaseApplication attachBaseContext");
        AndroidPluginManager.GetInstance().attachBaseContext(this, base);
    }
}
