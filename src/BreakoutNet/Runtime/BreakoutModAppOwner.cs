using System.Collections.Generic;
using UnityEngine;

namespace BreakoutMods.BreakoutNet
{
    internal sealed class BreakoutModAppOwner : MonoBehaviour
    {
        private readonly List<BreakoutModApp> apps = new List<BreakoutModApp>();

        public void Add(BreakoutModApp app)
        {
            if (app != null && !apps.Contains(app))
            {
                apps.Add(app);
            }
        }

        private void OnDestroy()
        {
            foreach (BreakoutModApp app in apps.ToArray())
            {
                app.Dispose();
            }

            apps.Clear();
        }
    }
}
