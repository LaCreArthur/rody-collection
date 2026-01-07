Shader "Hidden/Underwater"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        ZWrite Off Cull Off

        Pass
        {
            Name "Underwater"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            float4 _TintColor;
            float _TintStrength;
            float _WaterLevel;
            float _MaxDepth;
            float _DepthDarkening;
            float _DistortionStrength;
            float _DistortionSpeed;
            float _BlockSize;
            float2 _Resolution;
            float4x4 _InverseViewProjection;

            float4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;

                // Pixelated block distortion for retro look
                // Quantize UV to blocks
                float2 blockUV = floor(uv * _Resolution / _BlockSize) * _BlockSize / _Resolution;

                // Wave distortion based on block position (not per-pixel)
                float wave1 = sin(blockUV.y * 15.0 + _Time.y * _DistortionSpeed);
                float wave2 = sin(blockUV.y * 25.0 - _Time.y * _DistortionSpeed * 0.7) * 0.5;
                float combinedWave = (wave1 + wave2) * _DistortionStrength;

                // Apply horizontal offset to entire blocks
                float2 distortedUV = uv;
                distortedUV.x += combinedWave;

                // Clamp to prevent sampling outside screen
                distortedUV = saturate(distortedUV);

                // Sample with point filtering for sharp pixels
                float4 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, distortedUV);

                // Apply blue tint (additive color shift, not multiply which darkens)
                float3 tinted = lerp(color.rgb, _TintColor.rgb, 0.5); // Blend towards tint color
                color.rgb = lerp(color.rgb, tinted, _TintStrength);

                // Slight darkening at edges (vignette)
                float2 vignetteUV = uv * 2.0 - 1.0;
                float vignette = 1.0 - dot(vignetteUV, vignetteUV) * 0.15;
                color.rgb *= vignette;

                // Depth-based darkening (objects deeper below water surface = darker)
                // Sample depth buffer and reconstruct world position
                float rawDepth = SampleSceneDepth(distortedUV);

                // Reconstruct world position from depth
                float2 positionNDC = distortedUV * 2.0 - 1.0;
                #if UNITY_UV_STARTS_AT_TOP
                    positionNDC.y = -positionNDC.y;
                #endif
                float4 positionCS = float4(positionNDC, rawDepth, 1.0);
                float4 positionWS = mul(_InverseViewProjection, positionCS);
                positionWS /= positionWS.w;

                // Calculate how far below water surface this pixel is
                float pixelDepthBelowWater = _WaterLevel - positionWS.y;
                pixelDepthBelowWater = max(0, pixelDepthBelowWater); // Only darken below water

                // Normalize by max depth and apply quadratic falloff
                float normalizedDepth = saturate(pixelDepthBelowWater / max(_MaxDepth, 0.01));
                float depthCurve = normalizedDepth * normalizedDepth;
                float darkness = 1.0 - (depthCurve * _DepthDarkening);
                color.rgb *= darkness;

                return color;
            }
            ENDHLSL
        }
    }
}
