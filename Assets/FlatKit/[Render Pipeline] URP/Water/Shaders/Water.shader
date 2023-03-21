Shader "FlatKit/Water"
{
    Properties
    {
        [Header(Colors)][Space]
        [KeywordEnum(Linear, Gradient Texture)] _ColorMode ("     Source{Colors}", Float) = 0.0
        _ColorShallow ("[_COLORMODE_LINEAR]     Shallow", Color) = (0.35, 0.6, 0.75, 0.8) // Color alpha controls opacity
        _ColorDeep ("[_COLORMODE_LINEAR]     Deep", Color) = (0.65, 0.9, 1.0, 1.0)
        [NoScaleOffset] _ColorGradient("[_COLORMODE_GRADIENT_TEXTURE]     Gradient", 2D) = "white" {}
        _FadeDistance("     Shallow depth", Float) = 0.5
        _WaterDepth("     Gradient size", Float) = 5.0
        _LightContribution("     Light Color Contribution", Range(0, 1)) = 0

        [Space]
        _WaterClearness("     Transparency", Range(0, 1)) = 0.3
        _ShadowStrength("     Shadow strength", Range(0, 1)) = 0.35

        [Header(Crest)][Space]
        _CrestColor("     Color{Crest}", Color) = (1.0, 1.0, 1.0, 0.9)
        _CrestSize("     Size{Crest}", Range(0, 1)) = 0.1
        _CrestSharpness("     Sharp transition{Crest}", Range(0, 1)) = 0.1

        [Space][Header(Wave geometry)][Space]
        [KeywordEnum(None, Round, Grid, Pointy)] _WaveMode ("     Shape{Wave}", Float) = 1.0
        _WaveSpeed("[!_WAVEMODE_NONE]     Speed{Wave}", Float) = 0.5
        _WaveAmplitude("[!_WAVEMODE_NONE]     Amplitude{Wave}", Float) = 0.25
        _WaveFrequency("[!_WAVEMODE_NONE]     Frequency{Wave}", Float) = 1.0
        _WaveDirection("[!_WAVEMODE_NONE]     Direction{Wave}", Range(-1.0, 1.0)) = 0
        _WaveNoise("[!_WAVEMODE_NONE]     Noise{Wave}", Range(0, 1)) = 0.25

        [Space][Header(Foam)][Space]
        [KeywordEnum(None, Gradient Noise, Texture)] _FoamMode ("     Source{Foam}", Float) = 1.0
        [NoScaleOffset] _NoiseMap("[_FOAMMODE_TEXTURE]           Texture{Foam}", 2D) = "white" {}
        _FoamColor("[!_FOAMMODE_NONE]     Color{Foam}", Color) = (1, 1, 1, 1)
        [Space]
        _FoamDepth("[!_FOAMMODE_NONE]     Shore Depth{Foam}", Float) = 0.5
        _FoamNoiseAmount("[!_FOAMMODE_NONE]     Shore Blending{Foam}", Range(0.0, 1.0)) = 1.0
        [Space]
        _FoamAmount("[!_FOAMMODE_NONE]     Amount{Foam}", Range(0, 3)) = 0.25
        [Space]
        _FoamScale("[!_FOAMMODE_NONE]     Scale{Foam}", Range(0, 3)) = 1
        _FoamStretchX("[!_FOAMMODE_NONE]     Stretch X{Foam}", Range(0, 10)) = 1
        _FoamStretchY("[!_FOAMMODE_NONE]     Stretch Y{Foam}", Range(0, 10)) = 1
        [Space]
        _FoamSharpness("[!_FOAMMODE_NONE]     Sharpness{Foam}", Range(0, 1)) = 0.5
        [Space]
        _FoamSpeed("[!_FOAMMODE_NONE]     Speed{Foam}", Float) = 0.1
        _FoamDirection("[!_FOAMMODE_NONE]     Direction{Foam}", Range(-1.0, 1.0)) = 0

        [Space][Header(Refraction)][Space]
        _RefractionFrequency("     Frequency", Float) = 35
        _RefractionAmplitude("     Amplitude", Range(0, 0.1)) = 0.01
        _RefractionSpeed("     Speed", Float) = 0.1
        _RefractionScale("     Scale", Float) = 1

        [Space][Header(Rendering options)][Space]
        [ToggleOff] _Opaque("     Opaque", Float) = 0.0
        [HideInInspector] _QueueOffset("Queue offset", Float) = 0.0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"
        }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        Lighting Off
        ZWrite[_ZWrite]

        Pass
        {
            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma target 2.0

            #pragma shader_feature_local _COLORMODE_LINEAR _COLORMODE_GRADIENT_TEXTURE
            #pragma shader_feature_local _FOAMMODE_NONE _FOAMMODE_GRADIENT_NOISE _FOAMMODE_TEXTURE
            #pragma shader_feature_local _WAVEMODE_NONE _WAVEMODE_ROUND _WAVEMODE_GRID _WAVEMODE_POINTY

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile_fragment _ _LIGHT_LAYERS
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile_fragment _ _WRITE_RENDERING_LAYERS

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma multi_compile_fragment _ DEBUG_DISPLAY
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            #if defined(_COLORMODE_GRADIENT_TEXTURE)
            TEXTURE2D(_ColorGradient);
            SAMPLER(sampler_ColorGradient);
            #endif

            TEXTURE2D(_NoiseMap);
            SAMPLER(sampler_NoiseMap);

            CBUFFER_START(UnityPerMaterial)
            float _FadeDistance, _WaterDepth;

            half _LightContribution;

            half _WaveFrequency, _WaveAmplitude, _WaveSpeed, _WaveDirection, _WaveNoise;
            half _WaterClearness, _CrestSize, _CrestSharpness, _ShadowStrength;

            half4 _CrestColor;
            half4 _FoamColor;
            half _FoamDepth, _FoamAmount, _FoamScale, _FoamSharpness, _FoamStretchX, _FoamStretchY, _FoamSpeed,
                 _FoamDirection, _FoamNoiseAmount, _RefractionFrequency, _RefractionAmplitude, _RefractionSpeed,
                 _RefractionScale, _FresnelAmount, _FresnelSharpness, _SunReflection;

            half4 _SpecularColor;
            half _SpecularStrength;

            float4 _NoiseMap_ST;

            // _COLORMODE_LINEAR:
            half4 _ColorShallow, _ColorDeep;
            // _COLORMODE_GRADIENT_TEXTURE:
            float4 _ColorGradient_ST;
            CBUFFER_END

            struct VertexInput
            {
                float4 positionOS : POSITION;
                float2 texcoord : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexOutput
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD6;
                float2 uv : TEXCOORD0;
                float4 screenPosition : TEXCOORD1;
                float waveHeight : TEXCOORD2;

                float3 normal : TEXCOORD3; // World space.
                float3 viewDir : TEXCOORD4; // World space.

                half fogFactor : TEXCOORD5;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float2 GradientNoise_Dir(float2 p)
            {
                // Permutation and hashing used in webgl-nosie goo.gl/pX7HtC
                // 3d0a9085-1fec-441a-bba6-f1121cdbe3ba
                p = p % 289;
                float x = (34 * p.x + 1) * p.x % 289 + p.y;
                x = (34 * x + 1) * x % 289;
                x = frac(x / 41) * 2 - 1;
                return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
            }

            float GradientNoise(float2 UV, float Scale)
            {
                const float2 p = UV * Scale;
                const float2 ip = floor(p);
                float2 fp = frac(p);
                const float d00 = dot(GradientNoise_Dir(ip), fp);
                const float d01 = dot(GradientNoise_Dir(ip + float2(0, 1)), fp - float2(0, 1));
                const float d10 = dot(GradientNoise_Dir(ip + float2(1, 0)), fp - float2(1, 0));
                const float d11 = dot(GradientNoise_Dir(ip + float2(1, 1)), fp - float2(1, 1));
                fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
                return lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
            }

            inline float DepthFade(float2 uv, VertexOutput i)
            {
                const float is_ortho = unity_OrthoParams.w;
                const float is_persp = 1.0 - unity_OrthoParams.w;

                const float depth_packed = SampleSceneDepth(uv);

                // Separately handles orthographic and perspective cameras.
                const float scene_depth = lerp(_ProjectionParams.z, _ProjectionParams.y, depth_packed) * is_ortho +
                    LinearEyeDepth(SampleSceneDepth(uv), _ZBufferParams) * is_persp;
                const float surface_depth = lerp(_ProjectionParams.z, _ProjectionParams.y, i.screenPosition.z) *
                    is_ortho + i.screenPosition.w * is_persp;

                const float water_depth = scene_depth - surface_depth;

                return saturate((water_depth - _FadeDistance) / _WaterDepth);
            }

            inline float SineWave(float3 pos, float offset)
            {
                return sin(
                    offset + _Time.z * _WaveSpeed + (pos.x * sin(offset + _WaveDirection * PI) + pos.z *
                        cos(offset + _WaveDirection * PI)) * _WaveFrequency);
            }

            inline float WaveHeight(float2 texcoord, float3 position)
            {
                float s = 0;

                #if !defined(_WAVEMODE_NONE)
                    float2 noise_uv = texcoord * _WaveFrequency;
                    float noise01 = GradientNoise(noise_uv, 1.0);
                    float noise = (noise01 * 2.0 - 1.0) * _WaveNoise;

                    s = SineWave(position, noise);

                #if defined(_WAVEMODE_GRID)
                        s *= SineWave(position, HALF_PI + noise);
                #endif

                #if defined(_WAVEMODE_POINTY)
                        s = 1.0 - abs(s);
                #endif
                #endif

                return s;
            }

            VertexOutput vert(VertexInput i)
            {
                VertexOutput o = (VertexOutput)0;

                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_TRANSFER_INSTANCE_ID(i, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                // Vertex animation.
                const float3 originalPositionWS = TransformObjectToWorld(i.positionOS.xyz);
                const float s = WaveHeight(i.texcoord, originalPositionWS);
                o.waveHeight = s;
                o.positionWS = originalPositionWS;
                o.positionWS.y += s * _WaveAmplitude;

                o.positionHCS = TransformWorldToHClip(o.positionWS);
                o.screenPosition = ComputeScreenPos(o.positionHCS);
                o.uv = i.texcoord;

                {
                    // Normals.
                    const float3 viewDirWS = GetCameraPositionWS() - o.positionWS;
                    o.viewDir = viewDirWS;

                    const VertexNormalInputs normalInput = GetVertexNormalInputs(i.normalOS, i.tangentOS);

                    const float sample_distance = 0.01;

                    float3 pos_tangent = originalPositionWS + normalInput.tangentWS * sample_distance;
                    pos_tangent.y += WaveHeight(i.texcoord, pos_tangent) * _WaveAmplitude;

                    float3 pos_bitangent = originalPositionWS + normalInput.bitangentWS * sample_distance;
                    pos_bitangent.y += WaveHeight(i.texcoord, pos_bitangent) * _WaveAmplitude;

                    const float3 modified_tangent = pos_tangent - o.positionWS;
                    const float3 modified_bitangent = pos_bitangent - o.positionWS;
                    const float3 modified_normal = cross(modified_tangent, modified_bitangent);

                    o.normal = normalize(modified_normal);
                }

                const half fogFactor = ComputeFogFactor(o.positionHCS.z);
                o.fogFactor = fogFactor;

                return o;
            }

            half4 frag(VertexOutput i) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                // Refraction.
                const float2 noise_uv_refraction = i.uv * _RefractionFrequency + _Time.zz * _RefractionSpeed;
                const float noise01_refraction = GradientNoise(noise_uv_refraction, _RefractionScale);
                const float noise11_refraction = noise01_refraction * 2.0f - 1.0f;
                const float2 screen_uv = i.screenPosition.xy / i.screenPosition.w;
                const float depth_fade_original = DepthFade(screen_uv, i);
                float2 displaced_uv = screen_uv + noise11_refraction * _RefractionAmplitude * depth_fade_original;
                float depth_fade = DepthFade(displaced_uv, i);

                if (depth_fade <= 0.0f) // If above water surface.
                {
                    displaced_uv = screen_uv;
                    depth_fade = DepthFade(displaced_uv, i);
                }

                const half3 scene_color = SampleSceneColor(displaced_uv);
                half3 c = scene_color;

                // Water depth.
                half4 depth_color;
                half4 color_shallow;
                #if defined(_COLORMODE_LINEAR)
                depth_color = lerp(_ColorShallow, _ColorDeep, depth_fade);
                color_shallow = _ColorShallow;
                #endif

                #if defined(_COLORMODE_GRADIENT_TEXTURE)
                float2 gradient_uv = float2(depth_fade, 0.5f);
                depth_color = SAMPLE_TEXTURE2D(_ColorGradient, sampler_ColorGradient, gradient_uv);
                color_shallow = SAMPLE_TEXTURE2D(_ColorGradient, sampler_ColorGradient, float2(0.0f, 0.5f));
                #endif

                c = lerp(depth_color.rgb, c, _WaterClearness * depth_color.a);

                // Crest.
                {
                    const half c_inv = 1.0f - _CrestSize;
                    c = lerp(c, _CrestColor.rgb,
                             smoothstep(c_inv, saturate(c_inv + (1.0f - _CrestSharpness)),
                                        i.waveHeight) * _CrestColor.a);
                }

                // Foam.
                #if !defined(_FOAMMODE_NONE)
                    float2 stretch_factor = float2(_FoamStretchX, _FoamStretchY);
                    float noise_foam_base;

                #if defined(_FOAMMODE_TEXTURE)
                    const float2 rotated_uv = i.uv * cos(_FoamDirection * PI) +
                        float2(i.uv.y, -i.uv.x) * sin(_FoamDirection * PI);
                    const float2 noise_uv_foam = rotated_uv * 100.0f + _Time.zz * _FoamSpeed;
                    noise_foam_base = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap,
                        noise_uv_foam * stretch_factor / (_FoamScale * 100.0)).r;
                #endif

                #if defined(_FOAMMODE_GRADIENT_NOISE)
                    // This rotation is not exactly correct, but we're keeping it for backwards compatibility.
                    const float uv_angle = atan2(i.uv.y, i.uv.x);
                    const float cs = cos(uv_angle + _FoamDirection * PI);
                    const float sn = sin(uv_angle + _FoamDirection * PI);
                    const float2 rotated_uv = float2(i.uv.x * cs - i.uv.y * sn, i.uv.x * sn + i.uv.y * cs);
                    const float2 noise_uv_foam = rotated_uv * 100.0f + _Time.zz * _FoamSpeed;
                    noise_foam_base = GradientNoise(noise_uv_foam * stretch_factor, _FoamScale);
                #endif

                    float foam_blur = 1.0 - _FoamSharpness;
                    float shore_fade = saturate(depth_fade / _FoamDepth);
                    float hard_foam_end = 0.1;
                    float soft_foam_end = hard_foam_end + foam_blur * 0.3;
                    float foam_shore = smoothstep(0.5 - foam_blur * 0.5, 0.5 + foam_blur * 0.5, noise_foam_base);
                    foam_shore = saturate(smoothstep(soft_foam_end, hard_foam_end, shore_fade) +
                        smoothstep(1, soft_foam_end, shore_fade) * foam_shore * _FoamNoiseAmount);

                    float foam_surface = smoothstep(noise_foam_base, noise_foam_base + foam_blur, _FoamAmount);
                    foam_surface = smoothstep(0.5 - foam_blur * 0.5, 0.5 + foam_blur * 0.5, foam_surface);

                    float foam = saturate(foam_shore + foam_surface);
                    c = lerp(c, _FoamColor.rgb, foam * _FoamColor.a);
                #endif

                // Shadow.
                #define _MAIN_LIGHT_SHADOWS  // Since URP 13 or 14 this is not defined by default.
                #if defined(_MAIN_LIGHT_SHADOWS)
                    VertexPositionInputs vertexInput = (VertexPositionInputs)0;
                    vertexInput.positionWS = i.positionWS.xyz;
                    float4 shadowCoord = GetShadowCoord(vertexInput);
                    half shadowAttenutation = MainLightRealtimeShadow(shadowCoord);
                    c = lerp(c, c * color_shallow.rgb, _ShadowStrength * (1.0h - shadowAttenutation));
                #endif

                c *= lerp(half3(1, 1, 1), _MainLightColor.rgb, _LightContribution);

                c = MixFog(c, i.fogFactor);

                return half4(c, 1);
            }
            ENDHLSL
        }
    }

    CustomEditor "FlatKitWaterEditor"
}