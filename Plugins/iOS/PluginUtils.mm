#import "PluginUtils.h"

@implementation PluginUtils
+(NSString*) readStringFromPlist:(NSString*)key defaultValue:(NSString*) value
{
    NSDictionary *nsDict = [NSBundle mainBundle].infoDictionary;
    if([[nsDict allKeys] containsObject: key])
    {
        return nsDict[key];
    }

    return value;
}

+(BOOL) readBooleanFromPlist:(NSString *)key defaultValue:(BOOL)value
{
    NSDictionary *nsDict = [NSBundle mainBundle].infoDictionary;
    if([[nsDict allKeys] containsObject: key])
    {
        return [nsDict[key] boolValue];
    }

    return value;
}

+ (NSString*) dictToJson: (NSDictionary*) dict
{
    NSData *data = [NSJSONSerialization dataWithJSONObject:dict options:0 error:nil];
    NSString *jsonString = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
    return jsonString;
}

+ (NSDictionary*) jsonToDict: (NSString*) json
{
    NSError *jsonError;
    NSData* data = [json dataUsingEncoding:NSUTF8StringEncoding];
    NSDictionary* dict = [NSJSONSerialization JSONObjectWithData:data options:NSJSONReadingMutableContainers error:&jsonError];
    return (!dict ? nil : dict);
}

+ (NSMutableDictionary *)getKeychainQuery:(NSString *)service {
    return [NSMutableDictionary dictionaryWithObjectsAndKeys:
            (id)kSecClassGenericPassword,(id)kSecClass,
            service, (id)kSecAttrService,
            service, (id)kSecAttrAccount,
            (id)kSecAttrAccessibleAfterFirstUnlock,(id)kSecAttrAccessible,
            nil];
}

+ (void)saveKeychain:(NSString *)key data:(id)data
{
    //Get search dictionary
    NSMutableDictionary *keychainQuery = [self getKeychainQuery:key];
    //Delete old item before add new item
    SecItemDelete((CFDictionaryRef)keychainQuery);
    //Add new object to search dictionary(Attention:the data format)
    [keychainQuery setObject:[NSKeyedArchiver archivedDataWithRootObject:data] forKey:(id)kSecValueData];
    //Add item to keychain with the search dictionary
    SecItemAdd((CFDictionaryRef)keychainQuery, NULL);
}

+ (id)loadKeychain:(NSString *)key
{
    id ret = nil;
    NSMutableDictionary *keychainQuery = [self getKeychainQuery:key];
    //Configure the search setting
    //Since in our simple case we are expecting only a single attribute to be returned (the password) we can set the attribute kSecReturnData to kCFBooleanTrue
    [keychainQuery setObject:(id)kCFBooleanTrue forKey:(id)kSecReturnData];
    [keychainQuery setObject:(id)kSecMatchLimitOne forKey:(id)kSecMatchLimit];
    CFDataRef keyData = NULL;
    if (SecItemCopyMatching((CFDictionaryRef)keychainQuery, (CFTypeRef *)&keyData) == noErr) {
        try {
            ret = [NSKeyedUnarchiver unarchiveObjectWithData:(__bridge NSData *)keyData];
        } catch (NSException *e) {
            NSLog(@"Unarchive of %@ failed: %@", key, e);
        }
    }
    if (keyData)
        CFRelease(keyData);
    return ret;
}

+ (void)deleteKeychain:(NSString *)key
{
    NSMutableDictionary *keychainQuery = [self getKeychainQuery:key];
    SecItemDelete((CFDictionaryRef)keychainQuery);
}

+ (NSString *)createNSString:(const char *)cString
{
    if (cString)
        return [NSString stringWithUTF8String: cString];
    else
        return [NSString stringWithUTF8String: ""];
}
@end
