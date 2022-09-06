using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using Unity.Collections;

namespace UnityEngine.CustomPipeline
{
    public struct RenderingData
    {
        public CullingResults cullingResults;
        public CameraData cameraData;
        public LightData lightData;
        public bool useDynamicBatcher;
        public bool useGPUInstance;
        
    }
    public struct CameraData
    {
        public Camera camera;
    }
    public struct LightData
    {
        public int mainLightIndex;
        public int additionalLightCounts;
        public int maxPerObjectAdditionalLightsCount;
        public NativeArray<VisibleLight> visibleLights;
        public NativeArray<int> originalIndices;
        // public bool supportsLightLayers;
        public bool supportsAdditionalLights;


    }
    public sealed partial class CustomPipeline
    {
        static Vector4 k_DefaultLightPosition = new Vector4(0.0f, 0.0f, 1.0f, 0.0f);
        static Vector4 k_DefaultLightColor = Color.black;

        static Vector4 k_DefaultLightAttenuation = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
        static Vector4 k_DefaultLightSpotDirection = new Vector4(0.0f, 0.0f, 1.0f, 0.0f);
        public static void GetLightAttenuationAndSpotDirection(
            LightType lightType,float lightRange,Matrix4x4 lightLocalToWorldMatrix,
            float spotAngle,float? innerSpotAngle,
            out Vector4 lightAttenuation,out Vector4 lightSpotDir)
        {
            lightAttenuation = k_DefaultLightAttenuation;
            lightSpotDir = k_DefaultLightSpotDirection;
            if(lightType != LightType.Directional)
            {
                float lightRangeSqr = lightRange * lightRange;
                float fadeRangeSqr = -0.36f * lightRangeSqr;
                float oneOverFadeRangeSqr = 1.0f / fadeRangeSqr;
                float lightRangeSqrOverFadeRangeSqr = -lightRangeSqr / fadeRangeSqr;
                float oneOverLightRangeSqr = 1.0f / Mathf.Max(0.0001f, lightRangeSqr);

                lightAttenuation.x = (Application.isMobilePlatform || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Switch) 
                                    ? oneOverFadeRangeSqr : oneOverLightRangeSqr;
                lightAttenuation.y = lightRangeSqrOverFadeRangeSqr; 
            }
            if(lightType == LightType.Spot)
            {
                Vector4 dir = lightLocalToWorldMatrix.GetColumn(2);
                lightSpotDir = new Vector4(-dir.x, -dir.y, -dir.z, 0.0f);

                float cosOuterAngle = Mathf.Cos(Mathf.Deg2Rad * spotAngle * 0.5f);
                float cosInnerAngle;
                if(innerSpotAngle.HasValue)
                    cosInnerAngle = Mathf.Cos(innerSpotAngle.Value * Mathf.Deg2Rad * 0.5f);
                else
                    cosInnerAngle = Mathf.Cos((2.0f * Mathf.Atan(Mathf.Tan(spotAngle * 0.5f * Mathf.Deg2Rad) * (64.0f - 18.0f) / 64.0f)) * 0.5f);
                float smoothAngleRange = Mathf.Max(0.001f, cosInnerAngle - cosOuterAngle);
                float invAngleRange = 1.0f / smoothAngleRange;
                float add = -cosOuterAngle * invAngleRange;
                lightAttenuation.z = invAngleRange;
                lightAttenuation.w = add;
            }
        }
        public static void InitializeLightConstants_common(NativeArray<VisibleLight> visibleLights,int lightIndex,
            out Vector4 lightPos,out Vector4 lightColor,out Vector4 lightAttenuation,out Vector4 lightSpotDir)
        {
            lightPos = k_DefaultLightPosition;
            lightColor = k_DefaultLightColor;
            lightAttenuation = k_DefaultLightAttenuation;
            lightSpotDir = k_DefaultLightSpotDirection;

            if(lightIndex < 0)return;

            VisibleLight lightData = visibleLights[lightIndex];
            if(lightData.lightType == LightType.Directional)
            {
                Vector3 dir = -lightData.localToWorldMatrix.GetColumn(2);
                lightPos = new Vector4(dir.x,dir.y,dir.z,0.0f);
            }
            else
            {
                Vector3 pos = lightData.localToWorldMatrix.GetColumn(3);
                lightPos = new Vector4(pos.x,pos.y,pos.z,1.0f);
            }
            lightColor = lightData.finalColor;
            GetLightAttenuationAndSpotDirection(lightData.lightType,lightData.range,lightData.localToWorldMatrix,lightData.spotAngle,
                lightData.light?.innerSpotAngle,out lightAttenuation,out lightSpotDir);
        }
    }
}