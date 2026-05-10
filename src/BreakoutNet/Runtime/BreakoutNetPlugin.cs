using BepInEx;
using UnityEngine;

namespace BreakoutMods.BreakoutNet
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class BreakoutNetPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.breakoutmods.valheim.breakoutnet";
        public const string PluginName = "BreakoutNet";
        public const string PluginVersion = "0.2.0";

        private GameObject runnerObject;

        private void Awake()
        {
            BreakoutLog.Logger = Logger;

            runnerObject = new GameObject("BreakoutNet");
            DontDestroyOnLoad(runnerObject);
            runnerObject.AddComponent<BreakoutNetRunner>();

            BreakoutLog.Info("{0} {1} loaded.", PluginName, PluginVersion);
        }

        private void OnDestroy()
        {
            if (runnerObject != null)
            {
                Destroy(runnerObject);
                runnerObject = null;
            }
        }
    }
}
