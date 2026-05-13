using System;
using System.Collections.Generic;
using System.Linq;

namespace PluginSet.Core
{
    public sealed partial class PluginsManager : IIAPurchasePlugin
    {
        private Dictionary<string, IPaymentPlugin> _paymentPlugins;

        private Dictionary<string, IPaymentPlugin> PaymentPlugins
        {
            get
            {
                if (_paymentPlugins == null)
                {
                    _paymentPlugins = new Dictionary<string, IPaymentPlugin>();
                    foreach (var plugin in GetPlugins<IPaymentPlugin>())
                    {
                        _paymentPlugins.Add(plugin.Name, plugin);
                    }
                }

                return _paymentPlugins;
            }
        }

        public bool IsEnablePayment => GetValidLoginTypes().Length > 0;

        public bool IsEnablePayWith(string pluginName)
        {
            if (PaymentPlugins.TryGetValue(pluginName, out var plugin))
                return plugin.IsEnablePayment;

            return false;
        }

        public string[] GetPayTypes()
        {
            return PaymentPlugins.Keys.ToArray();
        }

        public string[] GetValidPayTypes()
        {
            return PaymentPlugins.Where(kv => kv.Value.IsEnablePayment).Select(kv => kv.Key).ToArray();
        }

        public void Pay(string productId, Action<PurchaseResult> callback = null, string jsonData = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                PayWith((string) context.Data, productId, callback, jsonData);
                PluginsEventContext.Return(context);
            };

            if (!NotifyAnyOne(PluginConstants.NOTIFY_CHOOSE_PAYMENT_TYPE, context))
            {
                callback?.Invoke(new PurchaseResult()
                {
                    Success = false,
                    Error = "Pay api need point to a plugin with name",
                    PluginName = string.Empty,
                    Data = $"{{\"productId\":\"{productId}\"}}",
                });

                PluginsEventContext.Return(context);
            }
        }

        public void PayWith(string pluginName, string productId, Action<PurchaseResult> callback = null,
            string jsonData = null)
        {
            if (PaymentPlugins.TryGetValue(pluginName, out var plugin))
            {
                if (plugin.IsEnablePayment)
                {
                    plugin.Pay(productId, callback, jsonData);
                    return;
                }
            }

            callback?.Invoke(new PurchaseResult()
            {
                Success = false,
                Code = PluginConstants.InvalidCode,
                Error = "Invalid plugin name" + pluginName,
                PluginName = pluginName,
                Data = $"{{\"productId\":\"{productId}\"}}",
            });
        }

        public void InitWithProducts(Dictionary<string, ProductType> products, Action<IAPInitResult> callback = null)
        {
            foreach (var kv in PaymentPlugins)
            {
                if (kv.Value is IIAPurchasePlugin purchasePlugin)
                    purchasePlugin.InitWithProducts(products,callback);
            }
        }

        public void InitWithProductsWith(string pluginName, Dictionary<string, ProductType> products, Action<IAPInitResult> callback = null)
        {
            if (PaymentPlugins.TryGetValue(pluginName, out var plugin))
            {
                if (plugin is IIAPurchasePlugin purchasePlugin)
                    purchasePlugin.InitWithProducts(products, callback);
                else
                    Logger.Warn($"Payment plugin {pluginName} need not call InitWithProducts");
            }
        }

        [Obsolete("Use PaymentCompleteWith instead")]
        public void PaymentComplete(string transactionId, Action<Result> callback = null)
        {
            Logger.Error("Cannot call PaymentComplete without plugin name");
        }
        public void PaymentCompleteWith(string pluginName, string transactionId, Action<Result> callback = null)
        {
            if (PaymentPlugins.TryGetValue(pluginName, out var plugin))
            {
                if (plugin is IIAPurchasePlugin purchasePlugin)
                    purchasePlugin.PaymentComplete(transactionId, callback);
                else
                    Logger.Warn($"Payment plugin {pluginName} need not call PaymentComplete");
            }
        }

        [Obsolete("API is deleted")]
        public void AddOnPaymentCompleted(Action<string> completed)
        {
            Logger.Error("API AddOnPaymentCompleted has been deleted!");
        }

        [Obsolete("API is deleted")]
        public void RemoveOnPaymentCompleted(Action<string> completed)
        {
            Logger.Error("API RemoveOnPaymentCompleted has been deleted!");
        }

        [Obsolete("API is deleted")]
        public void AddOnPaymentCompletedWith(string pluginName, Action<string> completed)
        {
            Logger.Error("API AddOnPaymentCompletedWith has been deleted!");
        }

        [Obsolete("API is deleted")]
        public void RemoveOnPaymentCompletedWith(string pluginName, Action<string> completed)
        {
            Logger.Error("API RemoveOnPaymentCompletedWith has been deleted!");
        }

        public void RestorePayments(Action<Result> callback = null, string json = null)
        {
            bool called = false;
            foreach (var kv in PaymentPlugins)
            {
                if (kv.Value is IIAPurchasePlugin purchasePlugin)
                {
                    purchasePlugin.RestorePayments(callback, json);
                    called = true;
                }
            }

            if (!called)
            {
                callback?.Invoke(new Result()
                {
                    Success = false,
                    Code = PluginConstants.InvalidCode,
                    Error = "Invalid platform",
                });
            }
        }

        public void RestorePaymentsWith(string pluginName, Action<Result> callback = null, string json = null)
        {
            if (PaymentPlugins.TryGetValue(pluginName, out var plugin))
            {
                if (plugin is IIAPurchasePlugin purchasePlugin)
                {
                    purchasePlugin.RestorePayments(callback, json);
                    return;
                }
            }
            
            InvalidCallback(callback, pluginName, json);
        }
    }
}
