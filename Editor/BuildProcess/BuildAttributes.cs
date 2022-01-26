using System;
using System.Collections.Generic;

namespace PluginSet.Core.Editor
{
    // 编译相关代码需要继承该类方便管理
    [AttributeUsage(AttributeTargets.Class)]
    public class BuildToolsAttribute : Attribute
    {
    }

    // 安卓gradle.properties文件中需要加入的选项，属性需返回一个Dict<key:string, value:string>
    // 当有相同的key但value不同时，如果priority相同，则报错，如果priority不同，则使用较高权重的值
    [AttributeUsage(AttributeTargets.Property)]
    public class AndroidGradlePropertiesAttribute : Attribute
    {
        public int Priority = 0;

        public AndroidGradlePropertiesAttribute(int priority)
        {
            Priority = priority;
        }
    }

    // 安卓项目build.gradle文件中需要加入的EXTERNAL_SOURCES, 属性需返回一个List<name:string>
    [AttributeUsage(AttributeTargets.Property)]
    public class AndroidExternalSourcesAttribute : Attribute
    {
    }

    // 安卓项目build.gradle文件中需要加入的APPLY_PLUGINS, 属性需返回一个List<name:string>
    [AttributeUsage(AttributeTargets.Property)]
    public class AndroidApplyPluginsAttribute : Attribute
    {
    }

    // 安卓项目build.gradle文件中需要加入的BUILD_SCRIPT_DEPS，属性需返回一个Dict<name:string, version:string>
    // 自动使用较高版本的依赖库，使用string.Comparer来判断版本高低
    [AttributeUsage(AttributeTargets.Property)]
    public class AndroidBuildScriptAttribute : Attribute
    {
    }

    // 安卓项目build.gradle文件中需要加入的远程依赖库DEPS，属性需返回一个Dict<name:string, version:string>
    // 自动使用较高版本的依赖库，使用string.Comparer来判断版本高低
    [AttributeUsage(AttributeTargets.Property)]
    public class AndroidDependLibsAttribute : Attribute
    {
    }
    
    // 安卓项目设定工程时调用该接口
    // 该接口有序调用，接收BuildProccessorContext, AndroidProjectManager
    [AttributeUsage(AttributeTargets.Method)]
    public class AndroidProjectModifyAttribute : OrderCallBack
    {
        public AndroidProjectModifyAttribute()
            : base(0)
        {
        }

        public AndroidProjectModifyAttribute(int order)
            : base(order)
        {
        }
    }
    

    // 苹果项目设定XCode工程时调用该接口
    // 该接口有序调用，接收BuildProccessorContext, PBXProject
    [AttributeUsage(AttributeTargets.Method)]
    public class iOSXCodeProjectModifyAttribute : OrderCallBack
    {
        public iOSXCodeProjectModifyAttribute()
            : base(0)
        {
        }

        public iOSXCodeProjectModifyAttribute(int order)
            : base(order)
        {
        }
    }

    // 安卓项目中需要加入Metadata的数据
    // 属性需要加在静态方法上，方法接受一个BuildProcessorContext参数
    // 方法需要返回一个Dict<key:string, value:string>结果
    [AttributeUsage(AttributeTargets.Method)]
    public class AndroidMetadataAttribute : Attribute
    {
    }

    // 重置编辑器设置（能保存的配置） 同步设置时触发
    [AttributeUsage(AttributeTargets.Method)]
    public class OnSyncEditorSettingAttribute : OrderCallBack
    {
        public OnSyncEditorSettingAttribute()
            : base(0)
        {
        }

        public OnSyncEditorSettingAttribute(int order)
            : base(order)
        {
        }
    }

    // 编译完成，第一次调用PostProcessScene时
    public class OnCompileCompleteAttribute : OrderCallBack
    {
        public OnCompileCompleteAttribute()
            : base(0)
        {
        }

        public OnCompileCompleteAttribute(int order)
            : base(order)
        {
        }
    }

    // 打所有默认资源包，此时需要将默认资源包都通过AddBuildBundle加入AssetBundle资源包
    [AttributeUsage(AttributeTargets.Method)]
    public class OnBuildBundlesAttribute : OrderCallBack
    {
        public OnBuildBundlesAttribute()
            : base(0)
        {
        }

        public OnBuildBundlesAttribute(int order)
            : base(order)
        {
        }
    }

    // 打所有增量资源包，此时需要将增量资源包都通过AddBuildBundle加入AssetBundle资源包
    [AttributeUsage(AttributeTargets.Method)]
    public class OnBuildPatchesAttribute : OrderCallBack
    {
        public OnBuildPatchesAttribute()
            : base(0)
        {
        }

        public OnBuildPatchesAttribute(int order)
            : base(order)
        {
        }
    }

    // 打bundle完成后调用
    // context: BuildProcessorContext streamingAssetsPath: string manifest: AssetBundleManifest
    [AttributeUsage(AttributeTargets.Method)]
    public class OnBuildBundlesCompletedAttribute : OrderCallBack
    {
        public OnBuildBundlesCompletedAttribute()
            : base(0)
        {
        }

        public OnBuildBundlesCompletedAttribute(int order)
            : base(order)
        {
        }
    }
}
