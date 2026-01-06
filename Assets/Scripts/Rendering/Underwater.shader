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

            float4 _TintColor;
            float _TintStrength;
            float _DistortionStrength;
            float _DistortionSpeed;
            float _BlockSize;
            float2 _Resolution;

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

                // Apply blue tint (overlay blend)
                float3 tinted = color.rgb * _TintColor.rgb;

                // Lerp based on strength
                color.rgb = lerp(color.rgb, tinted, _TintStrength);

                // Slight darkening at edges (vignette)
                float2 vignetteUV = uv * 2.0 - 1.0;
                float vignette = 1.0 - dot(vignetteUV, vignetteUV) * 0.15;
                color.rgb *= vignette;

                // Add subtle caustic-like brightness variation
                float caustic = sin(uv.x * 30.0 + _Time.y * 2.0) * sin(uv.y * 30.0 + _Time.y * 1.5);
                caustic = caustic * 0.5 + 0.5; // Remap to 0-1
                color.rgb += caustic * _TintColor.rgb * 0.05 * _TintStrength;

                return color;
            }
            ENDHLSL
        }
    }
}
