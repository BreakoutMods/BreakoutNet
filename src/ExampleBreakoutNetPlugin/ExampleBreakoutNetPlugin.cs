using BepInEx;
using BepInEx.Logging;
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
        private const string NamedEvent = "breakoutnet.example.public.loaded";

        internal static ManualLogSource ExampleLog { get; private set; }

        private BreakoutModApp breakoutApp;
        private float nextClientRequest;

        private void Awake()
        {
            ExampleLog = Logger;
            breakoutApp = BreakoutNet.ForPlugin(this, PluginGuid)
                .AddShared<ExampleSharedModule>()
                .AddServer<ExampleServerModule>()
                .AddClient<ExampleClientModule>()
                .Build();

            BreakoutRpc.Server.Register<JoinCheckRequest>(RequestRpc, OnJoinCheckRequest);
            BreakoutRpc.Client.Register<JoinCheckResult>(ResultRpc, OnJoinCheckResult);

            BreakoutSettingsSync.RegisterServerSettings(SettingsName, GetServerSettings);
            BreakoutSettingsSync.Client.Register<ExampleServerSettings>(SettingsName, ApplyServerSettings);

            breakoutApp.Context.Events.Publish(NamedEvent, new ExampleNamedEvent("Example plugin published a named extension event."));

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

        private void OnDestroy()
        {
            if (breakoutApp != null)
            {
                breakoutApp.Dispose();
                breakoutApp = null;
            }
        }
    }

    internal sealed class ExampleSharedModule : BreakoutSharedModule
    {
        public override void Initialize(BreakoutModuleContext context)
        {
            base.Initialize(context);
            Context.Hooks.OnNetworkReady(OnNetworkReady);
        }

        private void OnNetworkReady(BreakoutNetworkReadyEvent evt)
        {
            Context.Events.Publish(new ExampleCustomEvent("Network is ready inside the shared module."));
        }
    }

    internal sealed class ExampleClientModule : BreakoutClientModule
    {
        public override void Initialize(BreakoutModuleContext context)
        {
            base.Initialize(context);
            Context.Events.Subscribe<ExampleCustomEvent>(OnCustomEvent);
            Context.Events.Subscribe<ExampleNamedEvent>("breakoutnet.example.public.loaded", OnNamedEvent);
        }

        private static void OnCustomEvent(ExampleCustomEvent evt)
        {
            ExampleBreakoutNetPlugin.ExampleLog.LogInfo("Client module received custom event: " + evt.Message);
        }

        private static void OnNamedEvent(ExampleNamedEvent evt)
        {
            ExampleBreakoutNetPlugin.ExampleLog.LogInfo("Client module received named event: " + evt.Message);
        }
    }

    internal sealed class ExampleServerModule : BreakoutServerModule
    {
        public override void Initialize(BreakoutModuleContext context)
        {
            base.Initialize(context);
            Context.Hooks.OnPeerJoined(OnPeerJoined);
        }

        private static void OnPeerJoined(BreakoutPeerChangedEvent evt)
        {
            ExampleBreakoutNetPlugin.ExampleLog.LogInfo("Server module observed peer join: " + evt.PeerId);
        }
    }

    public sealed class ExampleCustomEvent : IBreakoutEvent
    {
        public ExampleCustomEvent(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }

    public sealed class ExampleNamedEvent : IBreakoutEvent
    {
        public ExampleNamedEvent(string message)
        {
            Message = message;
        }

        public string Message { get; }
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
