#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import <UserNotifications/UserNotifications.h>
#import "PluginUtils.h"
#import "AppDelegateListener.h"

static NSString* gDevicesPendingOpenPayload = nil;
static int gDevicesBadgeNumber = 0;

char* UtilsMakeStringCopyDN (const char* string)
{
    if (string == NULL)
        return NULL;

    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}

static void DevicesNotificationStoreOpenedUserInfo(NSDictionary* userInfo, BOOL isInitialLaunch) {
    if (userInfo == nil) {
        return;
    }
    NSString* title = userInfo[@"title"];
    NSString* body = userInfo[@"body"];
    NSString* actionId = userInfo[@"actionId"];

    NSMutableDictionary* data = [NSMutableDictionary dictionaryWithDictionary:userInfo];
    [data removeObjectForKey:@"title"];
    [data removeObjectForKey:@"body"];
    [data removeObjectForKey:@"actionId"];

    NSMutableDictionary* payload = [NSMutableDictionary dictionary];
    payload[@"title"] = title != nil ? title : @"";
    payload[@"body"] = body != nil ? body : @"";
    payload[@"actionId"] = actionId != nil ? actionId : @"";
    payload[@"data"] = data;
    payload[@"isInitialLaunch"] = @(isInitialLaunch);

    NSString* json = [PluginUtils dictToJson:payload];
    if (json == nil) {
        return;
    }
    gDevicesPendingOpenPayload = json;
}

static NSMutableDictionary* DevicesNotificationBuildUserInfo(
        const char* title, const char* body, const char* dataJson, const char* actionId) {
    NSMutableDictionary* userInfo = [NSMutableDictionary dictionary];
    if (title != NULL) {
        userInfo[@"title"] = [NSString stringWithUTF8String:title];
    }
    if (body != NULL) {
        userInfo[@"body"] = [NSString stringWithUTF8String:body];
    }
    if (actionId != NULL && strlen(actionId) > 0) {
        userInfo[@"actionId"] = [NSString stringWithUTF8String:actionId];
    }
    if (dataJson != NULL && strlen(dataJson) > 0) {
        NSString* json = [NSString stringWithUTF8String:dataJson];
        NSDictionary* custom = [PluginUtils jsonToDict:json];
        if (custom != nil) {
            [userInfo addEntriesFromDictionary:custom];
        }
    }
    return userInfo;
}

@interface DevicesNotificationAppDelegate : NSObject<AppDelegateListener, UNUserNotificationCenterDelegate>
@end

@implementation DevicesNotificationAppDelegate

+ (void)load {
    static DevicesNotificationAppDelegate* instance = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        instance = [[DevicesNotificationAppDelegate alloc] init];
        UnityRegisterAppDelegateListener(instance);
        UNUserNotificationCenter* center = [UNUserNotificationCenter currentNotificationCenter];
        if (center.delegate == nil) {
            center.delegate = instance;
        }
    });
}

- (void)userNotificationCenter:(UNUserNotificationCenter*)center
didReceiveNotificationResponse:(UNNotificationResponse*)response
         withCompletionHandler:(void (^)(void))completionHandler {
    NSDictionary* userInfo = response.notification.request.content.userInfo;
    DevicesNotificationStoreOpenedUserInfo(userInfo, NO);
    completionHandler();
}

@end

extern "C"
{
    void _PluginSetNotificationStorePendingOpenJson(const char* json) {
        if (json == NULL) {
            gDevicesPendingOpenPayload = nil;
            return;
        }
        gDevicesPendingOpenPayload = [NSString stringWithUTF8String:json];
    }

    bool _PluginSetNotificationRequestPermission() {
        // 同步化：让 C# 侧可以像麦克风权限一样“请求并返回结果”。
        // 注意：这里允许在主线程阻塞，且设置了超时，避免极端情况下死锁。
        __block BOOL granted = NO;
        dispatch_semaphore_t signal = dispatch_semaphore_create(0);

        UNUserNotificationCenter* center = [UNUserNotificationCenter currentNotificationCenter];
        [center requestAuthorizationWithOptions:(UNAuthorizationOptionAlert | UNAuthorizationOptionSound | UNAuthorizationOptionBadge)
                              completionHandler:^(BOOL permissionGranted, NSError* error) {
            granted = permissionGranted == YES && error == nil;
            dispatch_semaphore_signal(signal);
        }];

        // 最多等待 8 秒；超时则返回当前 granted（可能仍为 NO）。
        dispatch_time_t timeout = dispatch_time(DISPATCH_TIME_NOW, (int64_t)(8 * NSEC_PER_SEC));
        dispatch_semaphore_wait(signal, timeout);
        return granted == YES;
    }

    void _PluginSetNotificationSetBadgeNumber(int count) {
        gDevicesBadgeNumber = count;
        dispatch_async(dispatch_get_main_queue(), ^{
            [UIApplication sharedApplication].applicationIconBadgeNumber = count;
        });
    }

    int _PluginSetNotificationGetBadgeNumber() {
        return gDevicesBadgeNumber;
    }

    void _PluginSetNotificationRemoveAllDelivered() {
        dispatch_async(dispatch_get_main_queue(), ^{
            [[UNUserNotificationCenter currentNotificationCenter] removeAllDeliveredNotifications];
            [UIApplication sharedApplication].applicationIconBadgeNumber = 0;
            gDevicesBadgeNumber = 0;
        });
    }

    const char* _PluginSetNotificationConsumeInitialJson() {
        if (gDevicesPendingOpenPayload == nil) {
            return UtilsMakeStringCopyDN("");
        }
        const char* result = UtilsMakeStringCopyDN([gDevicesPendingOpenPayload UTF8String]);
        gDevicesPendingOpenPayload = nil;
        return result;
    }

    void _PluginSetNotificationFreeCString(const char* value) {
        if (value != NULL) {
            free((void*)value);
        }
    }

    void _PluginSetNotificationPostLocal(
            const char* identifier,
            const char* channelId,
            const char* title,
            const char* body,
            int delaySeconds,
            const char* dataJson,
            const char* actionId) {
        UNMutableNotificationContent* content = [[UNMutableNotificationContent alloc] init];
        content.title = title != NULL ? [NSString stringWithUTF8String:title] : @"";
        content.body = body != NULL ? [NSString stringWithUTF8String:body] : @"";
        content.sound = [UNNotificationSound defaultSound];
        content.userInfo = DevicesNotificationBuildUserInfo(title, body, dataJson, actionId);

        NSString* nsId = (identifier != NULL && strlen(identifier) > 0)
            ? [NSString stringWithUTF8String:identifier]
            : [[NSUUID UUID] UUIDString];

        UNNotificationTrigger* trigger = nil;
        if (delaySeconds > 0) {
            trigger = [UNTimeIntervalNotificationTrigger triggerWithTimeInterval:delaySeconds repeats:NO];
        } else {
            trigger = [UNTimeIntervalNotificationTrigger triggerWithTimeInterval:0.1 repeats:NO];
        }

        UNNotificationRequest* request = [UNNotificationRequest requestWithIdentifier:nsId content:content trigger:trigger];
        [[UNUserNotificationCenter currentNotificationCenter] addNotificationRequest:request withCompletionHandler:nil];
    }

    void _PluginSetNotificationCancelLocal(const char* identifier) {
        if (identifier == NULL || strlen(identifier) == 0) {
            return;
        }
        NSString* nsId = [NSString stringWithUTF8String:identifier];
        UNUserNotificationCenter* center = [UNUserNotificationCenter currentNotificationCenter];
        [center removePendingNotificationRequestsWithIdentifiers:@[nsId]];
        [center removeDeliveredNotificationsWithIdentifiers:@[nsId]];
    }

    void _PluginSetNotificationCancelAllPendingLocal() {
        UNUserNotificationCenter* center = [UNUserNotificationCenter currentNotificationCenter];
        [center removeAllPendingNotificationRequests];
        [center removeAllDeliveredNotifications];
    }
}
