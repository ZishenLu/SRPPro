using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.CustomPipeline
{
    public abstract class CustomRenderData : ScriptableObject
    {
        public abstract CustomRender Create();
    }
}