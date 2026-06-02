#if ENABLE_PUSH_MODULE
using System;
using System.Collections.Generic;
using PluginSet.Core.MiniJSON;
using UnityEngine;
using System.Runtime.InteropServices;

namespace PluginSet.Core
{
    internal static class DeviceNotificationNative
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        private static AndroidJavaClass _notification;

        private static AndroidJavaClass Notification
        {
            get
            {
                if (_notification == null)
                    _notification = new AndroidJavaClass("com.pluginset.devices.DevicesNotification");
                return _notification;
            }
        }
#endif

        public static void ConfigureChannel(string channelId, string channelName, bool requestPermission)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Notification.CallStatic("configureChannel", channelId, channelName, requestPermission);
#endif
        }

        public static void RequestPostNotificationPermission()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Notification.CallStatic("requestPostNotificationPermission", AndroidHelper.CurrentActivity);
#elif UNITY_IOS && !UNITY_EDITOR
            iOSHelper._PluginSetNotificationRequestPermission();
#endif
        }

        public static bool CanScheduleExactAlarms()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return Notification.CallStatic<bool>("canScheduleExactAlarms");
#else
            return true;
#endif
        }

        public static void OpenExactAlarmSettings()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Notification.CallStatic("openExactAlarmSettings", AndroidHelper.CurrentActivity);
#endif
        }

        public static bool AreNotificationsEnabled()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return Notification.CallStatic<bool>("areNotificationsEnabled");
#elif UNITY_IOS && !UNITY_EDITOR
            return true;
#else
            return true;
#endif
        }

        public static void CreateNotificationChannel(string channelId, string channelName)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Notification.CallStatic("createNotificationChannel", channelId, channelName);
#endif
        }

        public static void SetBadgeNumber(int count)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Notification.CallStatic("setBadgeNumber", count);
#elif UNITY_IOS && !UNITY_EDITOR
            iOSHelper._PluginSetNotificationSetBadgeNumber(count);
#endif
        }

        public static int GetBadgeNumber()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return Notification.CallStatic<int>("getBadgeNumber");
#elif UNITY_IOS && !UNITY_EDITOR
            return iOSHelper._PluginSetNotificationGetBadgeNumber();
#else
            return 0;
#endif
        }

        public static void RemoveDeliveredNotifications(string[] identifiers)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Notification.CallStatic("removeDeliveredNotifications", identifiers ?? Array.Empty<string>());
#endif
        }

        public static void RemoveAllDeliveredNotifications()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Notification.CallStatic("removeAllDeliveredNotifications");
#elif UNITY_IOS && !UNITY_EDITOR
            iOSHelper._PluginSetNotificationRemoveAllDelivered();
#endif
        }

        public static string ConsumeInitialNotificationJson()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return Notification.CallStatic<string>("consumeInitialNotificationJson") ?? string.Empty;
#elif UNITY_IOS && !UNITY_EDITOR
            var ptr = iOSHelper._PluginSetNotificationConsumeInitialJson();
            if (ptr == IntPtr.Zero)
                return string.Empty;
            var json = Marshal.PtrToStringAnsi(ptr);
            iOSHelper._PluginSetNotificationFreeCString(ptr);
            return json ?? string.Empty;
#else
            return string.Empty;
#endif
        }

        public static void PostLocalNotification(LocalNotificationRequest request)
        {
            if (request == null)
                return;

            var dataJson = SerializeNotificationData(request.Data);
            var actionId = request.ActionId ?? string.Empty;

#if UNITY_ANDROID && !UNITY_EDITOR
            Notification.CallStatic(
                "postLocalNotification",
                AndroidHelper.CurrentActivity,
                request.NotificationId ?? string.Empty,
                request.ChannelId ?? string.Empty,
                request.Title ?? string.Empty,
                request.Body ?? string.Empty,
                request.DelaySeconds,
                dataJson,
                actionId);
#elif UNITY_IOS && !UNITY_EDITOR
            iOSHelper._PluginSetNotificationPostLocal(
                request.NotificationId ?? string.Empty,
                request.ChannelId ?? string.Empty,
                request.Title ?? string.Empty,
                request.Body ?? string.Empty,
                request.DelaySeconds,
                dataJson,
                actionId);
#endif
        }

        private static string SerializeNotificationData(Dictionary<string, object> data)
        {
            if (data == null || data.Count == 0)
                return "{}";
            return Json.Serialize(data);
        }

        public static void CancelLocalNotification(string notificationId)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Notification.CallStatic("cancelLocalNotification", notificationId ?? string.Empty);
#elif UNITY_IOS && !UNITY_EDITOR
            iOSHelper._PluginSetNotificationCancelLocal(notificationId ?? string.Empty);
#endif
        }

        public static void CancelAllPendingLocalNotifications()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Notification.CallStatic("cancelAllPendingLocalNotifications");
#elif UNITY_IOS && !UNITY_EDITOR
            iOSHelper._PluginSetNotificationCancelAllPendingLocal();
#endif
        }
    }
}
#endif
