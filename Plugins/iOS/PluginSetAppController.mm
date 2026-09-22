#import "UnityAppController.h"
#import "PluginListener.h"
#import <UIKit/UIKit.h>
#import <objc/runtime.h>
#include <objc/message.h>

// 与引擎 UnityInterface.h 中的声明保持一致（该头文件不在本插件的可见范围内，
// 引擎自己的 UnityAppController.mm 也是靠隐式声明+链接期解析来用的）。
extern "C" void UnitySetAbsoluteURL(const char* url);

// 显式分发到父类实现。
//
// 【为什么不用 [super xxx]】
//   1) 本文件是 .mm（Objective-C++），在 block 内使用 super 会编译失败；
//   2) 下面 scene delegate 的转发写在 Category 里，C++ 下 Category 中的 super 语义
//      也不可靠。
//   两次都用 objc_msgSendSuper 显式分发，语义明确且不依赖编译器扩展。
//   （无需 objc_msgSendSuper2：本类与父类都不实现 +initialize，不涉及 isa 相关陷阱。）
static inline void PluginSetSendToSuper(id self, SEL selector, id arg)
{
    Class superClass = class_getSuperclass(object_getClass(self));
    struct objc_super sup = { self, superClass };
    ((void (*)(struct objc_super*, SEL, id))objc_msgSendSuper)(&sup, selector, arg);
}

@interface PluginSetAppController : UnityAppController
@end

// 【兼容性】仅在 Info.plist 确实声明了 UIApplicationSceneManifest 时才延后。
//   没有声明（旧工程、或未走本插件注入的构建）时保持引擎原有同步启动路径，
//   避免出现「延后了但永远没有 scene 回调来唤醒」的黑屏。
//   调用方：sceneDidBecomeActive: 会先转发，引擎内部已有幂等守卫，重复触发安全。
static BOOL PluginSetHasSceneManifest(void)
{
    id value = [[NSBundle mainBundle] objectForInfoDictionaryKey:@"UIApplicationSceneManifest"];
    return value != nil;
}

// ============================================================================
// UIScene 生命周期支持（iOS 27 启动崩溃修复）
//
// 【症状】iOS 27 上点开图标立刻闪退；iOS 26 及更早一切正常。
//
// 【根因】iOS 27 起系统强制要求 App 采用 UIKit 的 scene-based 生命周期。
//   Unity 2021.3 的 iOS trampoline 不支持 scene，其建窗代码是：
//       _window = [[UIWindow alloc] initWithWindowScene:
//                    [self pickStartupWindowScene: application.connectedScenes]];
//   在没有 scene manifest 时，系统不会建立 UIWindowSceneSession，
//   connectedScenes 为空 → pickStartupWindowScene: 返回 nil → 传入 nil。
//   而 initWithWindowScene: 收到 nil 时，iOS 26 及更早是静默降级，iOS 27 改为抛异常。
//   API 没变，是「容错」变成了「强制」——这就是同一份二进制只有 iOS 27 崩的原因。
//   崩溃点在启动链最前端（早于引擎初始化与 Lua），所以表现为立刻闪退。
//
// 【修复】两件事配合，均在本插件内完成，不需要改动 Unity 安装目录：
//   1) Editor 侧 IosUISceneCompat.cs 往导出的 Info.plist 注入 UIApplicationSceneManifest
//      （scene manifest 由此声明；本文件按类名字符串被系统查找，不需要 pbxproj 改动）。
//   2) 本文件把 didFinishLaunchingWithOptions: 交给引擎的时机延后一轮主 RunLoop，
//      等 UIWindowScene 挂载完成，引擎的 pickStartupWindowScene: 就能取到有效 scene，
//      从而走 initWithWindowScene: 的正确路径。
//
// 【为什么不用 if (scene == nil) 守卫】守卫只让「没有 scene」不崩溃，但那个窗口
//   没有绑定 scene，永远不会显示 —— 会从「启动闪退」变成「启动黑屏」，不是修复。
// ============================================================================
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

#pragma mark - UIScene 兼容

// 延后一轮主 RunLoop 再把启动流程交回引擎：
// didFinishLaunchingWithOptions: 返回后系统才把 UIWindowScene 挂到 UIApplication 上，
// 所以必须等这一轮走完，引擎的 connectedScenes 才不为空。
// 注意：仅在 manifest 存在时延后，否则保持同步启动（见 PluginSetHasSceneManifest 注释）。
- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions
{
    if (!PluginSetHasSceneManifest())
        return [super application:application didFinishLaunchingWithOptions:launchOptions];

    __weak PluginSetAppController* weakSelf = self;
    dispatch_async(dispatch_get_main_queue(), ^{
        PluginSetSendToSuper(weakSelf, @selector(application:didFinishLaunchingWithOptions:), launchOptions);
    });

    return YES;
}

@end

// ---------------------------------------------------------------------------
// scene delegate
//
// 用 Category 而不是新建类：这样类名仍是 PluginSetAppController，与
// IMPL_APP_CONTROLLER_SUBCLASS 注册的类一致，Editor 侧注入 manifest 时
// 直接用这个类名即可，不需要再引入一个类。
// Unity 6 自带的支持也是这个结构（UnityAppController + 独立的 UnityScene）。
// ---------------------------------------------------------------------------
@interface PluginSetAppController (UISceneLifecycle) <UIWindowSceneDelegate>
@end

@implementation PluginSetAppController (UISceneLifecycle)

// 全部转发回引擎原有的 application* 实现，复用其暂停/恢复/渲染逻辑与内部状态守卫
// （例如 applicationDidBecomeActive: 内部自带「未就绪则初始化、已就绪则恢复」的判断）。
// 刻意不触碰引擎的内部静态变量（_unityEngineInitialized / _unityAppReady 等），
// 那些符号在 UnityAppController.mm 内是文件级 static，本文件不可见。
- (void)sceneDidBecomeActive:(UIScene *)scene
{
    PluginSetSendToSuper(self, @selector(applicationDidBecomeActive:), [UIApplication sharedApplication]);
}

- (void)sceneWillEnterForeground:(UIScene *)scene
{
    PluginSetSendToSuper(self, @selector(applicationWillEnterForeground:), [UIApplication sharedApplication]);
}

- (void)sceneWillResignActive:(UIScene *)scene
{
    PluginSetSendToSuper(self, @selector(applicationWillResignActive:), [UIApplication sharedApplication]);
}

- (void)sceneDidEnterBackground:(UIScene *)scene
{
    PluginSetSendToSuper(self, @selector(applicationDidEnterBackground:), [UIApplication sharedApplication]);
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
