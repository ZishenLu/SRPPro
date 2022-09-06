using System.Collections;
using UnityEngine.Rendering;
using UnityEngine;

namespace UnityEngine.CustomPipeline
{
    [CreateAssetMenu(menuName = "Rendering/CustomPipelineAssets")]
    public partial class CustomPipelineAssets : RenderPipelineAsset
    {
        [SerializeField] CustomRenderData m_CustomRenderData;
        CustomRender m_CustomRender;
        public CustomRender customRender
        {
            get
            {
                if(m_CustomRenderData != null && m_CustomRender == null)
                    m_CustomRender = m_CustomRenderData.Create();
                return m_CustomRender;
            }
        }
        protected override RenderPipeline CreatePipeline()
        {
            return new CustomPipeline(this);
        }
    }
}