using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using ThunderRoad;

namespace Vampirism
{
    public class CustomPass<T> : MonoBehaviour
    {
        // VARIABLES
        public ScriptableRendererFeature feature;
        public ScriptableRendererData scriptableRendererData;
        //public ForwardRendererData forwardRendererData;
        private string featureName;

        // FUNCTIONS
        public virtual void Awake()
        {
            GetScriptableRenderer();
        }

        private void GetScriptableRenderer()
        {
            if (TryGetScriptableRendererData(out scriptableRendererData))
            {
                Debug.Log("Vampirism CustomPass: Renderer data found");
                //forwardRendererData = (ForwardRendererData) scriptableRendererData;
            }
            else
            {
                Debug.Log("Vampirism CustomPass: Renderer data not found");
            }
        }

        private bool TryGetScriptableRendererData(out ScriptableRendererData sRD)
        {
            UniversalRenderPipelineAsset renderPipelineAsset = (UniversalRenderPipelineAsset) GraphicsSettings.renderPipelineAsset;
            FieldInfo field = renderPipelineAsset.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
            sRD = ((ScriptableRendererData[])field?.GetValue(renderPipelineAsset))?[0];

            return sRD != null;
        }
        
        public virtual void OnDisable() => DestroyFeature();

        public virtual void OnDestroy() => DestroyFeature();

        public virtual void OnValidate() => UpdateSettings();

        public virtual void UpdateSettings() => scriptableRendererData?.SetDirty();

        public virtual void CreateFeature()
        {
            if (feature != null) DestroyFeature();

            feature = (ScriptableRendererFeature) ScriptableObject.CreateInstance(typeof(T));
            if ((bool)feature)
            {
                featureName = gameObject.name;
                feature.name = featureName;
                feature.SetActive(false);
                if (!(bool)scriptableRendererData) GetScriptableRenderer();
                UpdateSettings();
                scriptableRendererData?.rendererFeatures?.Add(feature);
                scriptableRendererData?.SetDirty();

                Debug.Log("Vampirism CustomPass: Feature created");
            }
            else
            {
                Debug.Log("Vampirism CustomPass: Error creating feature");
            }
        }

        public virtual void DestroyFeature()
        {
            feature?.SetActive(false);
            int rendererIndex = GetRendererIndex(featureName);
            if (rendererIndex >= 0)
            {
                scriptableRendererData?.rendererFeatures.RemoveAt(rendererIndex);
                Debug.Log("Vampirism CustomPass: Feature destroyed");
            }
            else
            {
                Debug.Log("Vampirism CustomPass: Error destroying feature");
            }
        }

        public virtual void DisableFeature()
        {
            feature?.SetActive(false);
            scriptableRendererData?.SetDirty();
            Debug.Log("Vampirism CustomPass: Feature disabled");
        }

        public virtual void EnableFeature()
        {
            if (feature == null)
            {
                CreateFeature();
            }
            else if (GetRendererIndex(featureName) == -1)
            {
                scriptableRendererData?.rendererFeatures?.Add(feature);
            }

            UpdateSettings();
            feature.SetActive(true);
        }

        public int GetRendererIndex(string name)
        {
            int num1 = -1;
            ScriptableRendererData sRD;
            if (TryGetScriptableRendererData(out sRD))
            {
                int index = 0;
                while (true)
                {
                    int num2 = index;
                    int? count = sRD?.rendererFeatures.Count;
                    int valueOrDefault = count.GetValueOrDefault();
                    if (num2 < valueOrDefault & count.HasValue)
                    {
                        if (!sRD.rendererFeatures[index].name.Equals(name))
                            ++index;
                        else
                            break;
                    }
                    else
                        goto label_6;
                }
                sRD.rendererFeatures[index].SetActive(false);
                num1 = index;
            label_6:;
            }
            return num1;
        }
    }
}
