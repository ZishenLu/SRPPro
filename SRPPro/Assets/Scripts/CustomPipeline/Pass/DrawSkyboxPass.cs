using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEngine.CustomPipeline
{
    public class DrawSkyboxPass : CustomRenderPass
    {
        public override void Execute(ScriptableRenderContext context,RenderingData renderingData)
        {
            context.DrawSkybox(renderingData.cameraData.camera);
        }
    }
}
