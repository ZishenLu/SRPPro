using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEngine.CustomPipeline
{
    public enum LightRenderingMode
    {
        Disabled = 0,
        PerVertex = 1,
        PerPixel = 2,
    }
    public partial class CustomPipelineAssets : RenderPipelineAsset
    {
        [SerializeField]bool m_SRPBatcher;
        [SerializeField]bool m_GPUInstance;
        [SerializeField]bool m_DynamicBatcher;
        [SerializeField]LightRenderingMode m_MainLightRenderMode = LightRenderingMode.PerPixel;
        [SerializeField]LightRenderingMode m_AdditionalLightsRenderMode = LightRenderingMode.PerPixel;
        [SerializeField]int m_MaxAdditionalLightsCount = 8;

        public bool useSRPBatcher 
        {
            get { return m_SRPBatcher; }
            set { m_SRPBatcher = value; }
        }
        public bool useGPUInstance
        {
            get { return m_GPUInstance; }
            set { m_GPUInstance = value; }
        }
        public bool useDynamicBatcher
        {
            get { return m_DynamicBatcher; }
            set { m_DynamicBatcher = value; }
        }
        public LightRenderingMode MainLightRenderMode
        {
            get { return m_MainLightRenderMode; }
            set { m_MainLightRenderMode = value; }
        }
        public LightRenderingMode AdditionalLightsRenderMode
        {
            get { return m_AdditionalLightsRenderMode; }
            set { m_AdditionalLightsRenderMode = value; }
        }
        public int MaxAdditionalLightsCount
        {
            get { return m_MaxAdditionalLightsCount; }
            set { m_MaxAdditionalLightsCount = value; }
        }
    }
}
