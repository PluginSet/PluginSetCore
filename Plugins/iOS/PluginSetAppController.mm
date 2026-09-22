#import "UnityAppController.h"
#import "PluginListener.h"
#import <UIKit/UIKit.h>

// 与引擎 UnityInterface.h 中的声明保持一致（该头文件不在本插件可见范围内，
// 引擎自己的 UnityAppController.mm 也是靠隐式声明 + 链接期解析来用的）。
extern "C" void UnitySetAbsoluteURL(const char* url);

// ============================================================================
// UIScene 生命周期支持（iOS 27 启动崩溃修复）
//
// 【症状】iOS 27 上点开图标立刻闪退；iOS 26 及更早一切正常。
//
// 【根因】iOS 27 起系统强制要求 App 采用 UIKit 的 scene-based 生命周期。
//   Unity 2021.3 的 iOS trampoline 不支持 scene，其建窗代码是：
//       _window = [[UIWindow alloc] initWithWindowScene:
//                    [self pickStartupWindowScene: application.connectedScenes]];
//   没有 scene manifest 时系统不建立 UIWindowSceneSession，connectedScenes 为空
//   → pickStartupWindowScene: 返回 nil → 传入 nil。该 API 在 iOS 26 及更早是静默降级，
//   iOS 27 起改为抛异常 —— 这就是同一份二进制只有 iOS 27 崩的原因。
//
// 【修复】manifest 由 Editor 侧 IosUISceneCompat.cs 注入导出的 Info.plist，让系统建立
//   scene；引擎原有的初始化流程拿到有效 scene 后即可正确建窗。因此本文件：
//     · 不覆写 didFinishLaunchingWithOptions:，保持引擎原有同步流程；
//     · 不调用引擎内部方法（initUnityWithApplication: 等未在 UnityAppController.h 声明）；
//     · scene 回调只转发给引擎既有的 application* 实现，由编译器生成分发。
//   类名仍为 PluginSetAppController（IMPL_APP_CONTROLLER_SUBCLASS 注册的类），
//   Editor 侧注入 manifest 时直接写这个类名，无需第二个类、无需改 pbxproj。
//
// 【三个已踩过的坑（勿重犯）】
//   1) block + 手写 objc_msgSendSuper 延后调用引擎的 didFinishLaunchingWithOptions:
//      → 真机 objc_retain 处 KERN_INVALID_ADDRESS（参数被破坏）。arm64e 下手工拼
//      objc_super 不可靠，禁止再用。
//   2) respondsToSelector: 之后再调用引擎未公开的方法 → 抑制不了编译期的 selector
//      可见性检查，CI 报 "no visible @interface declares the selector"。
//   3) ★ 在 Category 里写 [super ...] —— UnityAppController.h 中 UnityAppController
//      直接继承 NSObject（@interface UnityAppController : NSObject<UIApplicationDelegate>），
//      所以 Category 中的 super 指向 NSObject，applicationDidBecomeActive: 之类
//      UIApplicationDelegate 协议方法在那里并不存在 → doesNotRecognizeSelector 崩溃。
//      因此本文件把所有需要 [super ...] 的方法都放在主 @implementation 中，
//      只在 Category 放一个不需要 super 的 openURLContexts:。
//
// 【兼容性】manifest 不存在（旧工程 / 未走本插件注入的构建）时系统不创建 scene，
//   引擎的 didFinishLaunchingWithOptions: 会退回 initWithFrame:，且没有 scene 回调，
//   行为与改动前完全一致。
// ============================================================================

@interface PluginSetAppController : UnityAppController <UIWindowSceneDelegate>
@end

IMPL_APP_CONTROLLER_SUBCLASS (PluginSetAppController)
 
@implementation PluginSetAppController

- (BOOL)application:(UIApplication *)application continueUserActivity:(NSUserActivity *)userActivity
#if defined(__IPHONE_12_0) || defined(__TVOS_12_0)
    restorationHandler:(void (^)(NSArray<id<UIUserActivityRestoring> > * _Nullable restorableObjects))restorationHandler
#else
    restorationHandler:(void (^)(NSArray * _Nullable))restorationHandler
#endif
{
    NSMutableDictionary<NSString*, id>* notifData = [NSMutableDictionary dictionaryWithCapacity: 1];
    notifData[@"activity"] = userActivity;
    AppController_SendNotificationWithArg(kUnityOnContinueUserActivity, notifData);
    
    return [super application:application continueUserActivity:userActivity restorationHandler:restorationHandler];
}

#pragma mark - scene 生命周期转发
// 这些方法定义在「类」的 @implementation 内，因此 [super ...] 明确指向 UnityAppController，
// 由编译器生成分发；引擎内部自带 _unityEngineInitialized / _unityAppReady 幂等守卫，
// 重复调用安全。

- (void)sceneDidBecomeActive:(UIScene *)scene
{
    [self logSceneStateWithTag:@"sceneDidBecomeActive"];
    [super applicationDidBecomeActive:[UIApplication sharedApplication]];
}

- (void)sceneWillEnterForeground:(UIScene *)scene
{
    [super applicationWillEnterForeground:[UIApplication sharedApplication]];
}

- (void)sceneWillResignActive:(UIScene *)scene
{
    [super applicationWillResignActive:[UIApplication sharedApplication]];
}

- (void)sceneDidEnterBackground:(UIScene *)scene
{
    [super applicationDidEnterBackground:[UIApplication sharedApplication]];
}

#pragma mark - 诊断

// 打印 UIScene 关键事实，便于从真机日志直接判断修复是否生效（不必再靠推测）：
//   manifest=0 → Editor 侧注入没生效，iOS 27 上仍会因 nil scene 崩溃
//   scenes=0   → manifest 在，但系统没建立 scene（scene 类名解析失败等）
//   scenes>0   → scene 正常，修复处于预期状态
- (void)logSceneStateWithTag:(NSString *)tag
{
    BOOL hasManifest = [[NSBundle mainBundle] objectForInfoDictionaryKey:@"UIApplicationSceneManifest"] != nil;
    NSUInteger sceneCount = [UIApplication sharedApplication].connectedScenes.count;

    NSLog(@"[PluginSetUIScene][%@] manifest=%d connectedScenes=%lu",
          tag, (int)hasManifest, (unsigned long)sceneCount);
}

@end

// ---------------------------------------------------------------------------
// URL 回跳
//
// 放在 Category 里，因为这里不需要任何 [super ...]：
// 采用 scene 生命周期后 URL 不再走 application:openURL:options:，改由此回调送达。
// 微信 / 支付宝 / 云闪付 / 一键登录 的回跳依赖它；这里复刻引擎原有的通知负载结构，
// 保证既有插件的 kUnityOnOpenURL 监听逻辑无需改动。
// ---------------------------------------------------------------------------
@interface PluginSetAppController (UISceneOpenURL)
@end

@implementation PluginSetAppController (UISceneOpenURL)

- (void)scene:(UIScene *)scene openURLContexts:(NSSet<UIOpenURLContext *> *)URLContexts
{
    for (UIOpenURLContext *ctx in URLContexts)
    {
        if (ctx.URL == nil)
            continue;

        UnitySetAbsoluteURL(ctx.URL.absoluteString.UTF8String);

        NSMutableDictionary<NSString*, id>* notifData = [NSMutableDictionary dictionaryWithCapacity: 3];
        notifData[@"url"] = ctx.URL;
        if (ctx.options.sourceApplication != nil)
            notifData[@"sourceApplication"] = ctx.options.sourceApplication;
        if (ctx.options.annotation != nil)
            notifData[@"annotation"] = ctx.options.annotation;

        AppController_SendNotificationWithArg(kUnityOnOpenURL, notifData);
        break;
    }
}

@end
