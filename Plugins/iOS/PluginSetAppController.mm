#import "UnityAppController.h"
#import "PluginListener.h"

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

@end
