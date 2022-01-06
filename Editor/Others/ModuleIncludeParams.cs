using UnityEngine;

namespace PluginSet.Core.Editor
{
    [BuildChannelsParams("ModuleInclude", "模块包含记录(在打包前最好确认该数据是否正常)")]
    public class ModuleIncludeParams: ScriptableObject
    {
        [Tooltip("是否包含com.unity.modules.physics")]
        public bool IncludePhysics;

        [Tooltip("是否包含com.unity.modules.audio")]
        public bool IncludeAudio;

        [Tooltip("是否包含com.unity.modules.animation")]
        public bool IncludeAnimation;

        [Tooltip("是否包含com.unity.modules.imageconversion")]
        public bool IncludeImageConversion;

        [Tooltip("是否包含com.unity.modules.unitywebrequesttexture")]
        public bool IncludeUnityWebRequestTexture;
    }
}