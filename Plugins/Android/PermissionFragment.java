package com.pluginset.devices;

import android.app.Fragment;
import android.app.FragmentTransaction;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.os.Build;
import android.os.Bundle;
import android.util.Log;

// Credit: https://github.com/Over17/UnityAndroidPermissions/blob/0dca33e40628f1f279decb67d901fd444b409cd7/src/UnityAndroidPermissions/src/main/java/com/unity3d/plugin/UnityAndroidPermissions.java
public class PermissionFragment extends Fragment
{
    public static final String PERMISSION_NAMES    = "PermissionNames";

    private static final String TAG = "PermissionFragment";

    private static int REQUEST_CODE = 12345;

    private IPermissionRequestInternalCallback m_RequestCallback;
    private int m_RequestCode;

    public PermissionFragment()
    {
        m_RequestCode = REQUEST_CODE++;
    }

    public void SetRequestCallback(IPermissionRequestInternalCallback callback)
    {
        m_RequestCallback = callback;
    }

    @Override public void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);

        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.M) {
            Log.e(TAG, "Please check sdk int first");
            assert false;
            return;
        }

        String[] permissionNames = getArguments().getStringArray(PERMISSION_NAMES);
        if (permissionNames.length <= 0) {
            Log.e(TAG, "Please check permissions count first");
            assert false;
            return;
        }

        requestPermissions(permissionNames, m_RequestCode);
    }

    @Override public void onRequestPermissionsResult(int requestCode, String[] permissions, int[] grantResults)
    {
        if (requestCode != m_RequestCode)
            return;

        if (m_RequestCallback != null)
        {
            boolean allGranted = true;
            for (int i = 0; i < permissions.length && i < grantResults.length; ++i)
            {
                if (grantResults[i] == PackageManager.PERMISSION_GRANTED){
                    m_RequestCallback.onGranted(permissions[i]);
                } else {
                    allGranted = false;
                    m_RequestCallback.onDenied(permissions[i]);
                }
            }

            m_RequestCallback.onCompleted(allGranted);
        }

        FragmentTransaction fragmentTransaction = getFragmentManager().beginTransaction();
        fragmentTransaction.remove(this);
        fragmentTransaction.commit();


        // Resolves a bug in Unity 2019 where the calling activity
        // doesn't resume automatically after the fragment finishes
        // Credit: https://stackoverflow.com/a/12409215/2373034
        // Credit: https://github.com/yasirkula/UnityNativeGallery/tree/master/.github/AAR%20Source%20(Android)/java/com/yasirkula/unity
        try
        {
            Intent resumeUnityActivity = new Intent( getActivity(), getActivity().getClass() );
            resumeUnityActivity.setFlags( Intent.FLAG_ACTIVITY_REORDER_TO_FRONT );
            getActivity().startActivityIfNeeded( resumeUnityActivity, 0 );
        }
        catch( Exception e )
        {
            Log.e( "Unity", "Exception (resume):", e );
        }
    }
}
