package com.pluginset.core;

import android.app.Application;
import android.content.Context;

public interface IPluginSetApplicationLifecycle {
    void onApplicationCreate(Application application);

    void attachBaseContext(Application application, Context base);
}
