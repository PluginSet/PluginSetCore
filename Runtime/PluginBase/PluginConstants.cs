namespace PluginSet.Core
{
    public static class PluginConstants
    {
        public const int SuccessCode = 0;
        public const int CancelCode = -1;
        public const int InvalidCode = 404;
        public const int FailDefaultCode = 9999;
        
        public const string NOTIFY_RESTART = "NOTIFY_RESTART";

        public const string NOTIFY_REPORT = "NOTIFY_REPORT";
        
        public const string NOTIFY_CHOOSE_BANNER_TYPE = "NOTIFY_CHOOSE_BANNER_TYPE";
        public const string NOTIFY_CHOOSE_LOAD_REWARD_AD_TYPE = "NOTIFY_CHOOSE_LOAD_REWARD_AD_TYPE";
        public const string NOTIFY_CHOOSE_SHOW_REWARD_AD_TYPE = "NOTIFY_CHOOSE_SHOW_REWARD_AD_TYPE";
        public const string NOTIFY_CHOOSE_INTERSTITIAL_AD_TYPE = "NOTIFY_CHOOSE_INTERSTITIAL_AD_TYPE";

        public const string NOTIFY_CHANNEL_CHANGED = "NOTIFY_CHANNEL_CHANGED";

        public const string NOTIFY_CHOOSE_LOGIN_TYPE = "NOTIFY_CHOOSELOGINTYPE";
        public const string NOTIFY_CHOOSE_LOGOUT_TYPE = "NOTIFY_CHOOSE_LOGOUT_TYPE";
        public const string NOTIFY_CHOOSE_PAYMENT_TYPE = "NOTIFY_CHOOSEPAYTYPE";
        
        public const string NOTIFY_CHOOSE_SHARE_TEXT_TYPE = "NOTIFY_CHOOSE_SHARE_TEXT_TYPE";
        public const string NOTIFY_CHOOSE_SHARE_IMAGE_TYPE = "NOTIFY_CHOOSE_SHARE_IMAGE_TYPE";
        public const string NOTIFY_CHOOSE_SHARE_MUSIC_TYPE = "NOTIFY_CHOOSE_SHARE_MUSIC_TYPE";
        public const string NOTIFY_CHOOSE_SHARE_MUSIC_URL_TYPE = "NOTIFY_CHOOSE_SHARE_MUSIC_URL_TYPE";
        public const string NOTIFY_CHOOSE_SHARE_VIDEO_TYPE = "NOTIFY_CHOOSE_SHARE_VIDEO_TYPE";
        public const string NOTIFY_CHOOSE_SHARE_VIDEO_URL_TYPE = "NOTIFY_CHOOSE_SHARE_VIDEO_URL_TYPE";
        public const string NOTIFY_CHOOSE_SHARE_WEBPAGE_TYPE = "NOTIFY_CHOOSE_SHARE_WEBPAGE_TYPE";
        public const string NOTIFY_CHOOSE_SHARE_MINIPROGRAM_TYPE = "NOTIFY_CHOOSE_SHARE_MINIPROGRAM_TYPE";
        public const string NOTIFY_MOMENT_FETCH_TYPE = "NOTIFY_MOMENT_FETCH_TYPE";
        public const string NOTIFY_MOMENT_OPEN_TYPE = "NOTIFY_MOMENT_OPEN_TYPE";
        public const string NOTIFY_MOMENT_OPEN_PAGE_TYPE = "NOTIFY_MOMENT_OPEN_PAGE_TYPE";
        public const string NOTIFY_MOMENT_TRY_CLOSE_MOMENT = "NOTIFY_MOMENT_TRY_CLOSE_MOMENT";
        public const string NOTIFY_MOMENT_CLOSE_MOMENT = "NOTIFY_MOMENT_CLOSE_MOMENT";
        public const string NOTIFY_TAPTAP_DB_USER = "NOTIFY_TAPTAP_DB_USER";
        public const string NOTIFY_TAPTAP_DB_LEVEL = "NOTIFY_TAPTAP_DB_LEVEL";

        public const string NOTIFY_PLUGIN_INIT_FAILED = "NOTIFY_PLUGIN_INIT_FAILED";

        public const string NOTIFY_LOGIN_STATUS_CHANGED = "NOTIFY_LOGIN_STATUS_CHANGED";
        
        public const string PROGRESS_UPDATED = "PROGRESS_UPDATED";

        //订单生成成功
        public const string NOTIFY_ORDER_SUCCESS = "NOTIFY_ORDER_SUCCESS";
        //支付成功
        public const string NOTIFY_PAY_SUCCESS = "NOTIFY_PAY_SUCCESS";
        public const string NOTIFY_PAY_FAIL = "NOTIFY_PAY_FAIL";

        public const string NOTIFY_ACCOUNT_LOGOUT = "NOTIFY_ACCOUNT_LOGOUT";
        public const string NOTIFY_ACCOUNT_SWITCH = "NOTIFY_ACCOUNT_SWITCH";

        public const string NOTIFY_APPLICATION_ENTER_BACKGROUND = "NOTIFY_APPLICATION_ENTER_BACKGROUND";
        //关闭APPLOVIN奖励广告
        public const string NOTIFY_APPLICATION_ENTER_FOREGROUND = "NOTIFY_APPLICATION_ENTER_FOREGROUND";
        
        public const string NOTIFY_APPLICATION_PAUSE_SOUNDS = "NOTIFY_APPLICATION_PAUSE_SOUNDS";
        public const string NOTIFY_APPLICATION_RESUME_SOUNDS = "NOTIFY_APPLICATION_RESUME_SOUNDS";

        public const string NOTIFY_ON_REWARD_LOAD_START = "NOTIFY_ON_REWARD_LOAD_START";
        public const string NOTIFY_ON_REWARD_LOAD_SUCCESS = "NOTIFY_ON_REWARD_LOAD_SUCCESS";
        public const string NOTIFY_ON_REWARD_LOAD_FAIL = "NOTIFY_ON_REWARD_LOAD_FAIL";
        public const string NOTIFY_ON_REWARD_LOAD_CACHED = "NOTIFY_ON_REWARD_LOAD_CACHED";
        public const string NOTIFY_ON_REWARD_AD_CLICK = "NOTIFY_ON_REWARD_AD_CLICK";
        public const string NOTIFY_ON_REWARD_AD_DISPLAY = "NOTIFY_ON_REWARD_AD_DISPLAY";
        public const string NOTIFY_ON_REWARD_AD_PAUSED = "NOTIFY_ON_REWARD_AD_PAUSED";
        public const string NOTIFY_ON_REWARD_AD_RESUMED = "NOTIFY_ON_REWARD_AD_RESUMED";
        public const string NOTIFY_ON_REWARD_AD_COMPLETED = "NOTIFY_ON_REWARD_AD_COMPLETED";

        // SDK SPECIAL
        public const string PATCH_VERSION_STRING = "PATCH_VERSION_STRING";
        public const string PATCH_DOWNLOAD_APP_URL = "PATCH_DOWNLOAD_APP_URL";
        public const string PATCH_UPDATE_PATCH_URL = "PATCH_UPDATE_PATCH_URL";
        public const string PATCH_STREAMING_URL = "PATCH_STREAMING_URL";
        public const string PATCH_NOTIFY_CHECK_UPDATE = "PATCH_NOTIFY_CHECK_UPDATE";
        public const string PATCH_NOTIFY_UPDATE_START = "PATCH_NOTIFY_UPDATE_START";
        public const string PATCH_NOTIFY_UPDATE_COMPLETE = "PATCH_NOTIFY_UPDATE_COMPLETE";
        public const string PATCH_NOTIFY_UPDATE_PROGRESS = "PATCH_NOTIFY_UPDATE_PROGRESS";
        public const string PATCH_NOTIFY_REQUEST_DOWNLOAD_PATCHES = "PATCH_NOTIFY_REQUEST_DOWNLOAD_PATCHES";
        public const string PATCH_NOTIFY_REQUEST_DOWNLOAD_APP = "PATCH_NOTIFY_REQUEST_DOWNLOAD_APP";
        public const string PATCH_NOTIFY_NET_ERROR = "PATCH_NOTIFY_NET_ERROR";

        public const string PLUGIN_INIT_ERROR = "PLUGIN_INIT_ERROR";

        /// <summary>
        /// 事件接收一个List<IPaymentProduct>参数
        /// </summary>
        public const string IAP_ON_INIT_SUCCESS = "IAP_ON_INIT_SUCCESS";
        
        /// <summary>
        /// 事件接收一个错误字符串
        /// </summary>
        public const string IAP_ON_INIT_FAILED = "IAP_ON_INIT_FAILED";
        
        /// <summary>
        /// 事件接收一个Result参数，参数数据与支付成功回调数据相同
        /// </summary>
        public const string IAP_CALLBACK_LOST_PAYMENTS = "IAP_CALLBACK_LOST_PAYMENTS";

        //同意隐私协议
        public const string NOTIFY_AGREE_PRIVACY = "NOTIFY_AGREE_PRIVACY";

        public const string CHAT_LOGIN            = "CHAT_LOGIN";
        public const string CHAT_LOGOUT           = "CHAT_LOGOUT";
        public const string CHAT_NETWORK_ONLINE   = "CHAT_NETWORK_ONLINE";
        public const string CHAT_NETWORK_OFFLINE  = "CHAT_NETWORK_OFFLINE";

        public const string FRIEND_PREPARED = "FRIEND_PREPARED";
        public const string FRIEND_ADD_FRIEND = "FRIEND_ADD_FRIEND";
        public const string FRIEND_DELETE_FRIEND = "FRIEND_DELETE_FRIEND";
        public const string FRIEND_UPDATE_PROFILE = "FRIEND_UPDATE_PROFILE";
        public const string FRIEND_ADD_REQUEST = "FRIEND_ADD_REQUEST";
        public const string FRIEND_REQUEST_DELETE = "FRIEND_REQUEST_DELETE";
    }
}
