package com.pluginset.core;

import android.app.Application;
import android.content.Context;
import android.content.Intent;
import android.content.pm.ApplicationInfo;
import android.content.pm.PackageManager;
import android.content.res.Configuration;
import android.os.Bundle;
import android.util.Log;
import android.view.KeyEvent;
import android.view.MotionEvent;

import java.util.ArrayList;
import java.util.HashMap;

public class AndroidPluginManager implements IPluginSetApplicationLifecycle, IPluginSetActivityLifecycle {
    private static final String TAG = "AndroidPluginManager";

    private static final String APPLICATION_PLUGIN_PREFIX = "pluginset.application.";
    private static final String ACTIVITY_PLUGIN_PREFIX = "pluginset.activity.";

    private static HashMap<String, Object> createdPluginObjects = new HashMap<>();

    private static AndroidPluginManager _instance = null;
    public static AndroidPluginManager GetInstance() {
        if (_instance == null)
            _instance = new AndroidPluginManager();

        return _instance;
    }

    private ArrayList<IPluginSetApplicationLifecycle> applicationLifecycles = new ArrayList<>();
    private ArrayList<IPluginSetActivityLifecycle> activityLifecycles = new ArrayList<>();

    @Override
    public void onApplicationCreate(Application application) {
        activityLifecycles.clear();
        applicationLifecycles.clear();

        Bundle metaData = null;

        ApplicationInfo info = null;
        try {
            info = application.getPackageManager().getApplicationInfo(application.getPackageName(), PackageManager.GET_META_DATA);
            metaData = info.metaData;
        } catch (PackageManager.NameNotFoundException e) {
            e.printStackTrace();
        }
        if (metaData == null)
        {
            Log.e(TAG, "Get meta data of application fail, init life cycles fail!");
            return;
        }

        Log.i(TAG, "Start add plugins to activity or application!");
        for (String key: metaData.keySet()) {
            if (key.startsWith(APPLICATION_PLUGIN_PREFIX)) {
                tryAddApplicationPlugin(metaData.getString(key, null));
                continue;
            }
            if (key.startsWith(ACTIVITY_PLUGIN_PREFIX)) {
                tryAddActivityPlugin(metaData.getString(key, null));
                continue;
            }
        }

        for (IPluginSetApplicationLifecycle plugin: applicationLifecycles) {
            plugin.onApplicationCreate(application);
        }
    }

    @Override
    public void attachBaseContext(Application application, Context base) {
        for (IPluginSetApplicationLifecycle plugin: applicationLifecycles) {
            plugin.attachBaseContext(application, base);
        }
    }

    @Override
    public void onCreate(Context context, Bundle savedInstanceState) {
        for (IPluginSetActivityLifecycle plugin: activityLifecycles) {
            plugin.onCreate(context, savedInstanceState);
        }
    }

    @Override
    public void onUnityPlayerUnloaded(Context context) {
        for (IPluginSetActivityLifecycle plugin: activityLifecycles) {
            plugin.onUnityPlayerUnloaded(context);
        }
    }

    @Override
    public void onUnityPlayerQuitted(Context context) {
        for (IPluginSetActivityLifecycle plugin: activityLifecycles) {
            plugin.onUnityPlayerQuitted(context);
        }
    }

    @Override
    public void onNewIntent(Context context, Intent intent) {
        for (IPluginSetActivityLifecycle plugin: activityLifecycles) {
            plugin.onNewIntent(context, intent);
        }
    }

    @Override
    public void onDestroy(Context context) {
        for (IPluginSetActivityLifecycle plugin: activityLifecycles) {
            plugin.onDestroy(context);
        }
    }

    @Override
    public void onPause(Context context) {
        for (IPluginSetActivityLifecycle plugin: activityLifecycles) {
            plugin.onPause(context);
        }
    }

    @Override
    public void onResume(Context context) {
        for (IPluginSetActivityLifecycle plugin: activityLifecycles) {
            plugin.onResume(context);
        }
    }

    @Override
    public void onStart(Context context) {
        for (IPluginSetActivityLifecycle plugin: activityLifecycles) {
            plugin.onStart(context);
        }
    }

    @Override
    public void onRestart(Context context) {
        for (IPluginSetActivityLifecycle plugin: activityLifecycles) {
            plugin.onRestart(context);
        }
    }

    @Override
    public void onStop(Context context) {
        for (IPluginSetActivityLifecycle plugin: activityLifecycles) {
            plugin.onStop(context);
        }
    }

    @Override
    public void onLowMemory(Context context) {
        for (IPluginSetActivityLifecycle plugin: activityLifecycles) {
            plugin.onLowMemory(context);
        }
    }

    @Override
    public void onTrimMemory(Context context, int level) {
        for (IPluginSetActivityLifecycle plugin: activityLifecycles) {
            plugin.onTrimMemory(context, level);
        }
    }

    @Override
    public void onConfigurationChanged(Context context, Configuration newConfig) {
        for (IPluginSetActivityLifecycle plugin: activityLifecycles) {
            plugin.onConfigurationChanged(context, newConfig);
        }
    }

    @Override
    public void onWindowFocusChanged(Context context, boolean hasFocus) {
        for (IPluginSetActivityLifecycle plugin: activityLifecycles) {
            plugin.onWindowFocusChanged(context, hasFocus);
        }
    }

    @Override
    public boolean dispatchKeyEvent(Context context, KeyEvent event) {
        for (IPluginSetActivityLifecycle plugin: activityLifecycles) {
            if (plugin.dispatchKeyEvent(context, event))
                return true;
        }
        return false;
    }

    @Override
    public boolean onKeyUp(Context context, int keyCode, KeyEvent event) {
        for (IPluginSetActivityLifecycle plugin: activityLifecycles) {
            if (plugin.onKeyUp(context, keyCode, event))
                return true;
        }
        return false;
    }

    @Override
    public boolean onKeyDown(Context context, int keyCode, KeyEvent event) {
        for (IPluginSetActivityLifecycle plugin: activityLifecycles) {
            if (plugin.onKeyDown(context, keyCode, event))
                return true;
        }
        return false;
    }

    @Override
    public boolean onTouchEvent(Context context, MotionEvent event) {
        for (IPluginSetActivityLifecycle plugin: activityLifecycles) {
            if (plugin.onTouchEvent(context, event))
                return true;
        }
        return false;
    }

    @Override
    public void onActivityResult(Context context, int requestCode, int resultCode, Intent data) {
        for (IPluginSetActivityLifecycle plugin: activityLifecycles) {
            plugin.onActivityResult(context, requestCode, resultCode, data);
        }
    }

    private Object findCreatedPlugin(String pluginName) {
        if (createdPluginObjects.containsKey(pluginName))
            return createdPluginObjects.get(pluginName);

        return null;
    }

    private void tryAddApplicationPlugin(String pluginName) {
        if (pluginName == null || pluginName.isEmpty()) return;

        Object inst = findCreatedPlugin(pluginName);
        if (inst != null && inst instanceof IPluginSetApplicationLifecycle)
        {
            applicationLifecycles.add((IPluginSetApplicationLifecycle) inst);
            return;
        }

        try {
            Class mClass = Class.forName(pluginName);
            Log.i(TAG, "find application plugin:: " + pluginName + " >> class:: " + mClass);
            IPluginSetApplicationLifecycle plug = (IPluginSetApplicationLifecycle) mClass.newInstance();
            if (plug == null) {
                Log.w(TAG, "cannot create application plugin:: " + pluginName);
            } else{
                applicationLifecycles.add(plug);
            }
            createdPluginObjects.put(pluginName, plug);
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    private void tryAddActivityPlugin(String pluginName) {
        if (pluginName == null || pluginName.isEmpty()) return;

        Object inst = findCreatedPlugin(pluginName);
        if (inst != null && inst instanceof IPluginSetActivityLifecycle)
        {
            activityLifecycles.add((IPluginSetActivityLifecycle) inst);
            return;
        }

        try {
            Class mClass = Class.forName(pluginName);
            Log.i(TAG, "find activity plugin:: " + pluginName + " >> class:: " + mClass);
            IPluginSetActivityLifecycle plug = (IPluginSetActivityLifecycle) mClass.newInstance();
            if (plug == null) {
                Log.w(TAG, "cannot create activity plugin:: " + pluginName);
            } else{
                activityLifecycles.add(plug);
            }
            createdPluginObjects.put(pluginName, plug);
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
