package com.pluginset.devices;

import android.app.Activity;
import android.app.FragmentTransaction;
import android.content.ClipData;
import android.content.ClipboardManager;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.pm.ApplicationInfo;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.net.Uri;
import android.os.BatteryManager;
import android.os.Build;
import android.os.Bundle;
import android.os.Looper;
import android.os.VibrationEffect;
import android.os.Vibrator;
import android.provider.Settings;
import android.telephony.TelephonyManager;
import android.text.TextUtils;
import android.util.Log;

import java.lang.reflect.Method;
import java.util.ArrayList;

import androidx.core.content.ContextCompat;

public class Utils {
    private static final String TAG = "PluginSet";
    private static final int PermissionDenied = 0;
    private static final int PermissionGranted = 1;
    private static final int PermissionShouldAsk = 2;

    public static void CopyToClipboard(Context context, String input)
    {
        ClipboardManager cm = (ClipboardManager) context.getSystemService(Context.CLIPBOARD_SERVICE);
        ClipData mClipData = ClipData.newPlainText("Label", input);
        cm.setPrimaryClip(mClipData);
    }

    public static String ReadFromClipboard(Context context)
    {
        ClipboardManager cm = (ClipboardManager) context.getSystemService(Context.CLIPBOARD_SERVICE);
        if(cm.hasPrimaryClip())
        {
            return cm.getPrimaryClip().toString();
        }else{
            return "";
        }
    }

    public static String GetApplicationMetaString(Context context, String key, String defaultValue)
    {
        ApplicationInfo info ;
        try {
            info = context.getPackageManager().getApplicationInfo(
                    context.getPackageName(), PackageManager.GET_META_DATA);
            String var = info.metaData.getString(key, defaultValue);
            return var.trim();
        } catch (Exception e) {
            return defaultValue;
        }
    }

    public static int GetApplicationMetaInt(Context context, String key, int defaultValue)
    {
        ApplicationInfo info ;
        try {
            info = context.getPackageManager().getApplicationInfo(
                    context.getPackageName(), PackageManager.GET_META_DATA);
            return info.metaData.getInt(key, defaultValue);
        } catch (Exception e) {
            return defaultValue;
        }
    }

    public static void Vibrate(Context context, long milliseconds)
    {
        try {
            Vibrator v = (Vibrator) context.getSystemService(Context.VIBRATOR_SERVICE);
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
                v.vibrate(VibrationEffect.createOneShot(milliseconds, VibrationEffect.DEFAULT_AMPLITUDE));
            } else {
                //deprecated in API 26
                v.vibrate(milliseconds);
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public static int CheckPermission(Activity context, String permissionString)
    {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.M)
            return PermissionGranted;

        String[] list = permissionString.split("\\|");
        for (String permission: list) {
            int result = ContextCompat.checkSelfPermission(context, permission);
            Log.d("CheckPermission", permissionString + "   " + result);
            if (result == PackageManager.PERMISSION_GRANTED)
            {
                return PermissionGranted;
            }
            else
            {
                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
                    Log.d("CheckPermission", permissionString + "   " + context.shouldShowRequestPermissionRationale(permission));
                    if (!context.shouldShowRequestPermissionRationale(permission))
                        return PermissionDenied;
                }

                return PermissionShouldAsk;
            }
        }
        return PermissionGranted;
    }

    public static boolean IsPermissionGranted(Activity context, String permissionString)
    {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.M)
            return true;

        String[] list = permissionString.split("\\|");
        for (String permission: list) {
            int result = ContextCompat.checkSelfPermission(context, permission);
            if (result != PackageManager.PERMISSION_GRANTED)
                return false;
        }
        return true;
    }

    /**
     * 请求动态权限
     * @param context
     * @param permissionString 权限名称
     * @param callback 回调
     */
    // Credit: https://github.com/Over17/UnityAndroidPermissions/blob/0dca33e40628f1f279decb67d901fd444b409cd7/src/UnityAndroidPermissions/src/main/java/com/unity3d/plugin/UnityAndroidPermissions.java
    // Credit: https://github.com/yasirkula/UnityNativeGallery/tree/master/.github/AAR%20Source%20(Android)/java/com/yasirkula/unity
    public static void RequestPermissions(final Activity context, final String permissionString, final IPermissionRequestCallback callback)
    {
        final ArrayList<String> permissions = new ArrayList<>();
        String[] list = permissionString.split("\\|");
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.M) {
            if (callback != null) {
                for (String permission: list) {
                    callback.onGranted(permission);
                }

                callback.onCompleted(true);
            }
            return;
        }

        boolean allGranted = true;
        for (String permission: list) {
            if (IsPermissionGranted(context, permission)) {
                if (callback != null)
                    callback.onGranted(permission);
                continue;
            }

            if (permissions.contains(permission))
                continue;

//            if (!context.shouldShowRequestPermissionRationale(permission))
//            {
//                if (callback != null)
//                    callback.onDeniedAlways(permission);
//
//                allGranted = false;
//                continue;
//            }

            permissions.add(permission);
        }

        int size = permissions.size();
        if (size <= 0)
        {
            if (callback != null)
                callback.onCompleted(allGranted);
            return;
        }

        final PermissionFragment request = new PermissionFragment();
        request.SetRequestCallback(new IPermissionRequestInternalCallback() {
            @Override
            public void onGranted(String permission) {
                if (callback == null) return;
                callback.onGranted(permission);
            }

            @Override
            public void onDenied(String permission) {
                if (callback == null) return;

                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
                    if (!context.shouldShowRequestPermissionRationale(permission))
                        callback.onDeniedAlways(permission);

                    return;
                }

                callback.onDenied(permission);
            }

            @Override
            public void onCompleted(boolean allGranted) {
                if (callback == null) return;
                callback.onCompleted(allGranted);
            }
        });

        Bundle bundle = new Bundle();
        final String[] permissionList = permissions.toArray(new String[permissions.size()]);
        bundle.putStringArray(PermissionFragment.PERMISSION_NAMES, permissionList);
        request.setArguments(bundle);
        FragmentTransaction fragmentTransaction = context.getFragmentManager().beginTransaction();
        fragmentTransaction.add(0, request);
        fragmentTransaction.commit();
    }

    public static void OpenSettings(Context context)
    {
        Uri uri = Uri.fromParts("package", context.getPackageName(), null);

        Intent intent = new Intent();
        intent.setAction(Settings.ACTION_APPLICATION_DETAILS_SETTINGS);
        intent.setData(uri);

        context.startActivity(intent);
    }

    public static int GetBatteryLevel(Context context)
    {
        int level = 0;
        try {
            IntentFilter ifilter = new IntentFilter(Intent.ACTION_BATTERY_CHANGED);
            Intent intent = context.registerReceiver(null, ifilter);
            level = intent.getIntExtra("level", 0);// 获得当前电量
        } catch (Exception e) {

        }
        return level;
    }

    public static boolean IsBatteryCharging(Context context)
    {
        boolean isCharging = false;
        try {
            IntentFilter ifilter = new IntentFilter(Intent.ACTION_BATTERY_CHANGED);
            Intent intent = context.registerReceiver(null, ifilter);
            int status = intent.getIntExtra("status", 0);// 电池充电状态
            if (status == BatteryManager.BATTERY_STATUS_CHARGING || status == BatteryManager.BATTERY_STATUS_FULL) {
                isCharging = true;
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
        return isCharging;
    }

    public static long GetVersionCode(Context context)
    {
        PackageManager manager = context.getPackageManager();

        try {
            PackageInfo info = manager.getPackageInfo(context.getPackageName(), 0);
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.P) {
                return info.getLongVersionCode();
            } else {
                return info.versionCode;
            }
        } catch (PackageManager.NameNotFoundException e) {
            e.printStackTrace();
            return 0;
        }
    }

    public static String GetDeviceId(Context context)
    {
        TelephonyManager manager = (TelephonyManager) context.getSystemService(Context.TELEPHONY_SERVICE);
        try {
            return manager.getDeviceId();
        } catch (Exception e) {
            e.printStackTrace();
            return "";
        }
    }

    public static String GetIMEIAt(Context context, int index)
    {
        TelephonyManager manager = (TelephonyManager) context.getSystemService(Context.TELEPHONY_SERVICE);
        try {
            Method method = manager.getClass().getMethod("getImei", int.class);
            String imei = (String) method.invoke(manager, index);
            return imei;
        } catch (Exception e) {
            e.printStackTrace();
        }
        return "";
    }

    public static String GetIMEI(Context context)
    {
        TelephonyManager manager = (TelephonyManager) context.getSystemService(Context.TELEPHONY_SERVICE);
        try {
            Method method = manager.getClass().getMethod("getImei", int.class);
            String imei1 = (String) method.invoke(manager, 0);
            String imei2 = (String) method.invoke(manager, 1);
//
//            String imei1 = TryInvokeGetImei(manager, method, 0);
//            String imei2 = TryInvokeGetImei(manager, method, 1);
            if(TextUtils.isEmpty(imei2)){
                return imei1;
            }
            if(!TextUtils.isEmpty(imei1)){
                //因为手机卡插在不同位置，获取到的imei1和imei2值会交换，所以取它们的最小值,保证拿到的imei都是同一个
                String imei = "";
                if(imei1.compareTo(imei2) <= 0){
                    imei = imei1;
                }else{
                    imei = imei2;
                }
                return imei;
            }
        } catch (Exception e) {
            e.printStackTrace();
            return "";
        }
        return "";

    }

    private static String TryInvokeGetImei(TelephonyManager manager, Method method, int val)
    {
        try {
            return (String) method.invoke(manager, val);
        } catch (Exception e) {
            e.printStackTrace();
            return "";
        }
    }

    public static void LogI(String msg) {
        Log.i(TAG, msg);
    }


    /**
     * https://stackoverflow.com/questions/17511070/android-force-crash-with-uncaught-exception-in-thread
     */
    public static void ForceCrash() {
        Exception e = new Exception("test crash");
        Thread main = Looper.getMainLooper().getThread();
        main.getUncaughtExceptionHandler().uncaughtException(main, e);
    }

    public static boolean OSAvailable(int version) {
        if (Build.VERSION.SDK_INT >= version) {
            return true;
        }

        return false;
    }
}
