package com.pluginset.devices;

import android.app.Application;
import android.content.Context;

import com.pluginset.core.IPluginSetApplicationLifecycle;

/**
 * 应用启动时初始化 {@link DevicesNotification}，以便冷启动点击通知时能读取 Intent。
 */
public class DevicesNotificationApplicationPlugin implements IPluginSetApplicationLifecycle {
    @Override
    public void onApplicationCreate(Application application) {
        DevicesNotification.init(application);
    }

    @Override
    public void attachBaseContext(Application application, Context base) {
    }
}
