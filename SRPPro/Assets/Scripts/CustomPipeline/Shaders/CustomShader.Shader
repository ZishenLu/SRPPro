Shader "CustomShader/CustomShader"
{
    Properties
    {
        _BaseMap("MainTex", 2D) = "White" { }
        _BaseColor("BaseColor", Color) = (1.0, 1.0, 1.0, 1.0)
    }

    SubShader
    {
        // URP的shader要在Tags中注明渲染管线是UniversalPipeline
        Tags
        {
            "RanderPipline" = "UniversalPipeline"
            "RanderType" = "Opaque"
        }

        HLSLINCLUDE

            // 引入Core.hlsl头文件，替换UnityCG中的cginc
            #include "Assets/Scripts/CustomPipeline/ShaderLibrary/Input.hlsl"

            // 将不占空间的材质相关变量放在CBUFFER中，为了兼容SRP Batcher
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
            CBUFFER_END

            // 材质单独声明，使用DX11风格的新采样方法
            TEXTURE2D (_BaseMap);
            SAMPLER(sampler_BaseMap);

            struct a2v
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };

        ENDHLSL

        Pass
        {
            // 声明Pass名称，方便调用与识别
            Name "CustomPipeline"
            Tags {"LightMode" = "CustomPipeline"}

            HLSLPROGRAM

                // 声明顶点/片段着色器对应的函数
                #pragma vertex vert
                #pragma fragment frag

                // 顶点着色器
                v2f vert(a2v i)
                {
                    v2f o;
                    o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
                    o.uv = TRANSFORM_TEX(i.uv, _BaseMap);
                    return o;
                }

                // 片段着色器
                half4 frag(v2f i) : SV_Target
                {
                    half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _MainLightColor;
                    return  _MainLightColor * _BaseColor;
                }
            
            ENDHLSL
        }
    }
}