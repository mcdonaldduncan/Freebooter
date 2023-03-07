#ifndef FLATKIT_LIGHT_PASS_DR_INCLUDED
#define FLATKIT_LIGHT_PASS_DR_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Lighting_DR.hlsl"

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float2 texcoord     : TEXCOORD0;
    float2 lightmapUV   : TEXCOORD1;

#if defined(DR_VERTEX_COLORS_ON)
    float4 color        : COLOR;
#endif

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv                       : TEXCOORD0;
    DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);

    float3 posWS                    : TEXCOORD2;    // xyz: posWS

#ifdef _NORMALMAP
    float4 normal                   : TEXCOORD3;    // xyz: normal, w: viewDir.x
    float4 tangent                  : TEXCOORD4;    // xyz: tangent, w: viewDir.y
    float4 bitangent                : TEXCOORD5;    // xyz: bitangent, w: viewDir.z
#else
    float3  normal                  : TEXCOORD3;
    float3 viewDir                  : TEXCOORD4;
#endif

    half4 fogFactorAndVertexLight   : TEXCOORD6; // x: fogFactor, yzw: vertex light

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord              : TEXCOORD7;
#endif

    float4 positionCS               : SV_POSITION;
#if defined(DR_VERTEX_COLORS_ON)
    float4 VertexColor              : COLOR;
#endif

    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

/// ---------------------------------------------------------------------------

void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
{
    inputData = (InputData)0;

    inputData.positionWS = input.posWS;

#ifdef _NORMALMAP
    half3 viewDirWS = half3(input.normal.w, input.tangent.w, input.bitangent.w);
    inputData.normalWS = TransformTangentToWorld(normalTS,
        half3x3(input.tangent.xyz, input.bitangent.xyz, input.normal.xyz));
#else
    half3 viewDirWS = input.viewDir;
    inputData.normalWS = input.normal;
#endif

    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    viewDirWS = SafeNormalize(viewDirWS);

    inputData.viewDirectionWS = viewDirWS;

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    inputData.shadowCoord = input.shadowCoord;
#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
#else
    inputData.shadowCoord = float4(0, 0, 0, 0);
#endif

    inputData.fogCoord = input.fogFactorAndVertexLight.x;
    inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
    inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);

#if VERSION_GREATER_EQUAL(10, 0)
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);
#endif

#if VERSION_GREATER_EQUAL(12, 0)
    inputData.positionCS = input.positionCS;
#if defined(_NORMALMAP)
    inputData.tangentToWorld = half3x3(input.tangent.xyz, input.bitangent.xyz, input.normal.xyz);
#endif

    #if defined(DEBUG_DISPLAY)
    #if defined(DYNAMICLIGHTMAP_ON)
    inputData.dynamicLightmapUV = input.dynamicLightmapUV.xy;
    #endif
    #if defined(LIGHTMAP_ON)
    inputData.staticLightmapUV = input.staticLightmapUV;
    #else
    inputData.vertexSH = input.vertexSH;
    #endif
    #endif
#endif
}

Varyings StylizedPassVertex(Attributes input)
{
    #if defined(CURVEDWORLD_IS_INSTALLED) && !defined(CURVEDWORLD_DISABLED_ON)
    #ifdef CURVEDWORLD_NORMAL_TRANSFORMATION_ON
        CURVEDWORLD_TRANSFORM_VERTEX_AND_NORMAL(input.positionOS, input.normalOS, input.tangentOS)
    #else
        CURVEDWORLD_TRANSFORM_VERTEX(input.positionOS)
    #endif
    #endif

    UNITY_SETUP_INSTANCE_ID(input);

    Varyings output = (Varyings)0;
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    const VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    half3 viewDirWS = GetCameraPositionWS() - vertexInput.positionWS;
    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
    half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    output.posWS.xyz = vertexInput.positionWS;
    output.positionCS = vertexInput.positionCS;

#ifdef _NORMALMAP
    output.normal = half4(normalInput.normalWS, viewDirWS.x);
    output.tangent = half4(normalInput.tangentWS, viewDirWS.y);
    output.bitangent = half4(normalInput.bitangentWS, viewDirWS.z);
#else
    output.normal = NormalizeNormalPerVertex(normalInput.normalWS);
    output.viewDir = viewDirWS;
#endif

    OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
    OUTPUT_SH(output.normal.xyz, output.vertexSH);

    output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    output.shadowCoord = GetShadowCoord(vertexInput);
#endif

#if defined(DR_VERTEX_COLORS_ON)
    output.VertexColor = input.color;
#endif

    return output;
}

half4 StylizedPassFragment(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    SurfaceData surfaceData;
    InitializeSimpleLitSurfaceData(input.uv, surfaceData);

    InputData inputData;
    InitializeInputData(input, surfaceData.normalTS, inputData);

#if VERSION_GREATER_EQUAL(12, 1)
    SETUP_DEBUG_TEXTURE_DATA(inputData, input.uv, _BaseMap);
#endif

#ifdef _DBUFFER
    ApplyDecalToSurfaceData(input.positionCS, surfaceData, inputData);
#endif

    // Computes direct light contribution.
    half4 color = UniversalFragment_DSTRM(inputData, surfaceData.albedo * _BaseColor.rgb, surfaceData.emission, surfaceData.alpha);

    {
#if defined(_TEXTUREBLENDINGMODE_ADD)
        color.rgb += lerp(half3(0.0f, 0.0f, 0.0f), surfaceData.albedo, _TextureImpact);
#else  // _TEXTUREBLENDINGMODE_MULTIPLY
        // This is the default blending mode for compatibility with the v.1 of the asset.
        color.rgb *= lerp(half3(1.0f, 1.0f, 1.0f), surfaceData.albedo, _TextureImpact);
#endif
    }

#if defined(DR_VERTEX_COLORS_ON)
    color.rgb *= input.VertexColor.rgb;
#endif

    color.rgb = MixFog(color.rgb, inputData.fogCoord);

#if VERSION_GREATER_EQUAL(10, 0)
    color.a = OutputAlpha(color.a, _Surface);
#endif

    return color;
}

#endif // FLATKIT_LIGHT_PASS_DR_INCLUDED
