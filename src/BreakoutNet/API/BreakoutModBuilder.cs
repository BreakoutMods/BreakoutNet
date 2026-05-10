using System;
using System.Collections.Generic;
using BepInEx;
using UnityEngine;

namespace BreakoutMods.BreakoutNet
{
    public sealed class BreakoutModBuilder
    {
        private readonly BaseUnityPlugin plugin;
        private readonly string modGuid;
        private readonly List<Type> sharedModules = new List<Type>();
        private readonly List<Type> serverModules = new List<Type>();
        private readonly List<Type> clientModules = new List<Type>();

        internal BreakoutModBuilder(BaseUnityPlugin plugin, string modGuid)
        {
            if (string.IsNullOrWhiteSpace(modGuid))
            {
                throw new ArgumentException("Mod GUID cannot be empty.", nameof(modGuid));
            }

            this.plugin = plugin;
            this.modGuid = modGuid;
        }

        public BreakoutModBuilder AddShared<TModule>()
            where TModule : IBreakoutModule, new()
        {
            sharedModules.Add(typeof(TModule));
            return this;
        }

        public BreakoutModBuilder AddServer<TModule>()
            where TModule : IBreakoutModule, new()
        {
            serverModules.Add(typeof(TModule));
            return this;
        }

        public BreakoutModBuilder AddClient<TModule>()
            where TModule : IBreakoutModule, new()
        {
            clientModules.Add(typeof(TModule));
            return this;
        }

        public BreakoutModApp Build()
        {
            BreakoutModApp app = new BreakoutModApp(modGuid);
            BreakoutModuleContext context = app.Context;

            CreateModules(sharedModules, context, app);
            CreateModules(serverModules, context, app);

            if (!Application.isBatchMode)
            {
                CreateModules(clientModules, context, app);
            }

            BreakoutModAppOwner owner = plugin.gameObject.GetComponent<BreakoutModAppOwner>();
            if (owner == null)
            {
                owner = plugin.gameObject.AddComponent<BreakoutModAppOwner>();
            }

            owner.Add(app);
            return app;
        }

        private static void CreateModules(IEnumerable<Type> moduleTypes, BreakoutModuleContext context, BreakoutModApp app)
        {
            foreach (Type moduleType in moduleTypes)
            {
                IBreakoutModule module = (IBreakoutModule)Activator.CreateInstance(moduleType);
                module.Initialize(context);
                app.AddModule(module);
            }
        }
    }
}
