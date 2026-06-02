using PluginSet.Core.MiniJSON;

namespace PluginSet.Core
{
    /// <summary>
    /// 系统通知栏能力（权限、角标、已送达通知、冷启动点击缓存等），与 FCM/APNs 等远程推送 SDK 无关。
    /// 由构建宏 <c>ENABLE_PUSH_MODULE</c>（渠道 IncludePushModule）控制，未启用时为无操作存根。
    /// </summary>
    public static partial class Devices
    {
#if ENABLE_PUSH_MODULE
        public static NotificationResponse GetInitialNotification()
        {
            var json = DeviceNotificationNative.ConsumeInitialNotificationJson();
            return ParseNotificationResponse(json);
        }

        public static bool RequestNotificationPermissions()
        {
            // Android/iOS 都以同步返回 bool 的方式给上层调用方判断授权结果。
#if UNITY_ANDROID && !UNITY_EDITOR
            return AndroidHelper.RequestPermissions("android.permission.POST_NOTIFICATIONS");
#elif UNITY_IOS && !UNITY_EDITOR
            return iOSHelper._PluginSetNotificationRequestPermission();
#elif UNITY_WEBGL && !UNITY_EDITOR
            // WebGL 无原生通知权限模型，这里按已授权处理以避免阻塞业务流程。
            return true;
#else
            return true;
#endif
        }

        /// <summary>
        /// Android 12+：是否已允许「闹钟和提醒」精确闹钟。未授权时延迟本地通知可能被系统推迟或无法触发。
        /// </summary>
        public static bool CanScheduleExactAlarms()
        {
            return DeviceNotificationNative.CanScheduleExactAlarms();
        }

        /// <summary>
        /// Android 12+：打开系统「闹钟和提醒」设置页。
        /// </summary>
        public static void OpenExactAlarmSettings()
        {
            DeviceNotificationNative.OpenExactAlarmSettings();
        }

        /// <summary>
        /// 应用通知总开关是否开启（Android 7+ / iOS 恒为 true）。
        /// </summary>
        public static bool AreNotificationsEnabled()
        {
            return DeviceNotificationNative.AreNotificationsEnabled();
        }

        public static void SetBadgeNumber(int count)
        {
            DeviceNotificationNative.SetBadgeNumber(count);
        }

        public static int GetBadgeNumber()
        {
            return DeviceNotificationNative.GetBadgeNumber();
        }

        public static void RemoveDeliveredNotifications(params string[] identifiers)
        {
            DeviceNotificationNative.RemoveDeliveredNotifications(identifiers);
        }

        public static void RemoveAllDeliveredNotifications()
        {
            DeviceNotificationNative.RemoveAllDeliveredNotifications();
        }

        public static void CreateNotificationChannel(string channelId, string channelName, bool requestPermissionOnAndroid = true)
        {
            DeviceNotificationNative.ConfigureChannel(channelId, channelName, requestPermissionOnAndroid);
            DeviceNotificationNative.CreateNotificationChannel(channelId, channelName);
        }

        public static void CreateNotificationChannelFromJson(string config)
        {
            if (string.IsNullOrEmpty(config))
                return;

            var dict = Json.DeserializeObject(config);
            if (dict.IsNull)
                return;

            var channelId = dict.GetString("channelId");
            var channelName = dict.GetString("channelName");
            if (string.IsNullOrEmpty(channelId))
                return;

            var requestPermission = true;
            var raw = Json.Deserialize(config) as System.Collections.Generic.Dictionary<string, object>;
            if (raw != null && raw.TryGetValue("requestPermission", out var rpVal))
            {
                if (rpVal is bool b)
                    requestPermission = b;
                else
                    bool.TryParse(rpVal.ToString(), out requestPermission);
            }

            CreateNotificationChannel(
                channelId,
                string.IsNullOrEmpty(channelName) ? channelId : channelName,
                requestPermission);
        }

        public static void PostLocalNotification(LocalNotificationRequest request)
        {
            DeviceNotificationNative.PostLocalNotification(request);
        }

        public static void CancelLocalNotification(string notificationId)
        {
            DeviceNotificationNative.CancelLocalNotification(notificationId);
        }

        public static void CancelAllPendingLocalNotifications()
        {
            DeviceNotificationNative.CancelAllPendingLocalNotifications();
        }
#else
        public static NotificationResponse GetInitialNotification() => null;

        public static bool RequestNotificationPermissions()
        {
            return true;
        }

        public static bool CanScheduleExactAlarms() => true;

        public static void OpenExactAlarmSettings()
        {
        }

        public static bool AreNotificationsEnabled() => true;

        public static void SetBadgeNumber(int count)
        {
        }

        public static int GetBadgeNumber() => 0;

        public static void RemoveDeliveredNotifications(params string[] identifiers)
        {
        }

        public static void RemoveAllDeliveredNotifications()
        {
        }

        public static void CreateNotificationChannel(string channelId, string channelName, bool requestPermissionOnAndroid = true)
        {
        }

        public static void CreateNotificationChannelFromJson(string config)
        {
        }

        public static void PostLocalNotification(LocalNotificationRequest request)
        {
        }

        public static void CancelLocalNotification(string notificationId)
        {
        }

        public static void CancelAllPendingLocalNotifications()
        {
        }
#endif

        private static NotificationResponse ParseNotificationResponse(string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;

            var root = Json.DeserializeObject(json);
            if (root.IsNull)
                return null;

            var raw = Json.Deserialize(json) as System.Collections.Generic.Dictionary<string, object>;
            System.Collections.Generic.Dictionary<string, object> data = null;
            if (raw != null && raw.TryGetValue("data", out var dataObj)
                && dataObj is System.Collections.Generic.Dictionary<string, object> dataDict)
            {
                data = new System.Collections.Generic.Dictionary<string, object>(dataDict);
            }

            return new NotificationResponse
            {
                Message = new PushMessage
                {
                    Title = root.GetString("title"),
                    Body = root.GetString("body"),
                    Data = data ?? new System.Collections.Generic.Dictionary<string, object>(),
                },
                ActionId = root.GetString("actionId"),
                IsInitialLaunch = ParseIsInitialLaunch(raw),
            };
        }

        private static bool ParseIsInitialLaunch(System.Collections.Generic.Dictionary<string, object> raw)
        {
            if (raw == null || !raw.TryGetValue("isInitialLaunch", out var value))
                return false;
            if (value is bool b)
                return b;
            return bool.TryParse(value.ToString(), out var parsed) && parsed;
        }
    }
}
