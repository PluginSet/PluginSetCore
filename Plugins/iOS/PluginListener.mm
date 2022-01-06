//
//  PluginListener.m
//  UnityFramework
//
//  Created by macbook on 2022/1/5.
//

#import "PluginListener.h"

#define DEFINE_NOTIFICATION(name) extern "C" __attribute__((visibility ("default"))) NSString* const name = @#name;

DEFINE_NOTIFICATION(kUnityOnContinueUserActivity);

#undef DEFINE_NOTIFICATION

void RegisterPluginListener(id<PluginListener> obj)
{
    #define REGISTER_SELECTOR(sel, notif_name)                  \
    if([obj respondsToSelector:sel])                            \
        [[NSNotificationCenter defaultCenter]   addObserver:obj \
                                                selector:sel    \
                                                name:notif_name \
                                                object:nil      \
        ];                                                      \

    UnityRegisterAppDelegateListener(obj);

    REGISTER_SELECTOR(@selector(onContinueUserActivity:), kUnityOnContinueUserActivity);
}

void UnregisterPluginListener(id<PluginListener> obj)
{
    UnityUnregisterAppDelegateListener(obj);

    [[NSNotificationCenter defaultCenter] removeObserver: obj name: kUnityOnContinueUserActivity object: nil];
}
