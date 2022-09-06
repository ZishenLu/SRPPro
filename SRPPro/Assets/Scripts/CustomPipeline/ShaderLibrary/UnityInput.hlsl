#ifndef CUSTOM_UNITYINPUT_INCLUDE
#define CUSTOM_UNITYINPUT_INCLUDE

CBUFFER_START(UnityPerDraw)
    float4x4 unity_ObjectToWorld;
    float4x4 unity_WorldToObject;
    float4 unity_LODFade;
    real4 unity_WorldTransformParams;
    float4x4 unity_MatrixPreviousM;
    float4x4 unity_MatrixPreviousMI;
CBUFFER_END

float4x4 unity_MatrixV;
float4x4 unity_MatrixVP;
float4x4 glstate_matrix_projection;

#endif