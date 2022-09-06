using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEngine.CustomPipeline
{
    public class ForwardRender : CustomRender
    {
        DrawObjectsPass m_RenderOpaqueForwardPass;
        DrawSkyboxPass m_DrawSkyboxPass;
        DrawObjectsPass m_RenderTransparentForwardPass;
        ForwardLight m_ForwardLight;
        public ForwardRender()
        {
            m_DrawSkyboxPass = new DrawSkyboxPass();
            m_RenderOpaqueForwardPass = new DrawObjectsPass(true);
            m_RenderTransparentForwardPass = new DrawObjectsPass(false);
            m_ForwardLight = new ForwardLight();
        }
        public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            CameraClearFlags clearFlags = renderingData.cameraData.camera.clearFlags; 
            
            cmd.ClearRenderTarget(
                clearFlags <= CameraClearFlags.Depth,
                clearFlags == CameraClearFlags.Color,
                Color.clear
            );
            EnQueue(m_RenderOpaqueForwardPass);
            EnQueue(m_DrawSkyboxPass);
            EnQueue(m_RenderTransparentForwardPass);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
        public override void SetupLights(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            base.SetupLights(context,ref renderingData);
            m_ForwardLight.Setup(context,ref renderingData);
        }
    }
}
