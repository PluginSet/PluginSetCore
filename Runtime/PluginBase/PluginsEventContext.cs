using System;

namespace PluginSet.Core
{
    public class PluginsEventContext: SimpleDataSet
    {
        private static ObjectPool<PluginsEventContext> _pool = new ObjectPool<PluginsEventContext>(10);

        public static PluginsEventContext Get(PluginBase plugin)
        {
            var ret = _pool.Get();
            ret.Plugin = plugin;
            ret.Name = plugin.Name;
            ret.IsStopPropagation = false;
            return ret;
        }

        public static void Return(PluginsEventContext context)
        {
            context.Reset();
            _pool.Put(context);
        }

        public void Reset()
        {
            this.Clear();
            this.Data = null;
            this.Confirm = null;
            this.Cancel = null;
            this.IsStopPropagation = false;
        }

        public PluginBase Plugin { get; protected set; }
        public string Name { get; protected set; }
        public string EventName;

        public object Data;

        public Action Confirm;
        public Action Cancel;

        public bool IsStopPropagation { get; protected set; }

        public void StopPropagation()
        {
            IsStopPropagation = true;
        }
    }
}