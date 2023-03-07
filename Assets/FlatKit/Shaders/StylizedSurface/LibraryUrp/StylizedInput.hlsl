#ifndef FLAT_KIT_STYLIZED_INPUT_INCLUDED
#define FLAT_KIT_STYLIZED_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

#ifndef FLATKIT_TERRAIN
CBUFFER_START(UnityPerMaterial)
#endif

// --- `SimpleLitInput.hlsl` ---
float4 _BaseMap_ST;
#ifndef FLATKIT_TERRAIN
half4 _BaseColor;
half _Cutoff;
half _Surface;
#endif
half4 _EmissionColor;
// -----------------------------

half4 _UnityShadowColor;

// --- _CELPRIMARYMODE_SINGLE
half4 _ColorDim;
// --- _CELPRIMARYMODE_SINGLE

// --- DR_SPECULAR_ON
half4 _FlatSpecularColor;
float _FlatSpecularSize;
float _FlatSpecularEdgeSmoothness;
// --- DR_SPECULAR_ON

// --- DR_RIM_ON
half4 _FlatRimColor;
float _FlatRimSize;
float _FlatRimEdgeSmoothness;
float _FlatRimLightAlign;
// --- DR_RIM_ON

// --- _CELPRIMARYMODE_STEPS
half4 _ColorDimSteps;
sampler2D _CelStepTexture;
// --- _CELPRIMARYMODE_STEPS

// --- _CELPRIMARYMODE_CURVE
half4 _ColorDimCurve;
sampler2D _CelCurveTexture;
// --- _CELPRIMARYMODE_CURVE

// --- DR_CEL_EXTRA_ON
half4 _ColorDimExtra;
half _SelfShadingSizeExtra;
half _ShadowEdgeSizeExtra;
half _FlatnessExtra;
// --- DR_CEL_EXTRA_ON

// --- DR_GRADIENT_ON
half4 _ColorGradient;
half _GradientCenterX;
half _GradientCenterY;
half _GradientSize;
half _GradientAngle;
// --- DR_GRADIENT_ON

half _TextureImpact;

half _SelfShadingSize;
half _ShadowEdgeSize;
half _LightContribution;
half _LightFalloffSize;
half _Flatness;

half _UnityShadowPower;
half _UnityShadowSharpness;

half _OverrideLightmapDir;
half3 _LightmapDirection;

half4 _OutlineColor;
half _OutlineWidth;
half _OutlineScale;
half _OutlineDepthOffset;
half _CameraDistanceImpact;

// Unused, required in Meta pass.
#ifndef FLATKIT_TERRAIN
half4 _SpecColor;
TEXTURE2D(_SpecGlossMap);       SAMPLER(sampler_SpecGlossMap);
#endif

#ifndef FLATKIT_TERRAIN
CBUFFER_END
#endif

inline void InitializeSimpleLitSurfaceData(float2 uv, out SurfaceData outSurfaceData)
{
    outSurfaceData = (SurfaceData)0;

    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    outSurfaceData.alpha = albedoAlpha.a * _BaseColor.a;
    AlphaDiscard(outSurfaceData.alpha, _Cutoff);

    outSurfaceData.albedo = albedoAlpha.rgb;
    #ifdef _ALPHAPREMULTIPLY_ON
    outSurfaceData.albedo *= outSurfaceData.alpha;
    #endif

    outSurfaceData.metallic = 0.0; // unused
    outSurfaceData.specular = 0.0; // unused
    outSurfaceData.smoothness = 0.0; // unused
    outSurfaceData.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));
    outSurfaceData.occlusion = 1.0; // unused
    outSurfaceData.emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
}

half4 SampleSpecularSmoothness(half2 uv, half alpha, half4 specColor, TEXTURE2D_PARAM(specMap, sampler_specMap))
{
    half4 specularSmoothness = half4(0.0h, 0.0h, 0.0h, 1.0h);
    return specularSmoothness;
}

#endif  // FLAT_KIT_STYLIZED_INPUT_INCLUDED
