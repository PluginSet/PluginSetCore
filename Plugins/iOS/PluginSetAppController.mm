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
// 【修复做法】manifest 由 Editor 侧 IosUISceneCompat.cs 注入导出的 Info.plist，
//   让系统建立 scene；本文件负责「等 scene 就绪后再让引擎建窗」，做法是：
//     · 不覆写 didFinishLaunchingWithOptions:（保持引擎原有同步流程，不做任何转交）；
//     · 覆写 applicationDidBecomeActive:，在 scene 已挂载时先确保引擎以 scene 建窗，
//       然后原样交给引擎的既有实现。
//
// 【为什么不用 objc_msgSendSuper 手工分发】
//   上一版曾用 if (startupScene == nil) 思路，通过 block + objc_msgSendSuper 延后调用
//   引擎的 didFinishLaunchingWithOptions:，真机在 objc_retain 处 KERN_INVALID_ADDRESS 崩溃
//   （参数被破坏）。手工拼 objc_super / 函数指针转型在 arm64e 上不可靠，已彻底移除。
//   现在所有对父类的调用都是 [super ...]，由编译器生成分发。
//
// 【兼容性】
//   · manifest 不存在（旧工程 / 未走本插件注入的构建）：系统不会创建 scene。
//     引擎的 didFinishLaunchingWithOptions: 找不到 scene 时会退回 initWithFrame:，
//     且没有 scene 回调来打扰，行为与改动前完全一致。
//   · 引擎侧 _unityEngineInitialized 守卫保证不会重复初始化。
// ============================================================================

@interface PluginSetAppController : UnityAppController
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

// 由 scene 回调驱动（见下方 Category）。
//
// 为什么挂在 applicationDidBecomeActive: 而不是 didFinishLaunchingWithOptions:：
//   采用 scene 生命周期后，didFinishLaunchingWithOptions: 返回之前 UIWindowScene 尚未挂载，
//   此时调用引擎的初始化会拿到空 scene。sceneDidBecomeActive: 是「scene 已激活」的信号，
//   这个时点引擎的 connectedScenes 一定非空，建窗走 initWithWindowScene: 正确路径。
//
// 为什么仍然要调 [super applicationDidBecomeActive:]：
//   引擎实现内部自带「未就绪则初始化 / 已就绪则恢复」判断（_unityAppReady + _unityEngineInitialized
//   守卫），此处先确保初始化，再原样交给它，避免丢失它的恢复/聚焦逻辑。
- (void)applicationDidBecomeActive:(UIApplication *)application
{
    // 诊断日志：把关键事实写进真机日志，便于直接判断修复是否生效。
    // （上一版缺这条信息，只能靠推测，走了弯路。）
    [PluginSetAppController logSceneStateWithTag:@"didBecomeActive"];

    if ([self respondsToSelector:@selector(initUnityWithApplication:)])
        [self initUnityWithApplication:application];

    [super applicationDidBecomeActive:application];
}

#pragma mark - 诊断

// 打印 UIScene 关键事实：
//   manifest=0 → Editor 侧注入没生效，iOS 27 上仍会因 nil scene 崩溃
//   scenes=0   → manifest 在，但系统没建立 scene（scene 类名解析失败等）
//   scenes>0   → scene 正常，修复处于预期状态
+ (void)logSceneStateWithTag:(NSString *)tag
{
    BOOL hasManifest = [[NSBundle mainBundle] objectForInfoDictionaryKey:@"UIApplicationSceneManifest"] != nil;
    NSUInteger sceneCount = [UIApplication sharedApplication].connectedScenes.count;

    NSLog(@"[PluginSetUIScene][%@] manifest=%d connectedScenes=%lu",
          tag, (int)hasManifest, (unsigned long)sceneCount);
}

#pragma mark - scene 回调转发目标
// 这些方法定义在主 @implementation 中（而不是 Category），因此可以安心使用 [super ...]。
// Category 里的 scene delegate 只做一次 [self sceneForwardXxx] 转调。

- (void)sceneForwardDidBecomeActive
{
    [self applicationDidBecomeActive:[UIApplication sharedApplication]];
}

- (void)sceneForwardWillEnterForeground
{
    [super applicationWillEnterForeground:[UIApplication sharedApplication]];
}

- (void)sceneForwardWillResignActive
{
    [super applicationWillResignActive:[UIApplication sharedApplication]];
}

- (void)sceneForwardDidEnterBackground
{
    [super applicationDidEnterBackground:[UIApplication sharedApplication]];
}

@end

// ---------------------------------------------------------------------------
// scene delegate
//
// 用 Category 而不是新建类：这样类名仍是 PluginSetAppController，与
// IMPL_APP_CONTROLLER_SUBCLASS 注册的 AppController 类一致，Editor 侧注入 manifest 时
// 直接写这个类名即可，既不用引入第二个类，也不需要改 project.pbxproj 加文件。
// Unity 6 自带的支持也是同一结构（AppController + 独立 scene delegate）。
//
// Category 内不做任何父类分发：只把事件转回本类的既有实现（下面 sceneForward… 方法
// 定义在主 @implementation 里），因此不存在 Category 中 super 的语义问题。
// ---------------------------------------------------------------------------
@interface PluginSetAppController (UISceneLifecycleForward)
- (void)sceneForwardDidBecomeActive;
- (void)sceneForwardWillEnterForeground;
- (void)sceneForwardWillResignActive;
- (void)sceneForwardDidEnterBackground;
@end

@implementation PluginSetAppController (UISceneLifecycleForward)

- (void)sceneDidBecomeActive:(UIScene *)scene
{
    [self sceneForwardDidBecomeActive];
}

- (void)sceneWillEnterForeground:(UIScene *)scene
{
    [self sceneForwardWillEnterForeground];
}

- (void)sceneWillResignActive:(UIScene *)scene
{
    [self sceneForwardWillResignActive];
}

- (void)sceneDidEnterBackground:(UIScene *)scene
{
    [self sceneForwardDidEnterBackground];
}

// 采用 scene 生命周期后，URL 不再走 application:openURL:options:，改由此回调送达。
// 微信 / 支付宝 / 云闪付 / 一键登录 的回跳依赖它；这里复刻引擎原有的通知负载结构，
// 保证既有插件的 kUnityOnOpenURL 监听逻辑无需改动。
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
