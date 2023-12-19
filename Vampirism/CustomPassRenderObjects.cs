using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering.Universal;
using ThunderRoad;

namespace Vampirism
{
    public class CustomPassRenderObjects : CustomPass<RenderObjects>
    {
        // VARIBALES
        public RenderObjects.RenderObjectsSettings settings = new RenderObjects.RenderObjectsSettings();

        // FUNCTIONS
        public override void UpdateSettings()
        {
            if (settings == null || this.feature == null) return;
            if (this.feature is RenderObjects feature)
            {
                bool flag = feature.settings.overrideMaterial != settings.overrideMaterial;
                feature.settings = this.settings;
                if (flag && this.feature.isActive)
                {
                    DisableFeature();
                    EnableFeature();
                }
            }
            base.UpdateSettings();
        }
    }
}
