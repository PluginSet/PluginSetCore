
namespace PluginSet.Core
{
    public enum LoggerLevel
    {
        Debug,
        Warn,
        Info,
        None,
    }
    
    public abstract class Logger
    {
        public virtual string Tag { get; set; }

        protected abstract void DoLogDebug(string msg, params object[] args);
        
        protected abstract void DoLogWarn(string msg, params object[] args);
        
        protected abstract void DoLogError(string msg, params object[] args);

        protected abstract void DoLogInfo(string msg, params object[] args);
        
        protected abstract void DoLogDebug(string msg);
        
        protected abstract void DoLogWarn(string msg);
        
        protected abstract void DoLogError(string msg);

        protected abstract void DoLogInfo(string msg);

        protected int _level = (int)LoggerLevel.None;

        private const int DebugLevel = (int) LoggerLevel.Debug;
        private const int WarnLevel = (int) LoggerLevel.Warn;
        private const int InfoLevel = (int) LoggerLevel.Info;

        public void SetLevel(LoggerLevel level)
        {
            SetLevel((int)level);
        }

        public void SetLevel(int level)
        {
            _level = level;
        }

        public void Debug(string msg, params object[] args)
        {
            if (_level > DebugLevel)
                return;
            
            if (args != null && args.Length > 0)
                DoLogDebug(msg, args);
            else
                DoLogDebug(msg);
        }

        public void Warn(string msg, params object[] args)
        {
            if (_level > WarnLevel)
                return;
            
            if (args != null && args.Length > 0)
                DoLogWarn(msg, args);
            else
                DoLogWarn(msg);
        }

        public void Error(string msg, params object[] args)
        {
            if (args != null && args.Length > 0)
                DoLogError(msg, args);
            else
                DoLogError(msg);
        }

        public void Info(string msg, params object[] args)
        {
            if (_level > InfoLevel)
                return;
           
            if (args != null && args.Length > 0)
                DoLogInfo(msg, args);
            else
                DoLogInfo(msg);
        }
    }
}