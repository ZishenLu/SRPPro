#ifndef CUSTOM_REALTIMELIGHTS_INCLUDE
#define CUSTOM_REALTIMELIGHTS_INCLUDE

#include "Input.hlsl"
#include "Core.hlsl"

struct Light    
{
    half3 direction;
    half3 color;
    half distanceAttenuation;
};

Light GetMainLight()
{
    Light light;
    light.direction = half3(_MainLightPosition.xyz);
    light.distanceAttenuation = 1.0;
    light.color = _MainLightColor.rgb;
    return light;
}

Light GetAdditionalPerObjectLight(int perObjectLightIndex, float4 positionWS)
{
    float4 lightPositionWS = _AdditionalLightsPosition[perObjectLightIndex];
    half3 color = _AdditionalLightsColor[perObjectLightIndex].rgb;
    half4 distanceAndSpotAttenuation = _AdditionalLightsAttenuation[perObjectLightIndex];
    half4 spotDirection = _AdditionalLightsSpotDir[perObjectLightIndex];

    float3 lightVector = lightPositionWS.xyz - positionWS * lightPositionWS.w;
    float distanceSqr = max(dot(lightVector, lightVector), HALF_MIN);

    half3 lightDirection = half3(lightVector * rsqrt(distanceSqr));
    half attenuation = half(DistanceAttenuation(distanceSqr, distanceAndSpotAttenuation.xy) * AngleAttenuation(spotDirection.xyz, lightDirection, distanceAndSpotAttenuation.zw));

    Light light;
    light.direction = lightDirection;
    light.distanceAttenuation = attenuation;
    light.color = color;
    return light;
}

#endif