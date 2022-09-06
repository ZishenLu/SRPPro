using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEngine;
using Unity.Collections;

namespace UnityEngine.CustomPipeline
{
    public sealed partial class CustomPipeline : RenderPipeline
    {
        CustomPipelineAssets m_Asset;
        // {
        //     get => GraphicsSettings.currentRenderPipeline as CustomPipelineAssets;
        // } 
        public static int maxPerObjectLights
        {
            get => (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2) ? 4 : 8;
        }
        internal const int k_MaxVisibleAdditionalLightsMobileShaderLevelLessThan45 = 16;
        internal const int k_MaxVisibleAdditionalLightsMobile = 32;
        internal const int k_MaxVisibleAdditionalLightsNonMobile = 256;
        public static int maxVisibleAdditionalLights
        {
            get
            {
                bool isMobile = Application.isMobilePlatform;
                if(isMobile && (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2 || 
                    (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 && Graphics.minOpenGLESVersion <= OpenGLESVersion.OpenGLES30)))
                    return k_MaxVisibleAdditionalLightsMobileShaderLevelLessThan45;
                return (isMobile || SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore || 
                    SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2 || SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3)
                    ? k_MaxVisibleAdditionalLightsMobile : k_MaxVisibleAdditionalLightsNonMobile;
            }
        }
        public CustomPipeline(CustomPipelineAssets asset)
        {
            m_Asset = asset;
        } 
        protected override void Render(ScriptableRenderContext context,Camera[] cameras)
        {
            foreach (var camera in cameras)
            {
#if UNITY_EDITOR
                if(camera.cameraType == CameraType.SceneView)
                {
                    ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
                }
#endif
                context.SetupCameraProperties(camera);
                camera.TryGetCullingParameters(out var cullingParameters);
                var result = context.Cull(ref cullingParameters);
                CameraData cameraData = new CameraData
                {
                    camera = camera,
                };
                InitializeRenderingData(m_Asset,ref cameraData,ref result,out RenderingData renderingData);
                GraphicsSettings.lightsUseLinearIntensity = (QualitySettings.activeColorSpace == ColorSpace.Linear);
                GraphicsSettings.useScriptableRenderPipelineBatching = m_Asset.useSRPBatcher;
                var render = m_Asset.customRender;
                render.Clear();
                render.Setup(context,ref renderingData);
                render.Execute(context,renderingData);
                if(Handles.ShouldRenderGizmos())
                {
                    context.DrawGizmos(camera,GizmoSubset.PostImageEffects);
                    context.DrawGizmos(camera,GizmoSubset.PreImageEffects);
                }

            }
            context.Submit();
            return;
        }
        static int GetMainLightIndex(CustomPipelineAssets assets,
            NativeArray<VisibleLight> visibleLights
        )
        {
            Light sunLight = RenderSettings.sun;
            int totalVisibleLightsLength = visibleLights.Length;
            int brightestLightIndex = -1;
            float brieghtstLightIntensity = 0.0f;

            for (int i = 0; i < totalVisibleLightsLength; i++)
            {
                var curLight = visibleLights[i].light;
                if (curLight == null)break;
                
                if (visibleLights[i].lightType == LightType.Directional)
                {
                    if(curLight == sunLight)return i;
                    if(curLight.intensity > brieghtstLightIntensity)
                    {
                        brightestLightIndex = i;
                        brieghtstLightIntensity = curLight.intensity;
                    }
                }
            }
            return brightestLightIndex;
        }
        static void InitializeRenderingData(CustomPipelineAssets assets,
            ref CameraData cameraData,ref CullingResults cullingResults,
            out RenderingData renderingData
        )
        {
            var visibleLights = cullingResults.visibleLights;
            int mainLightIndex = GetMainLightIndex(assets,visibleLights);
            renderingData = new RenderingData
            {
                cameraData = cameraData,
                cullingResults = cullingResults,
                useDynamicBatcher = assets.useDynamicBatcher,
                useGPUInstance = assets.useGPUInstance,
            };
            InitializeLightData(assets,cullingResults.visibleLights,mainLightIndex,out renderingData.lightData);
        }
        static void InitializeLightData(CustomPipelineAssets assets,
            NativeArray<VisibleLight> visibleLights,int mainLightIndex,
            out LightData lightData
        )
        {
            lightData = new LightData();
            lightData.mainLightIndex = mainLightIndex;
            if(assets.AdditionalLightsRenderMode != LightRenderingMode.Disabled)
            {
                lightData.additionalLightCounts = Mathf.Min(mainLightIndex != -1 ? visibleLights.Length-1 : visibleLights.Length,
                                                            maxVisibleAdditionalLights);
                lightData.maxPerObjectAdditionalLightsCount = Mathf.Min(assets.MaxAdditionalLightsCount, maxPerObjectLights);
            }
            else
            {
                lightData.additionalLightCounts = 0;
                lightData.maxPerObjectAdditionalLightsCount = 0;
            }
            lightData.supportsAdditionalLights = assets.AdditionalLightsRenderMode != LightRenderingMode.Disabled;
            lightData.visibleLights = visibleLights;
            lightData.originalIndices = new NativeArray<int>(visibleLights.Length, Allocator.Temp);
            for (var i = 0; i < lightData.originalIndices.Length; i++)
            {
                lightData.originalIndices[i] = 90+i;
            }
        }
    }
}