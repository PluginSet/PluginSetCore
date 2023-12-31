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

        public void Pay(string productId, Action<Result> callback = null, string jsonData = null)
        {
            var context = PluginsEventContext.Get(this);
            context.Confirm = delegate
            {
                PayWith((string) context.Data, productId, callback, jsonData);
                PluginsEventContext.Return(context);
            };

            if (!NotifyAnyOne(PluginConstants.NOTIFY_CHOOSE_PAYMENT_TYPE, context))
            {
                callback?.Invoke(new Result
                {
                    Success = false,
                    Error = "Pay api need point to a plugin with name",
                    PluginName = string.Empty,
                    Data = $"{{\"productId\":\"{productId}\"}}",
                });

                PluginsEventContext.Return(context);
            }
        }

        public void PayWith(string pluginName, string productId, Action<Result> callback = null,
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

            InvalidCallback(callback, pluginName, $"{{\"productId\":\"{productId}\"}}");
        }

        public void InitWithProducts(Dictionary<string, int> products)
        {
            foreach (var kv in PaymentPlugins)
            {
                if (kv.Value is IIAPurchasePlugin purchasePlugin)
                    purchasePlugin.InitWithProducts(products);
            }
        }

        public void InitWithProductsWith(string pluginName, Dictionary<string, int> products)
        {
            if (PaymentPlugins.TryGetValue(pluginName, out var plugin))
            {
                if (plugin is IIAPurchasePlugin purchasePlugin)
                    purchasePlugin.InitWithProducts(products);
                else
                    Logger.Warn($"Payment plugin {pluginName} need not call InitWithProducts");
            }
        }

        [Obsolete("Use PaymentCompleteWith instead")]
        public void PaymentComplete(string transactionId)
        {
            Logger.Error("Cannot call PaymentComplete without plugin name");
        }
        public void PaymentCompleteWith(string pluginName, string transactionId)
        {
            if (PaymentPlugins.TryGetValue(pluginName, out var plugin))
            {
                if (plugin is IIAPurchasePlugin purchasePlugin)
                    purchasePlugin.PaymentComplete(transactionId);
                else
                    Logger.Warn($"Payment plugin {pluginName} need not call PaymentComplete");
            }
        }

        [Obsolete("Use AddOnPaymentCompletedWith instead")]
        public void AddOnPaymentCompleted(Action<string> completed)
        {
            Logger.Error("Cannot call AddOnPaymentCompleted without plugin name");
        }

        [Obsolete("Use AddOnPaymentCompletedWith instead")]
        public void RemoveOnPaymentCompleted(Action<string> completed)
        {
            Logger.Error("Cannot call RemoveOnPaymentCompleted without plugin name");
        }

        public void AddOnPaymentCompletedWith(string pluginName, Action<string> completed)
        {
            if (PaymentPlugins.TryGetValue(pluginName, out var plugin))
            {
                if (plugin is IIAPurchasePlugin purchasePlugin)
                    purchasePlugin.AddOnPaymentCompleted(completed);
                else
                    Logger.Warn($"Payment plugin {pluginName} need not call AddOnPaymentCompleted");
            }
        }

        public void RemoveOnPaymentCompletedWith(string pluginName, Action<string> completed)
        {
            if (PaymentPlugins.TryGetValue(pluginName, out var plugin))
            {
                if (plugin is IIAPurchasePlugin purchasePlugin)
                    purchasePlugin.RemoveOnPaymentCompleted(completed);
                else
                    Logger.Warn($"Payment plugin {pluginName} need not call RemoveOnPaymentCompleted");
            }
        }

        [Obsolete("Use RestorePaymentsWith instead")]
        public void RestorePayments(Action<Result> callback = null, string json = null)
        {
            Logger.Error("Cannot call RestorePayments without plugin name");
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
