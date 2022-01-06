namespace PluginSet.Core
{
    public static class AssetBundleEncryption
    {
        public static IEncryption Encryption { set; private get; }

        public static byte[] DecryptBytes(byte[] data, string key = null)
        {
            if (Encryption == null)
                return data;

            return Encryption.Decrypt(data, key);
        }
        
#if UNITY_EDITOR
        public static byte[] EncryptBytes(byte[] data, string key = null)
        {
            if (Encryption == null)
                return data;

            return Encryption.Encrypt(data, key);
        }
#endif
    }
}