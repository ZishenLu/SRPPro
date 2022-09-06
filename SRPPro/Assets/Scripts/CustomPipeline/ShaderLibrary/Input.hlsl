#ifndef CUSTOM_INPUT_INCLUDE
#define CUSTOM_INPUT_INCLUDE
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "UnityInput.hlsl"

    #if defined(SHADER_API_MOBILE) && (defined(SHADER_API_GLES) || defined(SHADER_API_GLES30))
        #define MAX_VISIBLE_LIGHTS 16
    #elif defined(SHADER_API_MOBILE) || (defined(SHADER_API_GLCORE) && !defined(SHADER_API_SWITCH)) || defined(SHADER_API_GLES) || defined(SHADER_API_GLES3) // Workaround because SHADER_API_GLCORE is also defined when SHADER_API_SWITCH is
        #define MAX_VISIBLE_LIGHTS 32
    #else
        #define MAX_VISIBLE_LIGHTS 256
    #endif

    float4 _MainLightPosition;
    half4 _MainLightColor;

    #ifndef SHADER_API_GLES3
    CBUFFER_START(AdditionalLights)
    #endif
    float4 _AdditionalLightsPosition[MAX_VISIBLE_LIGHTS];
    half4 _AdditionalLightsColor[MAX_VISIBLE_LIGHTS];
    half4 _AdditionalLightsAttenuation[MAX_VISIBLE_LIGHTS];
    half4 _AdditionalLightsSpotDir[MAX_VISIBLE_LIGHTS];
    #ifndef SHADER_API_GLES3 
    CBUFFER_END
    #endif

    #define UNITY_MATRIX_M unity_ObjectToWorld
    #define UNITY_MATRIX_I_M unity_WorldToObject
    #define UNITY_MATRIX_V unity_MatrixV
    #define UNITY_MATRIX_VP unity_MatrixVP
    #define UNITY_MATRIX_P glstate_matrix_projection
    #define UNITY_PREV_MATRIX_I_M unity_MatrixPreviousMI
    #define UNITY_PREV_MATRIX_M unity_MatrixPreviousM       //上一帧的m矩阵

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
#endif