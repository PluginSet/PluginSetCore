namespace PluginSet.Core
{
    public interface IEncryption
    {
        byte[] Decrypt(byte[] data, string key = null);
#if UNITY_EDITOR
        byte[] Encrypt(byte[] data, string key = null);
#endif
    }
}