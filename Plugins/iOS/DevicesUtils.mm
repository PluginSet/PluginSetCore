#import <AppTrackingTransparency/AppTrackingTransparency.h>
#import <AdSupport/AdSupport.h>
#import <AVFoundation/AVFoundation.h>
#import "PluginUtils.h"

NSString* Plugin_Utils_CreateNSString (const char* string)
{
    if (string)
        return [NSString stringWithUTF8String: string];
    else
        return [NSString stringWithUTF8String: ""];
}

char* UtilsMakeStringCopy (const char* string)
{
    if (string == NULL)
        return NULL;

    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}

const int PermissionDenied = 0;
const int PermissionGranted = 1;
const int PermissionShouldAsk = 2;

extern "C"
{

    void _PlatformLog(const char * msg)
    {
        NSLog(@"%@", Plugin_Utils_CreateNSString(msg));
    }


    //拷贝内容至剪切板
    void _CopyTextToClipboard(const char *textList)
    {
        NSString *text = Plugin_Utils_CreateNSString(textList);
        [UIPasteboard generalPasteboard].string = text;
    }
    
    //读取内容至剪切板
    const char* _ReadTextToClipboard()
    {
        if([UIPasteboard generalPasteboard].hasStrings)
        {
            NSString *version =  [UIPasteboard generalPasteboard].string;
            return UtilsMakeStringCopy([version UTF8String]);
        }
        return UtilsMakeStringCopy("");
    }

    /*
      马达震动
      style 震动幅度1~3,递增
     */
    void _Vibrate(int style)
    {
        NSString *version = [UIDevice currentDevice].systemVersion;
        double versionNumber = version.doubleValue;
        
        if (versionNumber < 10.0)
            return;
        
        if (versionNumber < 13.0 && style > 2)
            style = 2;
        
        
        UIImpactFeedbackGenerator *feedBackGenertor = [[UIImpactFeedbackGenerator alloc] initWithStyle:(UIImpactFeedbackStyle)style];
        
        [feedBackGenertor impactOccurred];
    }

    const char* _ReadInfoPlist(const char* key, const char* defaultValue)
    {
        NSString *nsKey = Plugin_Utils_CreateNSString(key);
        NSDictionary *nsDic = [NSBundle mainBundle].infoDictionary;
        if([[nsDic allKeys] containsObject: nsKey])
        {
            NSString *nsvalue = nsDic[nsKey];
            return UtilsMakeStringCopy([nsvalue UTF8String]);
        }
        return defaultValue;
    }

    const char* _GetVersionCode()
    {
        NSString* build = [[[NSBundle mainBundle]infoDictionary] objectForKey:@"CFBundleVersion"];
        return UtilsMakeStringCopy([build UTF8String]);
    }

    const char* _GetOrSaveKeyChain(const char* services, const char* key, const char* value)
    {
        NSString * stringServices = [PluginUtils createNSString:services];
        NSString * stringKey = [PluginUtils createNSString:key];
        NSString * stringValue = [PluginUtils createNSString:value];
        
        NSDictionary * dict = [PluginUtils loadKeychain:stringServices];
        if (!dict) {
            [PluginUtils saveKeychain:stringServices data:@{
                stringKey: stringValue
            }];
            return UtilsMakeStringCopy([stringValue UTF8String]);
        }
        
        if ([[dict allKeys] containsObject:stringKey])
        {
            return UtilsMakeStringCopy([[dict valueForKey:stringKey] UTF8String]);
        }
        
        NSMutableDictionary * mutableDict = dict.mutableCopy;
        [mutableDict setValue:stringValue forKey:stringKey];
        [PluginUtils saveKeychain:stringServices data:mutableDict];
        return UtilsMakeStringCopy([stringValue UTF8String]);
    }

    void _SaveKeyChain(const char* services, const char* key, const char* value)
    {
        NSString * stringServices = [PluginUtils createNSString:services];
        NSString * stringKey = [PluginUtils createNSString:key];
        NSString * stringValue = [PluginUtils createNSString:value];
        
        NSDictionary * dict = [PluginUtils loadKeychain:stringServices];
        NSMutableDictionary * mutableDict;
        if (dict) {
            mutableDict = dict.mutableCopy;
        } else {
            mutableDict = [NSMutableDictionary new];
        }
        
        [mutableDict setValue:stringValue forKey:stringKey];
        [PluginUtils saveKeychain:stringServices data:mutableDict];
    }

    void _DeleteAllKeyChainValue(const char * services)
    {
        NSString * stringServices = [PluginUtils createNSString:services];
        [PluginUtils deleteKeychain:stringServices];
    }

    void _DeleteKeyChain(const char* services, const char* key)
    {
        NSString * stringServices = [PluginUtils createNSString:services];
        NSDictionary * dict = [PluginUtils loadKeychain:stringServices];
        if (!dict) {
            return;
        }
        
        NSString * stringKey = [PluginUtils createNSString:key];
        if (![[dict allKeys] containsObject:stringKey])
        {
            return;
        }
        
        NSMutableDictionary * mutableDict = dict.mutableCopy;
        [mutableDict removeObjectForKey:stringKey];
        [PluginUtils saveKeychain:stringServices data:mutableDict];
    }

    int _CheckAdvertisingTrackingPermission()
    {
        if (@available(iOS 14, *)) {
            ATTrackingManagerAuthorizationStatus state = [ATTrackingManager trackingAuthorizationStatus];
            if(state == ATTrackingManagerAuthorizationStatusAuthorized){
                return PermissionGranted;
            } else if (state == ATTrackingManagerAuthorizationStatusNotDetermined) {
                return PermissionShouldAsk;
            }
            return PermissionDenied;
        } else {
            return [[ASIdentifierManager sharedManager] isAdvertisingTrackingEnabled] ? PermissionGranted : PermissionDenied;
        }
    }

    bool _RequestAdvertisingTracking()
    {
        if (@available(iOS 14, *)) {
            dispatch_semaphore_t signal = dispatch_semaphore_create(0);
            __block ATTrackingManagerAuthorizationStatus result;
            
            [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status) {
                result = status;
                dispatch_semaphore_signal(signal);
            }];
            dispatch_semaphore_wait(signal, DISPATCH_TIME_FOREVER);
            NSLog(@"sync _RequestAdvertisingTracking result %lu", (unsigned long)result);
            return result == ATTrackingManagerAuthorizationStatusAuthorized;
        } else {
            return [[ASIdentifierManager sharedManager] isAdvertisingTrackingEnabled];
        }
    }

    int _CheckMicrophonePermission()
    {
        AVAuthorizationStatus authStatus = [AVCaptureDevice authorizationStatusForMediaType:AVMediaTypeAudio];
        if (authStatus == AVAuthorizationStatusAuthorized) {
            return PermissionGranted;
        } else if (authStatus == AVAuthorizationStatusNotDetermined) {
            return PermissionShouldAsk;
        }
        
        return PermissionDenied;
    }

    bool _RequestMicrophoneAuth()
    {
        dispatch_semaphore_t signal = dispatch_semaphore_create(0);
        __block BOOL result;
        [AVCaptureDevice requestAccessForMediaType:AVMediaTypeAudio completionHandler:^(BOOL granted) {
            result = granted;
            dispatch_semaphore_signal(signal);
        }];
        dispatch_semaphore_wait(signal, DISPATCH_TIME_FOREVER);
        return result;
    }

    bool _EnableOpenSettings()
    {
        return (&UIApplicationOpenSettingsURLString) != NULL ;
    }

    void _OpenSettings()
    {
        if( _EnableOpenSettings() )
        {
    #if __IPHONE_OS_VERSION_MAX_ALLOWED >= 100000
            if( @available( iOS 10, *) )
                [[UIApplication sharedApplication] openURL:[NSURL URLWithString:UIApplicationOpenSettingsURLString] options:@{} completionHandler:nil];
            else
    #endif
                [[UIApplication sharedApplication] openURL:[NSURL URLWithString:UIApplicationOpenSettingsURLString]];
        }
    }

    bool _OSAvailable(int version)
    {
        NSString * ver = [NSString stringWithFormat:@"%d.0", version];
        return [[[UIDevice currentDevice]systemVersion]compare:ver options:NSNumericSearch] != NSOrderedAscending;
    }
}
