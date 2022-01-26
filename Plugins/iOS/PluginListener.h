#pragma once

#import "AppDelegateListener.h"

@protocol PluginListener<AppDelegateListener>
@optional

#define ERROR_CODE_CANCEL  -1
#define ERROR_CODE_SUCCESS  0
#define ERROR_CODE_FAIL_DEFAULT 9999

// when application called continueUserActivity
- (void) onContinueUserActivity:(NSNotification *)notification;

@end

void RegisterPluginListener(id<PluginListener> obj);
void UnregisterPluginListener(id<PluginListener> obj);

extern "C" __attribute__((visibility("default"))) NSString* const kUnityOnContinueUserActivity;

#define REGISTER_PLUGIN_LISTENER(ClassName)            \
static ClassName* _inst;                               \
@interface ClassName(ClassName##_PluginSingleton)      \
{                                                      \
}                                                      \
+(void)load;                                           \
+(ClassName*)GetInstance;                              \
@end                                                   \
                                                       \
@implementation ClassName(ClassName##_PluginSingleton) \
+(void)load                                            \
{                                                      \
    _inst = [ClassName new];                           \
    RegisterPluginListener(_inst);                     \
}                                                      \
                                                       \
+(ClassName*)GetInstance                               \
{                                                      \
    return _inst;                                      \
}                                                      \
@end                                                   \
