using BepInEx;
using BreakoutMods.BreakoutNet;
using UnityEngine;

namespace BreakoutMods.BreakoutNet.Example
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(BreakoutNetPlugin.PluginGuid)]
    public sealed class ExampleBreakoutNetPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.breakoutmods.valheim.breakoutnet.example";
        public const string PluginName = "Example BreakoutNet Plugin";
        public const string PluginVersion = "0.1.0";

        private const string RequestRpc = "breakoutnet.example.joincheck";
        private const string ResultRpc = "breakoutnet.example.joincheck.result";
        private const string SettingsName = "breakoutnet.example.server";

        private float nextClientRequest;

        private void Awake()
        {
            BreakoutRpc.Server.Register<JoinCheckRequest>(RequestRpc, OnJoinCheckRequest);
            BreakoutRpc.Client.Register<JoinCheckResult>(ResultRpc, OnJoinCheckResult);

            BreakoutSettingsSync.RegisterServerSettings(SettingsName, GetServerSettings);
            BreakoutSettingsSync.Client.Register<ExampleServerSettings>(SettingsName, ApplyServerSettings);

            Logger.LogInfo("Example BreakoutNet plugin loaded. Press F8 in world to send a sample request.");
        }

        private void Update()
        {
            if (Application.isBatchMode || !BreakoutSide.IsInWorld || Time.time < nextClientRequest)
            {
                return;
            }

            if (!Input.GetKeyDown(KeyCode.F8))
            {
                return;
            }

            nextClientRequest = Time.time + 1f;
            BreakoutRpc.Client.SendToServer(
                RequestRpc,
                new JoinCheckRequest { ModListHash = "example-dev-hash" },
                PluginGuid);
        }

        private void OnJoinCheckRequest(BreakoutRpcContext context, JoinCheckRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ModListHash))
            {
                context.Reject("Client did not send a mod list hash.");
                return;
            }

            Logger.LogInfo($"Received example join check from {context.SenderPeerId}: {request.ModListHash}");

            BreakoutRpc.Server.SendToClient(
                context.SenderPeerId,
                ResultRpc,
                new JoinCheckResult
                {
                    Allowed = true,
                    Message = "Example request accepted by server."
                },
                PluginGuid);
        }

        private void OnJoinCheckResult(BreakoutRpcContext context, JoinCheckResult result)
        {
            if (!context.IsFromServer)
            {
                context.Reject("Join check result must come from the server.");
                return;
            }

            Logger.LogInfo($"Server replied: allowed={result.Allowed}, message='{result.Message}'");
        }

        private ExampleServerSettings GetServerSettings()
        {
            return new ExampleServerSettings
            {
                VoiceRadius = 32f,
                RequireMatchingMods = true
            };
        }

        private void ApplyServerSettings(ExampleServerSettings settings)
        {
            Logger.LogInfo($"Applied example settings: radius={settings.VoiceRadius}, requireMods={settings.RequireMatchingMods}");
        }
    }

    public sealed class JoinCheckRequest : IBreakoutSerializable
    {
        public string ModListHash;

        public void Write(ZPackage package)
        {
            package.Write(ModListHash ?? string.Empty);
        }

        public void Read(ZPackage package)
        {
            ModListHash = package.ReadString();
        }
    }

    public sealed class JoinCheckResult : IBreakoutSerializable
    {
        public bool Allowed;
        public string Message;

        public void Write(ZPackage package)
        {
            package.Write(Allowed);
            package.Write(Message ?? string.Empty);
        }

        public void Read(ZPackage package)
        {
            Allowed = package.ReadBool();
            Message = package.ReadString();
        }
    }

    public sealed class ExampleServerSettings : IBreakoutSerializable
    {
        public float VoiceRadius;
        public bool RequireMatchingMods;

        public void Write(ZPackage package)
        {
            package.Write(VoiceRadius);
            package.Write(RequireMatchingMods);
        }

        public void Read(ZPackage package)
        {
            VoiceRadius = package.ReadSingle();
            RequireMatchingMods = package.ReadBool();
        }
    }
}
