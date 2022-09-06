using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using Unity.Collections;

namespace UnityEngine.CustomPipeline
{
    public class ForwardLight
    {
        static class LightConstantBuffer
        {
            public static int _MainLightPosition;
            public static int _MainLightColor;
            public static int _MainLightLayerMask;
            public static int _AdditionalLightsCount;
            public static int _AdditionalLightsPosition;
            public static int _AdditionalLightsColor;
            public static int _AdditionalLightsAttenuation;
            public static int _AdditionalLightsSpotDir;
            public static int _AdditionalLightsLayerMasks;
        }
        Vector4[] m_AdditionalLightsPositions;
        Vector4[] m_AdditionalLightsColors;
        Vector4[] m_AdditionalLightsAttenuations;
        Vector4[] m_AdditionalLightsSpotDirs;
        public ForwardLight()
        {
            LightConstantBuffer._MainLightColor = Shader.PropertyToID("_MainLightColor");
            LightConstantBuffer._MainLightPosition = Shader.PropertyToID("_MainLightPosition");
            LightConstantBuffer._MainLightLayerMask = Shader.PropertyToID("_MainLightLayerMask");
            LightConstantBuffer._AdditionalLightsColor = Shader.PropertyToID("_AdditionalLightsColor");
            LightConstantBuffer._AdditionalLightsCount = Shader.PropertyToID("_AdditionalLightsCount");
            LightConstantBuffer._AdditionalLightsAttenuation = Shader.PropertyToID("_AdditionalLightsAttenuation");
            LightConstantBuffer._AdditionalLightsSpotDir = Shader.PropertyToID("_AdditionalLightsSpotDir");
            LightConstantBuffer._AdditionalLightsLayerMasks = Shader.PropertyToID("_AdditionalLightsLayerMasks");
            
            int lightMaxIndex = CustomPipeline.maxVisibleAdditionalLights;
            m_AdditionalLightsPositions = new Vector4[lightMaxIndex];
            m_AdditionalLightsAttenuations = new Vector4[lightMaxIndex];
            m_AdditionalLightsColors = new Vector4[lightMaxIndex];
            m_AdditionalLightsSpotDirs = new Vector4[lightMaxIndex];
        }

        public void Setup(ScriptableRenderContext context,ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            SetupShaderLightConstant(cmd,ref renderingData);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        void SetupShaderLightConstant(CommandBuffer cmd,ref RenderingData renderingData)
        {
            SetupMainLightConstant(cmd,ref renderingData.lightData);
            SetupAdditionalLightsConstant(cmd,ref renderingData);
        }
        void SetupMainLightConstant(CommandBuffer cmd,ref LightData lightData)
        {
            Vector4 lightPos,lightColor,lightAttenuation,lightSpotDir;
            InitializeLightConstants(lightData.visibleLights,lightData.mainLightIndex,out lightPos
            ,out lightColor,out lightAttenuation,out lightSpotDir);
            cmd.SetGlobalVector(LightConstantBuffer._MainLightPosition,lightPos);
            cmd.SetGlobalVector(LightConstantBuffer._MainLightColor,lightColor);
        }
        void SetupAdditionalLightsConstant(CommandBuffer cmd,ref RenderingData renderingData)
        {
            ref LightData lightData = ref renderingData.lightData;
            var  cullingResults = renderingData.cullingResults;
            var lights = lightData.visibleLights;
            int maxAdditionalLightsCount = CustomPipeline.maxVisibleAdditionalLights;
            int additionalLightCount = SetupPerObjectLightIndices(cullingResults, ref lightData);
            if(additionalLightCount > 0)
            {
                for (int i = 0, lightIter = 0; i < lights.Length && lightIter < maxAdditionalLightsCount; i++)
                {
                    VisibleLight light = lights[i];
                    if(lightData.mainLightIndex != i)
                    {
                        InitializeLightConstants(lights, i, out m_AdditionalLightsPositions[lightIter],
                            out m_AdditionalLightsColors[lightIter],
                            out m_AdditionalLightsAttenuations[lightIter],
                            out m_AdditionalLightsSpotDirs[lightIter]
                        );
                        lightIter++;
                    }
                }
                cmd.SetGlobalVectorArray(LightConstantBuffer._AdditionalLightsAttenuation,m_AdditionalLightsAttenuations);
                cmd.SetGlobalVectorArray(LightConstantBuffer._AdditionalLightsColor,m_AdditionalLightsColors);
                cmd.SetGlobalVectorArray(LightConstantBuffer._AdditionalLightsPosition,m_AdditionalLightsPositions);
                cmd.SetGlobalVectorArray(LightConstantBuffer._AdditionalLightsSpotDir,m_AdditionalLightsSpotDirs);
                cmd.SetGlobalVector(LightConstantBuffer._AdditionalLightsCount,new Vector4(lightData.maxPerObjectAdditionalLightsCount,0.0f,0.0f,0.0f));
            }
            else
            {
                cmd.SetGlobalVector(LightConstantBuffer._AdditionalLightsCount,Vector4.zero);
            }
        }
        void InitializeLightConstants(NativeArray<VisibleLight> lights,int lightIndex,out Vector4 lightPos,
            out Vector4 lightColor,out Vector4 lightAttenuation,out Vector4 lightSpotDir)
        {
            CustomPipeline.InitializeLightConstants_common(lights,lightIndex,out lightPos,out lightColor,out lightAttenuation,out lightSpotDir);
            VisibleLight lightData = lights[lightIndex];
            Light light = lightData.light;
            // var additionalLightData = light.renderingLayerMask
        }
        int SetupPerObjectLightIndices(CullingResults cullResults, ref LightData lightData)
        {
            if(lightData.additionalLightCounts == 0)
                return lightData.additionalLightCounts;
            var visibleLights = lightData.visibleLights;
            var globalDirectionalLightsCount = 0;
            var additionalLightsCount = 0;
            var perObjectLightIndexMap = cullResults.GetLightIndexMap(Allocator.Temp);
            for (int i = 0; i < visibleLights.Length; ++i)
            {
                if (additionalLightsCount >= CustomPipeline.maxVisibleAdditionalLights)
                    break;

                VisibleLight light = visibleLights[i];
                if (i == lightData.mainLightIndex)
                {
                    perObjectLightIndexMap[i] = -1;
                    ++globalDirectionalLightsCount;
                }
                else
                {
                    perObjectLightIndexMap[i] -= globalDirectionalLightsCount;
                    ++additionalLightsCount;
                }
            }

            // Disable all remaining lights we cannot fit into the global light buffer.
            for (int i = globalDirectionalLightsCount + additionalLightsCount; i < perObjectLightIndexMap.Length; ++i)
                perObjectLightIndexMap[i] = -1;

            cullResults.SetLightIndexMap(perObjectLightIndexMap);
            perObjectLightIndexMap.Dispose();

            return additionalLightsCount;
        }
    }
}
