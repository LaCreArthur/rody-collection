Shader "Hidden/Pixelation"
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
            Name "Pixelation"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float2 _BlockCount;
            float2 _BlockSize;

            float4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;

                // Calculate which block this pixel belongs to
                float2 blockPos = floor(uv * _BlockCount);

                // Sample from the center of the block (not the corner)
                // This gives sharper, more consistent pixelation
                float2 blockCenter = blockPos * _BlockSize + _BlockSize * 0.5;

                // Sample the source texture at block center with point filtering
                float4 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, blockCenter);

                return color;
            }
            ENDHLSL
        }
    }
}
