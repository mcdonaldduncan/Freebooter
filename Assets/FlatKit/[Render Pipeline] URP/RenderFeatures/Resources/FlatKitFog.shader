Shader "Hidden/FlatKit/FogFilter"
{
    Properties
    {
        [Toggle(USE_DISTANCE_FOG)]_UseDistanceFog ("Use Distance", Float) = 0
        [Toggle(USE_DISTANCE_FOG_ON_SKY)]_UseDistanceFogOnSky ("Use Distance Fog On Sky", Float) = 0

        [Space]
        _Near ("Near", Float) = 0
        _Far ("Far", Float) = 100

        [Space]
        _DistanceFogIntensity ("Distance Fog Intensity", Range(0, 1)) = 1

        [Space(25)]
        [Toggle(USE_HEGHT_FOG)]_UseHeightFog ("Use Height", Float) = 0
        [Toggle(USE_HEGHT_FOG_ON_SKY)]_UseHeightFogOnSky ("Use Height Fog On Sky", Float) = 0

        [Space]
        _LowWorldY ("Low", Float) = 0
        _HighWorldY ("High", Float) = 10

        [Space]
        _HeightFogIntensity ("Height Fog Intensity", Range(0, 1)) = 1

        [Space(25)]
        _DistanceHeightBlend ("Distance / Height blend", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100
        ZWrite Off Cull Off

        Pass
        {
            Name "Fog"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            #pragma multi_compile _ _USE_DRAW_PROCEDURAL

            sampler2D _DistanceLUT;
            float _Near;
            float _Far;
            half _UseDistanceFog;
            half _UseDistanceFogOnSky;

            sampler2D _HeightLUT;
            float _LowWorldY;
            float _HighWorldY;
            half _UseHeightFog;
            half _UseHeightFogOnSky;

            float _DistanceFogIntensity;
            float _HeightFogIntensity;
            float _DistanceHeightBlend;

            #define ALMOST_ONE 0.999

            // Using `_CameraColorTexture` instead of the opaque texture `SampleSceneColor` to handle transparency.
            TEXTURE2D_X(_CameraColorTexture);
            SAMPLER(sampler_CameraColorTexture);
            float4 _CameraColorTexture_TexelSize;

            // Z buffer depth to linear 0-1 depth
            // Handles orthographic projection correctly
            float Linear01Depth(float z)
            {
                float isOrtho = unity_OrthoParams.w;
                float isPers = 1.0 - unity_OrthoParams.w;
                z *= _ZBufferParams.x;
                return (1.0 - isOrtho * z) / (isPers * z + _ZBufferParams.y);
            }

            float4 SampleCameraColor(float2 uv)
            {
                return SAMPLE_TEXTURE2D_X(_CameraColorTexture, sampler_CameraColorTexture, UnityStereoTransformScreenSpaceTex(uv));
            }

            float LinearEyeDepth(float z)
            {
                return rcp(_ZBufferParams.z * z + _ZBufferParams.w);
            }

            float4 Fog(float2 uv, float3 screen_pos)
            {
                float4 original = SampleCameraColor(uv);

                const float depthPacked = SampleSceneDepth(uv);
                const float depthEye = LinearEyeDepth(depthPacked);
                const float depthCameraPlanes = Linear01Depth(depthPacked);
                const float depthAbsolute = _ProjectionParams.y + (_ProjectionParams.z - _ProjectionParams.y) *
                    depthCameraPlanes;
                const float depthFogPlanes = saturate((depthAbsolute - _Near) / (_Far - _Near));
                const float isSky = step(ALMOST_ONE, depthCameraPlanes);

                float4 distanceFog = tex2D(_DistanceLUT, float2(depthFogPlanes, 0.5));
                distanceFog.a *= step(isSky, _UseDistanceFogOnSky);
                distanceFog.a *= _UseDistanceFog * _DistanceFogIntensity;

                const float3 worldPos = screen_pos * depthEye + _WorldSpaceCameraPos;
                const float heightUV = saturate((worldPos.y - _LowWorldY) / (_HighWorldY - _LowWorldY));
                float4 heightFog = tex2D(_HeightLUT, float2(heightUV, 0.5));
                heightFog.a *= step(isSky, _UseHeightFogOnSky);
                heightFog.a *= _UseHeightFog * _HeightFogIntensity;

                float fogBlend = _DistanceHeightBlend;
                if (!_UseDistanceFog) fogBlend = 1.0;
                if (!_UseHeightFog) fogBlend = 0.0;
                const float4 fog = lerp(distanceFog, heightFog, fogBlend);

                float4 final = lerp(original, fog, fog.a);
                final.a = original.a;
                return final;
            }

            struct Attributes
            {
                float4 positionOS: POSITION;
                float2 uv: TEXCOORD0;
                
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv: TEXCOORD0;
                float3 screen_pos: TEXCOORD1;
                float4 vertex: SV_POSITION;
                
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                #if _USE_DRAW_PROCEDURAL
                output.vertex = float4(input.positionOS.xyz, 1.0);
                #if UNITY_UV_STARTS_AT_TOP
                output.vertex.y *= -1;
                #endif
                #else
                const VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.vertex = vertexInput.positionCS;
                #endif

                output.uv = input.uv;
                output.screen_pos = ComputeScreenPos(output.vertex).xyz;

                return output;
            }

            half4 frag(Varyings input): SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float4 c = Fog(input.uv, input.screen_pos);
                return c;
            }

            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL

        }
    }
    FallBack "Diffuse"
}