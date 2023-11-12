package com.pluginset.devices;

public interface IPermissionRequestCallback {
    void onGranted(String permission);
    void onDenied(String permission);
    void onDeniedAlways(String permission);
    void onCompleted(boolean allGranted);
}