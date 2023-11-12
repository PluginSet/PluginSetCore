package com.pluginset.core;

public interface IPluginSetCallback {
    void onSuccess(String result);
    void onFailed(int code, String message);
}
