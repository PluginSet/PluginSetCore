package com.pluginset.core;

import android.app.Application;
import android.content.Context;

public class PluginSetBaseApplication extends Application {
    @Override
    public void onCreate() {
        super.onCreate();
        AndroidPluginManager.GetInstance().onApplicationCreate(this);
    }

    @Override
    protected void attachBaseContext(Context base) {
        super.attachBaseContext(base);
        AndroidPluginManager.GetInstance().attachBaseContext(this, base);
    }
}
