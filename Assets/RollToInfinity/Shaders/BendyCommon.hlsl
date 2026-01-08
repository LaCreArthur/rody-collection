#ifndef BENDY_COMMON_INCLUDED
#define BENDY_COMMON_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

// Global properties set by BendControllerRadial script
half3 _CurveOrigin;
half3 _ReferenceDirection;
half _Curvature;
half3 _Scale;
half _FlatMargin;
half _HorizonWaveFrequency;
half _curveMultiplier;

float4 BendVertex(float4 positionOS)
{
    float3 wpos = TransformObjectToWorld(positionOS.xyz);

    half2 xzDist = (wpos.xz - _CurveOrigin.xz) / _Scale.xz;
    half dist = length(xzDist);
    half waveMultiplier = 1;

    #if defined(HORIZON_WAVES)
    half2 direction = lerp(_ReferenceDirection.xz, xzDist, min(dist, 1));
    half theta = acos(clamp(dot(normalize(direction), _ReferenceDirection.xz), -1, 1));
    waveMultiplier = cos(theta * _HorizonWaveFrequency);
    #endif

    dist = max(0, dist - _FlatMargin);

    wpos.y -= dist * dist * _Curvature * waveMultiplier;

    #if defined(CURVES)
    half curveMultiplier = _curveMultiplier;
    wpos.x -= dist * dist * (_Curvature / 2) * curveMultiplier;
    #endif

    return float4(TransformWorldToObject(wpos), 1.0);
}

float4 ApplyBend(float4 positionOS)
{
    #if defined(BEND_ON)
    return BendVertex(positionOS);
    #else
    return positionOS;
    #endif
}

#endif
