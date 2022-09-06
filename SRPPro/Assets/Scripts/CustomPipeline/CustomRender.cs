using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEngine.CustomPipeline
{
    public abstract class CustomRender
    {
        List<CustomRenderPass> m_ActiveRenderPass = new List<CustomRenderPass>();
        public abstract void Setup(ScriptableRenderContext context,ref RenderingData renderingData);
        public void EnQueue(CustomRenderPass pass)
        {
            m_ActiveRenderPass.Add(pass);
        }
        public void Clear()
        {
            m_ActiveRenderPass.Clear();
        }   
        public void Execute(ScriptableRenderContext context,RenderingData renderingData)
        {
            SetupLights(context,ref renderingData);
            foreach (var item in m_ActiveRenderPass)
            {
                item.Execute(context,renderingData);
            }
        }
        public virtual void SetupLights(ScriptableRenderContext context,ref RenderingData renderingData)
        {

        }
    }
}