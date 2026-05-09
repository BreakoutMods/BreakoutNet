using System;
using System.Collections.Generic;

namespace BreakoutMods.BreakoutNet
{
    internal static class BreakoutSettingsSyncRegistry
    {
        public const float BroadcastIntervalSeconds = 10f;

        private const string RpcPrefix = "breakoutnet.settings.";
        private static readonly Dictionary<string, ISettingsProvider> ServerProviders = new Dictionary<string, ISettingsProvider>();

        public static void RegisterServerSettings<TSettings>(string settingsName, Func<TSettings> getSettings)
            where TSettings : IBreakoutSerializable, new()
        {
            if (string.IsNullOrWhiteSpace(settingsName))
            {
                throw new ArgumentException("Settings name cannot be empty.", nameof(settingsName));
            }

            if (getSettings == null)
            {
                throw new ArgumentNullException(nameof(getSettings));
            }

            ServerProviders[settingsName] = new SettingsProvider<TSettings>(settingsName, getSettings);
            BreakoutLog.Info("Registered server settings sync '{0}'.", settingsName);
        }

        public static void RegisterClientSettings<TSettings>(string settingsName, Action<TSettings> applySettings)
            where TSettings : IBreakoutSerializable, new()
        {
            if (string.IsNullOrWhiteSpace(settingsName))
            {
                throw new ArgumentException("Settings name cannot be empty.", nameof(settingsName));
            }

            if (applySettings == null)
            {
                throw new ArgumentNullException(nameof(applySettings));
            }

            string rpcName = BuildRpcName(settingsName);
            BreakoutRpc.Client.Register<TSettings>(
                rpcName,
                (context, settings) =>
                {
                    if (!context.IsFromServer)
                    {
                        context.Reject("Settings sync packets are only accepted from the server.");
                        return;
                    }

                    applySettings(settings);
                });

            BreakoutLog.Info("Registered client settings sync '{0}'.", settingsName);
        }

        public static void BroadcastServerSettings()
        {
            if (!BreakoutSide.IsServer || ZRoutedRpc.instance == null)
            {
                return;
            }

            foreach (ISettingsProvider provider in ServerProviders.Values)
            {
                provider.Broadcast();
            }
        }

        private static string BuildRpcName(string settingsName)
        {
            return RpcPrefix + settingsName;
        }

        private interface ISettingsProvider
        {
            void Broadcast();
        }

        private sealed class SettingsProvider<TSettings> : ISettingsProvider
            where TSettings : IBreakoutSerializable, new()
        {
            private readonly string settingsName;
            private readonly Func<TSettings> getSettings;

            public SettingsProvider(string settingsName, Func<TSettings> getSettings)
            {
                this.settingsName = settingsName;
                this.getSettings = getSettings;
            }

            public void Broadcast()
            {
                try
                {
                    TSettings settings = getSettings();
                    if (settings == null)
                    {
                        BreakoutLog.Warning("Settings sync '{0}' returned null and was not broadcast.", settingsName);
                        return;
                    }

                    BreakoutRpc.Server.Broadcast(BuildRpcName(settingsName), settings, BreakoutNetPlugin.PluginGuid);
                }
                catch (Exception ex)
                {
                    BreakoutLog.Error("Settings sync '{0}' failed: {1}", settingsName, ex);
                }
            }
        }
    }
}
