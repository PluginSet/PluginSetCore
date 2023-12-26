using System;
using System.Collections.Generic;

namespace PluginSet.Core.Editor
{
    // 编译相关代码需要继承该类方便管理
    [AttributeUsage(AttributeTargets.Class)]
    public class BuildToolsAttribute : Attribute
    {
    }
    
    // 项目导出完成回调
    [AttributeUsage(AttributeTargets.Method)]
    public class CheckRebuildAssetBundlesAttribute : Attribute
    {
        public CheckRebuildAssetBundlesAttribute()
        {
        }
    }
    
    // 构建完成后回调，接受BuildProccessorContext
    [AttributeUsage(AttributeTargets.Method)]
    public class BuildProjectCompletedAttribute : OrderCallBack
    {
        public BuildProjectCompletedAttribute()
            : base(0)
        {
            
        }

        public BuildProjectCompletedAttribute(int order)
            : base(order)
        {
            
        }
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

    // WebGL项目设定项目导出目录时调用该接口
    // 该接口有序调用，接收BuildProccessorContext, string
    [AttributeUsage(AttributeTargets.Method)]
    public class WebGLProjectModifyAttribute : OrderCallBack
    {
        public WebGLProjectModifyAttribute()
            : base(0)
        {
        }

        public WebGLProjectModifyAttribute(int order)
            : base(order)
        {
        }
    }
    
    // 安卓项目多渠道ID数据构建时，不重新导出项目，直接修改安卓工程
    // 该接口有序调用，接收BuildProccessorContext, AndroidProjectManager
    public class AndroidMultipleBuildSetupAttribute : OrderCallBack
    {
        public AndroidMultipleBuildSetupAttribute()
            : base(0)
        {
            
        }

        public AndroidMultipleBuildSetupAttribute(int order)
            : base(order)
        {
            
        }
    }
    
    // 苹果项目多渠道ID数据构建时，不重新导出项目，直接修改安卓工程（预留，完整的打包流程不支持苹果项目多渠道打包）
    // 该接口有序调用，接收BuildProccessorContext, PBXProject
    [AttributeUsage(AttributeTargets.Method)]
    public class iOSMultipleBuildSetupAttribute : OrderCallBack
    {
        public iOSMultipleBuildSetupAttribute()
            : base(0)
        {
        }

        public iOSMultipleBuildSetupAttribute(int order)
            : base(order)
        {
        }
    }

    // WebGL项目多渠道ID数据构建时，不重新导出项目，直接修改安卓工程
    // 该接口有序调用，接收BuildProccessorContext, string
    public class WebGLMultipleBuildSetupAttribute : OrderCallBack
    {
        public WebGLMultipleBuildSetupAttribute()
            : base(0)
        {
            
        }

        public WebGLMultipleBuildSetupAttribute(int order)
            : base(order)
        {
            
        }
    }
    
    /**
     * 项目构建完成后回调(在fabfile中调用的流程，不使用fab命令打包时，不会进入该流程。该流程主要在于安卓build完成之后才会生成symbols文件）
     * 该接口有序调用，接收BuildProccessorContext
     */
    [AttributeUsage(AttributeTargets.Method)]
    public class BuildCompletedCallbackAttribute : OrderCallBack
    {
        public BuildCompletedCallbackAttribute()
            : base(0)
        {
            
        }

        public BuildCompletedCallbackAttribute(int order)
            : base(order)
        {
            
        }
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class AssetBundleFilePathsCollectorAttribute : OrderCallBack
    {
        public AssetBundleFilePathsCollectorAttribute()
            : base(0)
        {
            
        }

        public AssetBundleFilePathsCollectorAttribute(int order)
            : base(order)
        {
            
        }
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

    // 资源准备完成，在构建app之前调用
    // context: BuildProcessorContext  assetsPath: string
    public class AssetsPreparedAttribute : OrderCallBack
    {
        public AssetsPreparedAttribute()
            : base(0)
        {
        }

        public AssetsPreparedAttribute(int order)
            : base(order)
        {
        }
    }
}
