Shader "DOOM/Water"
{
    Properties
    {
        [Header(Colors)]
        _ShallowColor ("Shallow Color", Color) = (0.4, 0.7, 0.8, 1)
        _DeepColor ("Deep Color", Color) = (0.1, 0.2, 0.5, 1)
        _FoamColor ("Foam Color", Color) = (1, 1, 1, 1)

        [Header(Waves)]
        _WaveSpeed ("Wave Speed", Float) = 0.5
        _WaveHeight ("Wave Height", Float) = 0.15
        _WaveFrequency ("Wave Frequency", Float) = 2.0

        [Header(Foam)]
        _FoamDistance ("Foam Distance", Float) = 1.5
        _FoamSteps ("Foam Steps (Quantization)", Range(2, 8)) = 4
        _FoamSpeed ("Foam Pulse Speed", Range(0.1, 2)) = 0.5

        [Header(Depth)]
        _DepthDistance ("Depth Fade Distance", Float) = 3.0
        _TintStrength ("Tint Strength", Range(0, 1)) = 0.7

        [Header(Surface)]
        _Smoothness ("Smoothness", Range(0, 1)) = 0.8
        _NormalStrength ("Normal Strength", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        // Alpha blending for transparency
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            Name "Water"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
                float fogFactor : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _ShallowColor;
                float4 _DeepColor;
                float4 _FoamColor;
                float _WaveSpeed;
                float _WaveHeight;
                float _WaveFrequency;
                float _FoamDistance;
                float _FoamSteps;
                float _FoamSpeed;
                float _DepthDistance;
                float _TintStrength;
                float _Smoothness;
                float _NormalStrength;
            CBUFFER_END

            // Gerstner wave function
            float3 GerstnerWave(float3 pos, float frequency, float speed, float height, float2 direction)
            {
                float phase = dot(pos.xz, direction) * frequency + _Time.y * speed;
                float3 offset;
                offset.y = sin(phase) * height;
                offset.xz = cos(phase) * direction * height * 0.5;
                return offset;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;

                float3 posOS = input.positionOS.xyz;

                // Use world-space position for seamless tiling
                float3 posWS = TransformObjectToWorld(posOS);

                // Apply two Gerstner waves based on world position
                float3 wave1 = GerstnerWave(posWS, _WaveFrequency, _WaveSpeed, _WaveHeight, float2(1, 0));
                float3 wave2 = GerstnerWave(posWS, _WaveFrequency * 0.7, _WaveSpeed * 1.3, _WaveHeight * 0.5, float2(0.7, 0.7));

                posOS += wave1 + wave2;

                // Calculate displaced normal (approximate)
                float3 tangent = float3(1, (wave1.y + wave2.y) * _NormalStrength, 0);
                float3 bitangent = float3(0, (wave1.y + wave2.y) * _NormalStrength, 1);
                float3 normal = normalize(cross(bitangent, tangent));
                normal = lerp(float3(0, 1, 0), normal, _NormalStrength);

                output.positionWS = TransformObjectToWorld(posOS);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(normal);
                output.screenPos = ComputeScreenPos(output.positionCS);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 screenUV = input.screenPos.xy / input.screenPos.w;

                // Sample scene depth
                float sceneDepth = LinearEyeDepth(SampleSceneDepth(screenUV), _ZBufferParams);
                float surfaceDepth = input.screenPos.w;
                float depthDiff = sceneDepth - surfaceDepth;

                // Depth factor
                float depthFactor = saturate(depthDiff / _DepthDistance);

                // Water color
                float3 waterColor = lerp(_ShallowColor.rgb, _DeepColor.rgb, depthFactor);

                // Shore foam with wave animation
                // Animate foam distance - waves crashing in and out
                float waveOffset = sin(_Time.y * _FoamSpeed) * 0.3 + 0.15;
                float animatedFoamDist = _FoamDistance * (1.0 + waveOffset);

                float shoreValue = saturate(depthDiff / animatedFoamDist);
                float invertedShore = 1.0 - shoreValue;
                float quantizedFoam = floor(invertedShore * _FoamSteps) / _FoamSteps;
                float foamMask = step(0.5, quantizedFoam);

                // Animated noise for foam breakup
                float noiseTime = floor(_Time.y * _FoamSpeed * 8.0) * 0.1; // Step-based for retro feel
                float noise = frac(sin(dot(input.positionWS.xz + noiseTime, float2(12.9898, 78.233))) * 43758.5453);
                foamMask *= step(noise, quantizedFoam + 0.3);

                // Apply foam
                float3 finalColor = lerp(waterColor, _FoamColor.rgb, foamMask * _FoamColor.a);

                // Alpha: shallow = transparent, deep = opaque (using material alpha values)
                float alpha = lerp(_ShallowColor.a, _DeepColor.a, depthFactor);

                // Foam is always opaque
                alpha = lerp(alpha, 1.0, foamMask * _FoamColor.a);

                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
