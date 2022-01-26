namespace PluginSet.Core
{
    public interface IAndroidCallable
    {
        void Call(params object[] args);
        T Call<T>(params object[] args);
        
        void CallStatic(params object[] args);
        T CallStatic<T>(params object[] args);
    }
}