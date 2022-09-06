using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace UnityEngine.CustomPipeline
{
    public abstract class CustomRenderPass
    {
        List<CustomRenderPass> m_ActiveRenderPassQueue = new List<CustomRenderPass>();
        public void EnqueuePass(CustomRenderPass pass)
        {
            m_ActiveRenderPassQueue.Add(pass);
        }
        public void Clear()
        {
            m_ActiveRenderPassQueue.Clear();
        }
        public abstract void Execute(ScriptableRenderContext context,RenderingData renderingData);
        public DrawingSettings CreateDrawingSettings(
            List<ShaderTagId> shaderTags,
            ref RenderingData renderingData,
            SortingSettings sortingSettings
        )
        {
            DrawingSettings ds = new DrawingSettings()
            {
                sortingSettings = sortingSettings,
                enableDynamicBatching = renderingData.useDynamicBatcher,
                enableInstancing = ((renderingData.cameraData.camera.cameraType != CameraType.Preview) && renderingData.useGPUInstance) ? true : false,
            };
            for (int i = 0; i < shaderTags.Count; i++)
            {
                ds.SetShaderPassName(i,shaderTags[i]);
            }
            return ds;
        }
    }
}