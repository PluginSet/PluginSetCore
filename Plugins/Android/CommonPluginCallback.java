package com.pluginset.devices;

public interface CommonPluginCallback {
    void OnCallback(String str);

    /**
     * @param resultMsg
     */
    void OnSuccess(String resultMsg);

    /**
     * @param resultMsg
     * @param resultCode
     */
    void OnFailure(String resultMsg, int resultCode);
}