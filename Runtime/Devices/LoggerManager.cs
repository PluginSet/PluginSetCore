using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace PluginSet.Core
{
    public static class LoggerManager
    {
        private static Dictionary<string, Logger> _loggers = new Dictionary<string, Logger>();

#if UNITY_ANDROID && !UNITY_EDITOR
        private static Type LoggerType = typeof(AndroidLogger);
#else
        private static Type LoggerType = typeof(UnityLogger);
#endif

        private static LoggerLevel _loggerLevel = LoggerLevel.Debug;

        public static void SetLoggerType<T>() where T : Logger
        {
            SetLoggerType(typeof(T));
        }

        public static void SetLoggerType(Type type)
        {
            LoggerType = type;
        }

        public static void SetLoggerLevel(int level)
        {
            SetLoggerLevel((LoggerLevel) level);
        }

        public static void SetLoggerLevel(LoggerLevel level)
        {
            if (_loggerLevel == level)
                return;
            
            _loggerLevel = level;
            foreach (var logger in _loggers.Values)
            {
                logger.SetLevel(level);
            }
        }
        
        public static Logger GetLogger(string tag)
        {
            if (_loggers.TryGetValue(tag, out var val))
                return val;
            
            var logger = (Logger) Activator.CreateInstance(LoggerType);
            logger.Tag = tag;
            logger.SetLevel(_loggerLevel);
            _loggers.Add(tag, logger);
            return logger;
        }
    }
}