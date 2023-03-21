Shader "Hidden/FlatKit/CopyTexture"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZWrite Off Cull Off

        Pass
        {
            Name "Custom Copy Texture"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma multi_compile _ _USE_DRAW_PROCEDURAL

            float4 _SourceSize;

            TEXTURE2D_X(_EffectTexture);
            SAMPLER(sampler_EffectTexture);

            float4 SampleEffectTexture(float2 uv)
            {
                return SAMPLE_TEXTURE2D_X(_EffectTexture, sampler_EffectTexture, UnityStereoTransformScreenSpaceTex(uv));
            }

            struct Attributes
            {
                float4 positionHCS   : POSITION;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                #if _USE_DRAW_PROCEDURAL
                output.positionCS = float4(input.positionHCS.xyz, 1.0);
                #if UNITY_UV_STARTS_AT_TOP
                output.positionCS.y *= -1;
                #endif
                #else
                output.positionCS = TransformObjectToHClip(input.positionHCS.xyz);
                #endif

                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float4 c = SampleEffectTexture(input.uv);
                return c;
            }

            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
    }
}
