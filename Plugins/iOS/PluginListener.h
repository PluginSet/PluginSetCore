#pragma once

#import "PluginUtils.h"
#import "AppDelegateListener.h"

@protocol PluginListener<AppDelegateListener>
@optional

#define ERROR_CODE_CANCEL  -1
#define ERROR_CODE_SUCCESS  0
#define ERROR_CODE_FAIL_DEFAULT 9999

#define UNITY_CALLBACK_NAME "UnityAppleListener"
#define UNITY_CALLBACK_FUNC "OnUnityAppleCallback"

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
-(void) callback:(NSString*)callback success:(bool)success withCode:(int)code withResult:(NSString*)result;\
-(void) successCallback:(NSString*)callback withResult:(NSDictionary*)dict;\
-(void) failCallback:(NSString*)callback withResult:(NSDictionary*)dict;\
-(void) failCallback:(NSString*)callback withCode:(int)code withResult:(NSDictionary*)dict;\
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
                                                       \
\
-(void) callback:(NSString *)callback success:(bool)success withCode:(int)code withResult:(NSString *)result\
{\
    NSDictionary * data = @{\
        @"callback": callback,\
        @"success" : @(success),\
        @"code"    : @(code),\
        @"result"  : result\
    };\
    \
    NSString * dataStr = [PluginUtils dictToJson:data];\
    UnitySendMessage(UNITY_CALLBACK_NAME, UNITY_CALLBACK_FUNC, [dataStr UTF8String]);\
}\
\
-(void) successCallback:(NSString *)callback withResult:(NSDictionary *)dict\
{\
    [self callback:callback success:true withCode:ERROR_CODE_SUCCESS withResult:[PluginUtils dictToJson:dict]];\
}\
\
-(void) failCallback:(NSString *)callback withResult:(NSDictionary *)dict\
{\
    [self failCallback:callback withCode:ERROR_CODE_FAIL_DEFAULT withResult:dict];\
}\
\
-(void) failCallback:(NSString *)callback withCode:(int)code withResult:(NSDictionary *)dict\
{\
    [self callback:callback success:false withCode:code withResult:[PluginUtils dictToJson:dict]];\
}\
@end                                                   \
