//
//  NSDictionary+SafeUseDictionary.h
//  basketball-mobile
//
//  Created by macbook on 2021/10/9.
//
#pragma once

NS_ASSUME_NONNULL_BEGIN

@interface NSDictionary (SafeUseDictionary)
- (bool) hasKey:(id)key;
- (int) optInt:(id)key default:(int) value;
- (BOOL) optBool:(id)key default:(BOOL) value;
- (NSString *) optString:(id)key default:(nullable NSString*) value;
- (id) optObject:(id)key default:(nullable id) value;
@end

@interface NSMutableDictionary (SafeUseDictionary)
- (void) safeSet:(id)value forKey:(id)key;
@end

NS_ASSUME_NONNULL_END
