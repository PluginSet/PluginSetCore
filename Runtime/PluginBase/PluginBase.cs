using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PluginSet.Core
{
    public abstract class PluginBase: MonoBehaviour, IPluginBase
    {
        [AttributeUsage(AttributeTargets.Method)]
        protected abstract class ExecutableAttribute : Attribute
        {
        }
        
        [AttributeUsage(AttributeTargets.Method)]
        protected abstract class YieldAttribute : Attribute
        {
            public int Order { get; private set; }
            
            public YieldAttribute(int order)
            {
                Order = order;
            }
        }
        
        private class MethodOrder
        {
            public MethodBase Method;
            public int Order;
        }
        
        static int getYieldMethodOrder<T>(MethodBase info) where T : YieldAttribute
        {
            return ((YieldAttribute) info.GetCustomAttributes(typeof(T), false).First()).Order;
        }
        
        protected readonly EventDispatcher<PluginsEventContext> dispatcher = PluginEventDispatcher.GlobalDispatcher;

        protected PluginsManager _managerInstance;

        internal void SetPluginsMangerInstance(PluginsManager inst)
        {
            _managerInstance = inst;
        }

        protected virtual void Awake()
        {
            Init(PluginSetConfig.Asset);
        }

        public abstract string Name { get; }

        protected virtual void Init(PluginSetConfig config)
        {
        }
        
        protected void ExecuteAll<T>(params object[] args) where T: ExecutableAttribute
        {
            var executeType = typeof(T);
            foreach (var method in GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(method => method.IsDefined(executeType)))
            {
                method.Invoke(this, args);
            }
        }

        protected IEnumerator YieldAll<T>(params object[] args) where T : YieldAttribute
        {
            var executeType = typeof(T);
            var methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(method => method.IsDefined(executeType));
            
            List<MethodOrder> methodOrders = new List<MethodOrder>();
            foreach (var method in methods)
            {
                methodOrders.Add(new MethodOrder {Method = method, Order = getYieldMethodOrder<T>(method)});
            }
        
            methodOrders.Sort((a, b) =>
            {
                return a.Order < b.Order ? -1 : (a.Order > b.Order ? 1 : 0);
            });
            
            var count = methodOrders.Count;
            for (int i = 0; i < count; i++)
            {
                yield return (IEnumerator)methodOrders[i].Method.Invoke(this, args);
            }
        }

        public void AddEventListener(string eventName, Action callback)
        {
            dispatcher.AddEventListener(eventName, callback);
        }
        
        public void AddEventListener(string eventName, Action<PluginsEventContext> callback)
        {
            dispatcher.AddEventListener(eventName, callback);
        }
        
        public void RemoveEventListener(string eventName, Action callback)
        {
            dispatcher.RemoveEventListener(eventName, callback);
        }
        
        public void RemoveEventListener(string eventName, Action<PluginsEventContext> callback)
        {
            dispatcher.RemoveEventListener(eventName, callback);
        }

        public void RemoveEventListeners(string eventName)
        {
            dispatcher.RemoveEventListeners(eventName);
        }
        
        public void RemoveEventListeners()
        {
            dispatcher.RemoveEventListeners();
        }

        public bool SendNotification(string eventName, PluginsEventContext context)
        {
            context.EventName = eventName;
            return dispatcher.DispatchEvent(eventName, context);
        }

        public bool SendNotification(string eventName, object data = null
            , Action confirm = null, Action cancel = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Data = data;
            context.Confirm = confirm;
            context.Cancel = cancel;
            var result = SendNotification(eventName, context);
            PluginsEventContext.Return(context);
            return result;
        }
        
        public bool NotifyAnyOne(string eventName, PluginsEventContext context)
        {
            context.EventName = eventName;
            return dispatcher.DispatchOne(eventName, context);
        }

        public bool NotifyAnyOne(string eventName, object data = null
            , Action confirm = null, Action cancel = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Data = data;
            context.Confirm = confirm;
            context.Cancel = cancel;
            var result = NotifyAnyOne(eventName, context);
            PluginsEventContext.Return(context);
            return result;
        }
    }
}