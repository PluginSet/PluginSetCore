using System;
using System.Collections.Generic;
using UnityEngine;

namespace PluginSet.Core
{
    public abstract class PluginBase: MonoBehaviour, IPluginBase
    {
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

        protected void AddEventListener(string eventName, Action callback)
        {
            dispatcher.AddEventListener(eventName, callback);
        }
        
        protected void AddEventListener(string eventName, Action<PluginsEventContext> callback)
        {
            dispatcher.AddEventListener(eventName, callback);
        }
        
        protected void RemoveEventListener(string eventName, Action callback)
        {
            dispatcher.RemoveEventListener(eventName, callback);
        }
        
        protected void RemoveEventListener(string eventName, Action<PluginsEventContext> callback)
        {
            dispatcher.RemoveEventListener(eventName, callback);
        }

        protected void RemoveEventListeners(string eventName)
        {
            dispatcher.RemoveEventListeners(eventName);
        }
        
        protected void RemoveEventListeners()
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