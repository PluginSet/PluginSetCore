#pragma once

@interface PluginUtils : NSObject
+(NSString*)readStringFromPlist:(NSString*)key defaultValue:(NSString*) value;

+(BOOL)readBooleanFromPlist:(NSString*)key defaultValue:(BOOL) value;

+(NSString*)dictToJson:(NSDictionary*)dict;

+(NSDictionary*)jsonToDict:(NSString*)json;

+(NSMutableDictionary *)getKeychainQuery:(NSString *)key;

+(void)saveKeychain:(NSString*)key data:(id)data;

+(id)loadKeychain:(NSString*)key;

+(void)deleteKeychain:(NSString*)key;

+(NSString *)createNSString:(const char *)cString;
@end

