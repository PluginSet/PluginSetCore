using System;

namespace PluginSet.Core.Editor
{
	// 初始化框架时调用的方法，检测资源完整
	[AttributeUsage(AttributeTargets.Method)]
    public class OnFrameworkInitAttribute : OrderCallBack
    {
        public OnFrameworkInitAttribute()
            : base(0) {}

        public OnFrameworkInitAttribute(int order)
	        : base(order) {}
    }
}