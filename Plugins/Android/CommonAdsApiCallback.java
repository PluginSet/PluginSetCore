package com.pluginset.devices;

public interface CommonAdsApiCallback {
    void OnLoadSuccess();

    void OnLoadFail(String msg);

    void OnPlayFail(String msg);

    void OnAdsClose(boolean isComplete);

    void OnAdsClick();

    void OnAdsShow();

    void OnInitSuccess();

    void OnInitFail(String msg);
}