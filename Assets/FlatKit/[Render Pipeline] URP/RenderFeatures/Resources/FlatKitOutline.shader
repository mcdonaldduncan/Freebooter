Shader "Hidden/FlatKit/OutlineFilter"
{
    Properties
    {
        [HideInInspector]_BaseMap ("Base (RGB)", 2D) = "white" {}

        _EdgeColor ("Outline Color", Color) = (1, 1, 1, 1)
        _Thickness ("Thickness", Range(0, 5)) = 1

        [Space(15)]
        [Toggle(OUTLINE_USE_DEPTH)]_UseDepth ("Use Depth", Float) = 1
        _DepthThresholdMin ("Min Depth Threshold", Range(0, 1)) = 0
        _DepthThresholdMax ("Max Depth Threshold", Range(0, 1)) = 0.25

        [Space(15)]
        [Toggle(OUTLINE_USE_NORMALS)]_UseNormals ("Use Normals", Float) = 0
        _NormalThresholdMin ("Min Normal Threshold", Range(0, 1)) = 0.5
        _NormalThresholdMax ("Max Normal Threshold", Range(0, 1)) = 1.0

        [Space(15)]
        [Toggle(OUTLINE_USE_COLOR)]_UseColor ("Use Color", Float) = 0
        _ColorThresholdMin ("Min Color Threshold", Range(0, 1)) = 0
        _ColorThresholdMax ("Max Color Threshold", Range(0, 1)) = 0.25

        [Space(15)]
        [Toggle(OUTLINE_ONLY)]_OutlineOnly ("Outline Only", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZWrite Off Cull Off

        Pass
        {
            Name "Outline"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            #pragma shader_feature OUTLINE_USE_DEPTH
            #pragma shader_feature OUTLINE_USE_NORMALS
            #pragma shader_feature OUTLINE_USE_COLOR
            #pragma shader_feature OUTLINE_ONLY
            #pragma shader_feature RESOLUTION_INVARIANT_THICKNESS

            #pragma multi_compile _ _USE_DRAW_PROCEDURAL

            uniform half _Thickness;
            uniform half4 _EdgeColor;
            uniform half _DepthThresholdMin, _DepthThresholdMax;
            uniform half _NormalThresholdMin, _NormalThresholdMax;
            uniform half _ColorThresholdMin, _ColorThresholdMax;

            TEXTURE2D_X(_CameraColorTexture);
            SAMPLER(sampler_CameraColorTexture);

            // #define OUTLINE_USE_TRANSPARENT_DEPTH
            #ifdef OUTLINE_USE_TRANSPARENT_DEPTH
            TEXTURE2D_X(_CameraTransparentDepthTexture);
            #endif

            float4 _SourceSize;

            // Z buffer depth to linear 0-1 depth
            // Handles orthographic projection correctly
            float Linear01Depth(float z)
            {
                const float isOrtho = unity_OrthoParams.w;
                const float isPers = 1.0 - unity_OrthoParams.w;
                z *= _ZBufferParams.x;
                return (1.0 - isOrtho * z) / (isPers * z + _ZBufferParams.y);
            }

            float SampleDepth(float2 uv)
            {
                float d = SampleSceneDepth(uv);
                #ifdef OUTLINE_USE_TRANSPARENT_DEPTH
                d += SAMPLE_TEXTURE2D_X(_CameraTransparentDepthTexture, sampler_CameraColorTexture, UnityStereoTransformScreenSpaceTex(uv)).r;
                #endif
                return Linear01Depth(d);
            }

            float4 SampleCameraColor(float2 uv)
            {
                return SAMPLE_TEXTURE2D_X(_CameraColorTexture, sampler_CameraColorTexture, UnityStereoTransformScreenSpaceTex(uv));
            }

            float4 Outline(float2 uv)
            {
                float4 original = SampleCameraColor(uv);

                const float offset_positive = +ceil(_Thickness * 0.5f);
                const float offset_negative = -floor(_Thickness * 0.5f);

                #if RESOLUTION_INVARIANT_THICKNESS
                const float screen_ratio = _SourceSize.x / _SourceSize.y;
                const float2 texel_size = 1.0 / 800.0 * float2(1.0, screen_ratio);
                #else
                const float2 texel_size = _SourceSize.zw;
                #endif

                float left = texel_size.x * offset_negative;
                float right = texel_size.x * offset_positive;
                float top = texel_size.y * offset_negative;
                float bottom = texel_size.y * offset_positive;

                const float2 uv0 = uv + float2(left, top);
                const float2 uv1 = uv + float2(right, bottom);
                const float2 uv2 = uv + float2(right, top);
                const float2 uv3 = uv + float2(left, bottom);

                #ifdef OUTLINE_USE_DEPTH
                const float d0 = SampleDepth(uv0);
                const float d1 = SampleDepth(uv1);
                const float d2 = SampleDepth(uv2);
                const float d3 = SampleDepth(uv3);

                const float depth_threshold_scale = 300.0f;
                float d = length(float2(d1 - d0, d3 - d2)) * depth_threshold_scale;
                d = smoothstep(_DepthThresholdMin, _DepthThresholdMax, d);
                #else
                float d = 0.0f;
                #endif  // OUTLINE_USE_DEPTH

                #ifdef OUTLINE_USE_NORMALS
                const float3 n0 = SampleSceneNormals(uv0);
                const float3 n1 = SampleSceneNormals(uv1);
                const float3 n2 = SampleSceneNormals(uv2);
                const float3 n3 = SampleSceneNormals(uv3);

                const float3 nd1 = n1 - n0;
                const float3 nd2 = n3 - n2;
                float n = sqrt(dot(nd1, nd1) + dot(nd2, nd2));
                n = smoothstep(_NormalThresholdMin, _NormalThresholdMax, n);
                #else
                float n = 0.0f;
                #endif  // OUTLINE_USE_NORMALS

                #ifdef OUTLINE_USE_COLOR
                const float3 c0 = SampleCameraColor(uv0).rgb;
                const float3 c1 = SampleCameraColor(uv1).rgb;
                const float3 c2 = SampleCameraColor(uv2).rgb;
                const float3 c3 = SampleCameraColor(uv3).rgb;

                const float3 cd1 = c1 - c0;
                const float3 cd2 = c3 - c2;
                float c = sqrt(dot(cd1, cd1) + dot(cd2, cd2));
                c = smoothstep(_ColorThresholdMin, _ColorThresholdMax, c);
                #else
                float c = 0;
                #endif  // OUTLINE_USE_COLOR

                const float g = max(d, max(n, c));

                #ifdef OUTLINE_ONLY
                original.rgb = lerp(1.0 - _EdgeColor.rgb, _EdgeColor.rgb, g * _EdgeColor.a);
                #endif  // OUTLINE_ONLY

                float4 output;
                output.rgb = lerp(original.rgb, _EdgeColor.rgb, g * _EdgeColor.a);
                output.a = original.a;
                return output;
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

                float4 c = Outline(input.uv);
                return c;
            }

            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}
