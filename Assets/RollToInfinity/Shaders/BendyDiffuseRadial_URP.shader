/// <summary>
/// URP version of Bendy diffuse - Radial shader (Opaque)
/// </summary>

Shader "Custom/Bendy diffuse - Radial (URP)"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #pragma multi_compile __ HORIZON_WAVES
            #pragma multi_compile __ BEND_ON
            #pragma multi_compile __ CURVES

            #include "BendyCommon.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float fogFactor : TEXCOORD3;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float4 positionOS = ApplyBend(IN.positionOS);

                OUT.positionWS = TransformObjectToWorld(positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(OUT.positionWS);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.fogFactor = ComputeFogFactor(OUT.positionHCS.z);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * _Color;

                Light mainLight = GetMainLight();
                float3 normalWS = normalize(IN.normalWS);
                half NdotL = saturate(dot(normalWS, mainLight.direction));
                half3 diffuse = texColor.rgb * mainLight.color * NdotL;
                half3 ambient = texColor.rgb * unity_AmbientSky.rgb;
                half3 finalColor = MixFog(diffuse + ambient, IN.fogFactor);

                return half4(finalColor, texColor.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag

            #pragma multi_compile __ BEND_ON
            #pragma multi_compile __ HORIZON_WAVES
            #pragma multi_compile __ CURVES

            #include "BendyCommon.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            Varyings ShadowVert(Attributes IN)
            {
                Varyings OUT;

                float4 positionOS = ApplyBend(IN.positionOS);
                float3 positionWS = TransformObjectToWorld(positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);

                OUT.positionHCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
                return OUT;
            }

            half4 ShadowFrag(Varyings IN) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma vertex DepthVert
            #pragma fragment DepthFrag

            #pragma multi_compile __ BEND_ON
            #pragma multi_compile __ HORIZON_WAVES
            #pragma multi_compile __ CURVES

            #include "BendyCommon.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            Varyings DepthVert(Attributes IN)
            {
                Varyings OUT;
                float4 positionOS = ApplyBend(IN.positionOS);
                OUT.positionHCS = TransformObjectToHClip(positionOS.xyz);
                return OUT;
            }

            half4 DepthFrag(Varyings IN) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }

    Fallback "Universal Render Pipeline/Lit"
}
