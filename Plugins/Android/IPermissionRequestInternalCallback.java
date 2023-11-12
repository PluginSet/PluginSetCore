package com.pluginset.devices;

public interface IPermissionRequestInternalCallback {
    void onGranted(String permission);
    void onDenied(String permission);
    void onCompleted(boolean allGranted);
}
