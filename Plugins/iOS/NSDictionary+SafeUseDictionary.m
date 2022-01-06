//
//  NSDictionary+SafeUseDictionary.m
//  basketball-mobile
//
//  Created by macbook on 2021/10/9.
//

#import "NSDictionary+SafeUseDictionary.h"

@implementation NSDictionary (SafeUseDictionary)

- (bool) hasKey:(id)key
{
    return [[self allKeys] containsObject:key];
}

- (int) optInt:(id)key default:(int)value
{
    if ([self hasKey:key])
    {
        return [[self valueForKey:key] intValue];
    }
    return value;
}


- (BOOL) optBool:(id)key default:(BOOL) value
{
    if ([self hasKey:key])
    {
        return [[self valueForKey:key] boolValue];
    }
    return value;
}

- (NSString*) optString:(id)key default:(nullable NSString *)value
{
    if ([self hasKey:key])
    {
        return [NSString stringWithFormat:@"%@", [self valueForKey:key]];
    }
    
    return value;
}

- (id) optObject:(id)key default:(id)value
{
    if ([self hasKey:key]) {
        return [self valueForKey:key];
    }
    
    return value;
}

@end

@implementation NSMutableDictionary (SafeUseDictionary)
- (void) safeSet:(id)value forKey:(id)key
{
    if (!value) return;
    [self setValue:value forKey:key];
}
@end
