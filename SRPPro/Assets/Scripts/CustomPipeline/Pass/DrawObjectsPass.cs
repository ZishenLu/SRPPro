using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace UnityEngine.CustomPipeline
{
    public class DrawObjectsPass : CustomRenderPass
    {
        bool m_isOpaque;
        List<ShaderTagId> shaderTagIdLists = new List<ShaderTagId>()
        {
            new ShaderTagId("CustomPipeline"),
            new ShaderTagId("SRPDefaultUnlit"),
        };

        public DrawObjectsPass(bool isOpaque)
        {
            this.m_isOpaque = isOpaque;
        }
        public override void Execute(ScriptableRenderContext context,RenderingData renderingData)
        {
            
            CommandBuffer cmd = CommandBufferPool.Get();
            
            SortingSettings sortingSettings = new SortingSettings()
            {
                criteria = m_isOpaque ? SortingCriteria.CommonOpaque : SortingCriteria.CommonTransparent
            };
            DrawingSettings ds = CreateDrawingSettings(shaderTagIdLists,ref renderingData,sortingSettings);
            
            FilteringSettings fs = new FilteringSettings(m_isOpaque ? RenderQueueRange.opaque : RenderQueueRange.transparent);
            context.ExecuteCommandBuffer(cmd);
            context.DrawRenderers(renderingData.cullingResults,ref ds,ref fs);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
}