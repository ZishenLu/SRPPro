using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.CustomPipeline
{
    [CreateAssetMenu(menuName ="Rendering/ForwardRenderData")]
    public class ForwardRenderData : CustomRenderData
    {
        public ForwardRenderData()
        {

        }
        public override CustomRender Create()
        {
            return new ForwardRender();
        }
    }

}