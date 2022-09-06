using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEngine.CustomPipeline
{
    public abstract class CustomRenderFeature : ScriptableObject
    {
        public bool m_Active;
        public abstract void Create();
        public abstract void AddRenderPasses(CustomRender render,ref RenderingData renderingData);
        void OnEnable()
        {
            Create();
        }
    }
}