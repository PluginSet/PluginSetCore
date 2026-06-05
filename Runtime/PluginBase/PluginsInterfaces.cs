using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace PluginSet.Core
{
    /// <summary>
    /// 通用SDK调用结果<br/>
    /// 用户信息建议：<br/>
    ///     string userId<br/>
    ///     string nickName<br/>
    ///     string avatarUrl<br/>
    /// 支付信息建议：<br/>
    ///     string productId - 产品编号 a.k.a SKU<br/>
    ///     string productName - 显示名称<br/>
    ///     string transactionId - 交易号<br/>
    ///     string currency - ISO 4217<br/>
    ///     int price - 单位（分）<br/>
    /// </summary>
    public class CommonResult<T>
    {
        public bool Success;
        public int Code;
        public string Error;
        public string PluginName;
        public string Data;
        public T DataObject;
    }
    
    public class Result: CommonResult<object>
    {
    }

    public struct ReportError
    {
        public string Condition;
        public string StackTrack;
        public string ThreadName;
        public LogType LogType;
    }

    public enum BannerPosition
    {
        TopLeft,
        TopCenter,
        TopRight,
        Centered,
        CenterLeft,
        CenterRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    public enum ChangeAdIdMode
    {
        None,
        ChangeWhenFail,
        ChangeWhenDisplay,
    }

    public abstract class IPaymentProduct
    {
        public abstract bool AvailableToPurchase { get; }
        public abstract string ProductId { get; }
        public abstract ProductType Type { get; }
        public abstract int Price { get; }
        public abstract string Currency { get; }
        public abstract string PriceString { get; }
        public abstract string Title { get; }
        public abstract string Description { get; }
    }

    public interface IPluginBase
    {
        string Name { get; }
    }

    public interface IStartPlugin : IPluginBase
    {
        int StartOrder { get; }

        bool IsRunning { get; }

        IEnumerator StartPlugin();

        void DisposePlugin(bool isAppQuit = false);
    }

    public interface IAnalytics : IPluginBase
    {
        void CustomEvent(string customEventName, Dictionary<string, object> eventData = null);
    }
    
    public interface IPageAnalytics : IPluginBase
    {
        void PageStart(string pageName, string pageClassOverride);

        void PageEnd(string pageName);
    }

    public interface IReport : IPluginBase
    {
        void OnUserRegister(string method = "Indefinite");

        void OnUserRealName();

        void OnUserPurchase(bool success, string currency, float price, string paymentMethod
            , int amount = 1, string productId = null, string productName = null, string productType = null);
    }

    public interface IUserSet : IPluginBase
    {
        /// <summary>
        /// 设置玩家信息
        /// </summary>
        /// <param name="isNewUser">是否是新玩家</param>
        /// <param name="userId">玩家ID</param>
        /// <param name="pairs" desc = "玩家信息数据对，通用字段名称定义如下">
        ///   <key name = "serverID">服务器ID（数字字符串）</key>
        ///   <key name = "serverName">服务器名称</key>
        ///   <key name = "gameRoleName">角色名称</key>
        ///   <key name = "gameRoleID">角色ID</key>
        ///   <key name = "gameRoleBalance">角色用户余额</key>
        ///   <key name = "gameRoleLevel">角色用户等级</key>
        ///   <key name = "partyName">公会社团</key>
        ///   <key name = "roleCreateTime">角色创建时间(10位数的unix timestamp时间戳)</key>
        ///   <key name = "partyId">帮派id</key>
        ///   <key name = "gameRoleGender">角色性别</key>
        ///   <key name = "gameRolePower">战力</key>
        ///   <key name = "partyRoleId">角色在帮派中的id</key>
        ///   <key name = "partyRoleName">角色在帮派中的名称</key>
        ///   <key name = "professionId">角色职业id</key>
        ///   <key name = "profession">角色职业名称</key>
        ///   <key name = "friendList">好友关系列表</key>
        /// </param>
        void SetUserInfo(bool isNewUser, string userId, Dictionary<string, object> pairs = null);
        
        void ClearUserInfo();
        void FlushUserInfo();
    }

    public interface ILoginPlugin : IPluginBase
    {
        bool IsEnableLogin { get; }

        /// <summary>
        /// Is the user logged in?
        /// </summary>
        bool IsLoggedIn { get; }

        void Login(Action<Result> callback = null, string json = null);

        void Logout(Action<Result> callback = null, string json = null);

        /// <summary>
        /// Get user info json representation.
        /// </summary>
        /// <returns></returns>
        string GetUserLoginData();
    }
    
    public enum ProductType
    {
        /// <summary>
        /// Consumables may be purchased more than once.
        ///
        /// Purchase history is not typically retained by store
        /// systems once consumed.
        /// </summary>
        Consumable,

        /// <summary>
        /// Non consumables cannot be repurchased and are owned indefinitely.
        /// </summary>
        NonConsumable,

        /// <summary>
        /// Subscriptions have a finite window of validity.
        /// </summary>
        Subscription
    }

    public class IAPInitResult: CommonResult<Dictionary<string, IPaymentProduct>>
    {
        
    }

    [Serializable]
    public class PurchaseCallback
    {
        [SerializeField]
        public string transactionId;
        [SerializeField]
        public string productId;
        [SerializeField]
        public string callback;
    }

    public class PurchaseResult : CommonResult<PurchaseCallback> {}

    public interface IPaymentPlugin : IPluginBase
    {
        bool IsEnablePayment { get; }

        /// <summary>
        /// 开始购买
        /// </summary>
        /// <param name="productId">商品id</param>
        /// <param name="callback">购买回调</param>
        /// <param name="jsonData"></param>
        void Pay(string productId, Action<PurchaseResult> callback = null, string jsonData = null);
    }

    public interface IIAPurchasePlugin : IPaymentPlugin
    {
        void InitWithProducts(Dictionary<string, ProductType> products, Action<IAPInitResult> callback = null);

        void PaymentComplete(string transactionId, Action<Result> callback = null);

        void RestorePayments(Action<Result> callback = null, string json = null);
    }

    public interface ISharePlugin : IPluginBase
    {
        bool IsEnableShare { get; }
    }

    public interface IShareTextPlugin : ISharePlugin
    {
        void ShareText(string text, Action success = null, Action fail = null
            , string title = null, string image = null);
    }

    public interface IShareImagePlugin : ISharePlugin
    {
        void ShareImage(string imagePath, Action success = null, Action fail = null
            , string title = null, string desc = null, string image = null);
    }

    public interface IShareImageUrlPlugin : ISharePlugin
    {
        void ShareImageUrl(string imageUrl, Action success = null, Action fail = null
            , string title = null, string desc = null, string image = null);
    }

    public interface IShareMusicPlugin : ISharePlugin
    {
        void ShareMusic(string musicFile, Action success = null, Action fail = null
            , string title = null, string desc = null, string image = null);
    }

    public interface IShareMusicUrlPlugin : ISharePlugin
    {
        void ShareMusicUrl(string musicUrl, Action success = null, Action fail = null
            , string title = null, string desc = null, string image = null);
    }

    public interface IShareVideoPlugin : ISharePlugin
    {
        void ShareVideo(string videoFile, Action success = null, Action fail = null
            , string extra = null, string title = null, string desc = null, string image = null);
    }

    public interface IShareVideoUrlPlugin : ISharePlugin
    {
        void ShareVideoUrl(string videoUrl, Action success = null, Action fail = null, string extra = null
            , string title = null, string desc = null, string image = null);
    }

    public interface IShareWebPagePlugin : ISharePlugin
    {
        void ShareWebPage(string webUrl, Action success = null, Action fail = null
            , string title = null, string desc = null, string image = null);
    }

    public interface IShareMiniProgramPlugin : ISharePlugin
    {
        void ShareMiniProgram(string url, string path, string appId, Action success = null, Action fail = null
            , string title = null, string desc = null, string image = null);
    }
    public interface IBannerAdPlugin : IPluginBase
    {
        bool IsEnableShowBanner { get; }

        void ShowBannerAd(string adId, BannerPosition position = BannerPosition.BottomCenter,
            Dictionary<string, object> extensions = null);

        void HideBannerAd(string adId);

        void HideAllBanners();
    }

    public enum AdErrorCode
    {
        Success = 0,
        IsLoading = 1,
        IsShowing = 2,
        NotLoaded = 3,
        ShowFail  = 4,
        Dismissed = 5,
    }

    public struct AdInfo
    {
        public static AdInfo Null = new AdInfo() { };
        
        public string AdUnitId;
        public string Plugin;
        public double Revenue;
        public string RevenuePrecision;
    }

    public interface IOpenAdPlugin : IPluginBase
    {
        bool IsEnableShowOpenAd { get; }
        
        bool IsReadyToShowOpenAd { get; }

        void LoadOpenAd(Action success = null, Action<int> fail = null, Dictionary<string, object> extParams = null);
        
        void ShowOpenAd(Action<bool, int> dismiss = null);

        AdInfo GetLoadedOpenAdInfo();
    }

    public interface IRewardAdPlugin : IPluginBase
    {
        bool IsEnableShowRewardAd { get; }

        bool IsReadyToShowRewardAd { get; }

        void LoadRewardAd(Action success = null, Action<int> fail = null);

        void ShowRewardAd(Action<bool, int> dismiss = null);
        
        AdInfo GetLoadedRewardAdInfo();
    }

    public interface IInterstitialAdPlugin : IPluginBase
    {
        bool IsEnableShowInterstitialAd { get; }

        bool IsReadyToShowInterstitialAd { get; }

        void LoadInterstitialAd(Action success = null, Action<int> fail = null);

        void ShowInterstitialAd(Action<bool, int> dismiss = null);
        
        AdInfo GetLoadedInterstitialAdInfo();
    }

    /// <summary>
    /// 合规性
    /// </summary>
    public interface IRegulationPlugin : IPluginBase
    {
        /// <summary>
        /// 是否允许私域流量信息显示
        /// </summary>
        bool AllowPrivateTraffic { get; }

        bool ShowTosOnLoginScreen { get; }

        /// <summary>
        /// 是否允许游客登录
        /// </summary>
        bool AllowGuestLogin { get; }
    }

    public interface IRealNamePlugin : IPluginBase
    {
        bool IsEnableVerifyRealName { get; }

        void VerifyRealName(Action<Result> callback = null);
    }

    public interface IExitPlugin : IPluginBase
    {
        bool HasExitDialog { get; }
        
        void ExitApplication();
    }

    /// <summary>
    /// taptap 动态页 内嵌网页(sdk实现)
    /// </summary>
    public interface IMomentPlugin : IPluginBase
    {
        bool IsEnableShowMoment { get; }
        bool IsReadyToShowMoment { get; }

        void MomentFetch();
        void MomentOpen();
        void MomentOpenPage(string page);
        void MomentTryCloseMomentConfirmWindow(string tips, string content);
        void MomentClose();
    }

    public interface IChannel : IPluginBase
    {
        /// <summary>
        /// 渠道使用的优先级，数值越高，优先级越高
        /// </summary>
        int ChannelPriority { get; }
        
        /// <summary>
        /// 返回渠道名称，如果返回空则无效
        /// </summary>
        /// <returns></returns>
        string GetChannelName();

        /// <summary>
        /// 获取渠道ID，返回值仅大于等于0时有效
        /// </summary>
        /// <returns>当无渠道ID时返回小于0的值</returns>
        int GetChannelId();
    }

    public interface IStore : IPluginBase
    {
        /// <summary>
        /// 评价
        /// </summary>
        void Review();
    }

    /// <summary>
    /// 推送消息体（与具体商店 SDK 无关的通用结构）。
    /// </summary>
    public class PushMessage
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public Dictionary<string, object> Data { get; set; }
    }

    /// <summary>
    /// 本地通知请求（由应用在本地调度，不经过 FCM/APNs）。
    /// </summary>
    public class LocalNotificationRequest
    {
        /// <summary>业务侧唯一 ID，用于取消；为空时由系统生成。</summary>
        public string NotificationId;

        /// <summary>Android 通知渠道 ID；为空时使用默认渠道。</summary>
        public string ChannelId;

        public string Title;
        public string Body;

        /// <summary>延迟秒数；0 表示立即展示。</summary>
        public int DelaySeconds;

        /// <summary>
        /// 通知动作 ID（如按钮/分类标识）；点击通知后在 <see cref="NotificationResponse.ActionId"/> 中返回。
        /// </summary>
        public string ActionId;

        /// <summary>
        /// 自定义键值，点击通知后在 <see cref="NotificationResponse.Message.Data"/> 中返回（不含 title/body/actionId）。
        /// </summary>
        public Dictionary<string, object> Data;
    }

    /// <summary>
    /// 用户点击通知后的响应信息。
    /// </summary>
    public class NotificationResponse
    {
        public PushMessage Message { get; set; }
        public string ActionId { get; set; }
        public bool IsInitialLaunch { get; set; }
    }

    /// <summary>
    /// <see cref="PluginConstants.NOTIFY_PUSH_TOKEN_REFRESH"/> 事件载荷（各推送插件统一结构）。
    /// </summary>
    public class PushTokenRefreshEventData
    {
        public string PluginName { get; set; }
        public string Token { get; set; }
        public string UserId { get; set; }
        public string Platform { get; set; }

        public static PushTokenRefreshEventData Create(string pluginName, string token, string userId = null)
        {
            return new PushTokenRefreshEventData
            {
                PluginName = pluginName ?? string.Empty,
                Token = token ?? string.Empty,
                UserId = userId ?? string.Empty,
                Platform = Application.platform.ToString(),
            };
        }
    }

    /// <summary>
    /// <see cref="PluginConstants.NOTIFY_PUSH_REGISTER_READY"/> 事件载荷（各推送插件统一结构）。
    /// </summary>
    public class PushRegisterEventData
    {
        public string PluginName { get; set; }
        public string UserId { get; set; }
        public string Token { get; set; }
        public string Platform { get; set; }

        /// <summary>
        /// true：绑定用户与 Token；false：登出解绑（UserId/Token 为空）。
        /// </summary>
        public bool IsRegister { get; set; }

        public static PushRegisterEventData CreateRegister(string pluginName, string userId, string token)
        {
            return new PushRegisterEventData
            {
                PluginName = pluginName ?? string.Empty,
                UserId = userId ?? string.Empty,
                Token = token ?? string.Empty,
                Platform = Application.platform.ToString(),
                IsRegister = true,
            };
        }

        public static PushRegisterEventData CreateUnregister(string pluginName)
        {
            return new PushRegisterEventData
            {
                PluginName = pluginName ?? string.Empty,
                UserId = string.Empty,
                Token = string.Empty,
                Platform = Application.platform.ToString(),
                IsRegister = false,
            };
        }
    }

    /// <summary>
    /// 远程推送 SDK 能力（FCM / APNs Token、主题订阅等），由各商店插件实现并由 PluginsManager 转发。
    /// </summary>
    public interface IRemotePushNotificationPlugin : IPluginBase
    {
        bool IsEnablePush { get; }

        bool HasPushToken { get; }

        string PushToken { get; }

        void SubscribeToTopic(string topic);

        void UnsubscribeFromTopic(string topic);
    }

    /// <summary>云端玩家数据存储能力（后端无关：Firestore / PlayFab / REST 等均可实现）。</summary>
    public interface ICloudDataStorageModule
    {
        /// <summary>覆盖写入整个文档。uniqId 为业务侧玩家唯一键（非 SetUserInfo 的 userId）；path 为逻辑路径，如 "player/profile"。</summary>
        void SetData(string uniqId, string path, string json, string revision, Action<Result> callback = null);

        /// <summary>读取整个文档；不存在时返回 IsNotFound。</summary>
        void GetData(string uniqId, string path, Action<Result> callback);

        /// <summary>删除文档。</summary>
        void DeleteData(string uniqId, string path, Action<Result> callback = null);
    }

    public interface ICloudDataStoragePlugin : IPluginBase, ICloudDataStorageModule
    {
    }

    /// <summary>
    /// 远程配置能力。SetDefault 与 GetConfig 必须使用同一配置结构类型。
    /// </summary>
    public interface IRemoteConfigPlugin : IPluginBase
    {
        void SetDefault<T>(T config) where T : class, new();

        T GetConfig<T>() where T : class, new();
    }

    /// <summary>
    /// 其实可以用事件代替，但想想还是加一个更方便使用
    /// </summary>
    public interface ICustomPlugin : IPluginBase
    {
        void CustomCall(string func, Action<Result> callback = null, string json = null);
    }

    /// <summary>
    /// 隐私授权回调响应插件，隐私授权完成后调用该插件方法响应所有插件的事务，其实也可以用内置事件，但独立成插件似乎更好用，尽量让外部少使用事件调用SDK
    /// </summary>
    public interface IPrivacyAuthorizationCallback : IPluginBase
    {
        void OnPrivacyAuthorization();
    }
    
    public interface IMultipleLanguage : IPluginBase
    {
        LanguageType CurrentLanguage { get; }
        
        LanguageType[] ValidLanguages { get; }

        string GetLanguageFlag(LanguageType lang);

        /// <summary>
        /// change language and return is changed
        /// </summary>
        /// <param name="language"></param>
        /// <returns>changed</returns>
        bool SetCurrentLanguage(LanguageType language);
    }

    public interface ILanguageObserver : IPluginBase
    {
        IEnumerator OnLanguageChanged();
    }

    public interface IChatBase : IPluginBase
    {
        void LoginChat(string json, Action<Result> callback = null);

        void LogoutChat(Action<Result> callback = null);

        void AddNewMessageReceiveCallback(Action<Result> callback);
        
        void RemoveNewMessageReceiveCallback(Action<Result> callback);

        void SetMessageLocalData(string messageId, int? intData = null, string stringData = null,
            Action<Result> callback = null);
    }

    public interface IChatConv : IChatBase
    {
        void AddNewConvCallback(Action<Result> callback);
        
        void RemoveNewConvCallback(Action<Result> callback);
        
        void DeleteConv(string convId, Action<Result> callback = null);
        
        void GetConvList(Action<Result> callback);

        string SendConvTextMessage(string userId, string content, string extra = null, Action<Result> callback = null);
        
        string SendConvSoundMessage(string userId, string soundPath, int duration, string extra = null, Action<Result> callback = null);

        string SendConvCustomMessage(string userId, string customType, string content, string extra = null, Action<Result> callback = null);

        void CancelSendConvMessage(string userId, string messageId, Action<Result> callback = null);
        
        void RevokeConvMessage(string userId, string messageId, Action<Result> callback = null);

        void DeleteConvMessage(string userId, string messageId, Action<Result> callback = null);

        void GetConvMessageList(string userId, Action<Result> callback);
        
        void GetConvMessageList(string userId, uint maxCount, Action<Result> callback);
        
        void GetConvMessageList(string userId, uint maxCount, string lastMessageId, Action<Result> callback);

        void ClearConvHistoryMessage(string userId, Action<Result> callback = null);

        void ReportReadConvMessage(string convId, string messageId, Action<Result> callback = null);
    }

    public interface IChatGroup : IChatBase
    {
        void CreateGroup(string groupId, string jsonParams, Action<Result> callback = null);

        void DeleteGroup(string groupId, Action<Result> callback = null);
        
        void DeleteGroupConv(string groupId, Action<Result> callback = null);

        void JoinGroup(string groupId, Action<Result> callback = null);
        
        void JoinGroup(string groupId, string hello, Action<Result> callback = null);

        void QuitGroup(string groupId, Action<Result> callback = null);
        
        string SendGroupTextMessage(string groupId, string content, string extra = null, Action<Result> callback = null);
        
        string SendGroupSoundMessage(string groupId, string soundPath, int duration, string extra = null, Action<Result> callback = null);

        string SendGroupCustomMessage(string groupId, string customType, string content, string extra = null, Action<Result> callback = null);

        void CancelSendGroupMessage(string groupId, string messageId, Action<Result> callback = null);
        
        void RevokeGroupMessage(string groupId, string messageId, Action<Result> callback = null);

        void DeleteGroupMessage(string groupId, string messageId, Action<Result> callback = null);

        void GetGroupMessageList(string groupId, Action<Result> callback);
        
        void GetGroupMessageList(string groupId, uint maxCount, Action<Result> callback);
        
        void GetGroupMessageList(string groupId, uint maxCount, string lastMessageId, Action<Result> callback);

        void ClearGroupHistoryMessage(string groupId, Action<Result> callback = null);
        
        void ReportReadGroupMessage(string convId, string messageId, Action<Result> callback = null);
    }

    public interface IAudioDevice : IPluginBase
    {
        void StartLocalAudio(string json, Action<Result> callback = null);
        
        void StopLocalAudio();

        void PauseLocalAudio();
        
        void ResumeLocalAudio();
    }

    public interface IChatAudio : IChatBase, IAudioDevice
    {
        void AddRemoteStateChangeCallback(Action<Result> callback);
        
        void RemoveRemoteStateChangeCallback(Action<Result> callback);

        bool IsRemoteMute(string userId);

        void MuteRemoteAudio(string userId, bool enable);
        
        void MuteAllRemoteAudio(bool enable);

        int GetAudioCaptureVolume();

        void SetAudioCaptureVolume(int volume);

        int GetAudioPlayVolume();

        void SetAudioPlayVolume(int volume);
    }

    public interface IProfile : IPluginBase
    {
        void GetUserProfileList(List<string> userIds, Action<Result> callback);
        void GetUserProfileList(string jsonUserIds, Action<Result> callback);
    }

    public interface IFriendShip : IPluginBase
    {
        void ModifySelfUserProfile(string json, Action<Result> callback = null);

        void ModifyFriendProfile(string userId, string json, Action<Result> callback = null);

        void GetFriendProfileList(Action<Result> callback);

        void GetFriendsInfo(List<string> userIds, Action<Result> callback);
        void GetFriendsInfo(string jsonUserIds, Action<Result> callback);
        void SearchFriends(string json, Action<Result> callback);

        void AddFriend(string userId, string json, Action<Result> callback = null);

        void DeleteFriend(List<string> userIds, bool both, Action<Result> callback = null);
        void DeleteFriend(string jsonUserIds, bool both, Action<Result> callback = null);

        void CheckFriendType(List<string> userIds, bool both, Action<Result> callback);
        void CheckFriendType(string jsonUserIds, bool both, Action<Result> callback);

        void AddToBlackList(List<string> userIds, Action<Result> callback = null);
        void AddToBlackList(string jsonUserIds, Action<Result> callback = null);

        void GetBlackList(Action<Result> callback);

        void DeleteFromBlackList(List<string> userIds, Action<Result> callback = null);
        void DeleteFromBlackList(string jsonUserIds, Action<Result> callback = null);

        void GetRequestList(Action<Result> callback);

        void DeleteRequest(List<string> requestIds, Action<Result> callback = null);
        void DeleteRequest(string jsonRequestIds, Action<Result> callback = null);
        
        void HandleAgreeRequest(string userId, string json, Action<Result> callback = null);
        
        void HandleAgreeAndAddRequest(string userId, string json, Action<Result> callback = null);
        
        void HandleRejectRequest(string userId, string json, Action<Result> callback = null);
    }

    public interface IAudioRecorder: IPluginBase
    {
        void StartRecording(string json, Action<Result> callback = null, Action<int> volumeCallback = null);

        void StopRecording();

        void CancelRecording();
        
        void PlayRecordFile(string recordPath, string json = null, Action<Result> callback = null);

        void StopPlayRecord();

        void SetMicVolume(int volume);
    }
}
