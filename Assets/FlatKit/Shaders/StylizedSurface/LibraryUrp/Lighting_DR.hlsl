#ifndef FLATKIT_LIGHTING_DR_INCLUDED
#define FLATKIT_LIGHTING_DR_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

inline half NdotLTransition(half3 normal, half3 lightDir, half selfShadingSize, half shadowEdgeSize, half flatness) {
    const half NdotL = dot(normal, lightDir);
    const half angleDiff = saturate((NdotL * 0.5 + 0.5) - selfShadingSize);
    const half angleDiffTransition = smoothstep(0, shadowEdgeSize, angleDiff); 
    return lerp(angleDiff, angleDiffTransition, flatness);
}

inline half NdotLTransitionPrimary(half3 normal, half3 lightDir) { 
    return NdotLTransition(normal, lightDir, _SelfShadingSize, _ShadowEdgeSize, _Flatness);
}

#if defined(DR_CEL_EXTRA_ON)
inline half NdotLTransitionExtra(half3 normal, half3 lightDir) { 
    return NdotLTransition(normal, lightDir, _SelfShadingSizeExtra, _ShadowEdgeSizeExtra, _FlatnessExtra);
}
#endif

inline half NdotLTransitionTexture(half3 normal, half3 lightDir, sampler2D stepTex) {
    const half NdotL = dot(normal, lightDir);
    const half angleDiff = saturate((NdotL * 0.5 + 0.5) - _SelfShadingSize * 0.0);
    const half4 rampColor = tex2D(stepTex, half2(angleDiff, 0.5));
    // NOTE: The color channel here corresponds to the texture format in the shader editor script.
    const half angleDiffTransition = rampColor.r;
    return angleDiffTransition;
}

half3 LightingPhysicallyBased_DSTRM(Light light, half3 normalWS, half3 viewDirectionWS, float3 positionWS)
{
    // If all light in the scene is baked, we use custom light direction for the cel shading.
    light.direction = lerp(light.direction, _LightmapDirection, _OverrideLightmapDir);

    half4 c = _BaseColor;

#if defined(_CELPRIMARYMODE_SINGLE)
    const half NdotLTPrimary = NdotLTransitionPrimary(normalWS, light.direction);
    c = lerp(_ColorDim, c, NdotLTPrimary);
#endif  // _CELPRIMARYMODE_SINGLE

#if defined(_CELPRIMARYMODE_STEPS)
    const half NdotLTSteps = NdotLTransitionTexture(normalWS, light.direction, _CelStepTexture);
    c = lerp(_ColorDimSteps, c, NdotLTSteps);
#endif  // _CELPRIMARYMODE_STEPS

#if defined(_CELPRIMARYMODE_CURVE)
    const half NdotLTCurve = NdotLTransitionTexture(normalWS, light.direction, _CelCurveTexture);
    c = lerp(_ColorDimCurve, c, NdotLTCurve);
#endif  // _CELPRIMARYMODE_CURVE

#if defined(DR_CEL_EXTRA_ON)
    const half NdotLTExtra = NdotLTransitionExtra(normalWS, light.direction);
    c = lerp(_ColorDimExtra, c, NdotLTExtra);
#endif  // DR_CEL_EXTRA_ON

#if defined(DR_GRADIENT_ON)
    const float angleRadians = _GradientAngle / 180.0 * PI;
    const float posGradRotated = (positionWS.x - _GradientCenterX) * sin(angleRadians) + 
                           (positionWS.y - _GradientCenterY) * cos(angleRadians);
    const float gradientTop = _GradientCenterY + _GradientSize * 0.5;
    const half gradientFactor = saturate((gradientTop - posGradRotated) / _GradientSize);
    c = lerp(c, _ColorGradient, gradientFactor);
#endif  // DR_GRADIENT_ON

    const half NdotL = dot(normalWS, light.direction);

#if defined(DR_RIM_ON)
    const float rim = 1.0 - dot(viewDirectionWS, normalWS);
    const float rimSpread = 1.0 - _FlatRimSize - NdotL * _FlatRimLightAlign;
    const float rimEdgeSmooth = _FlatRimEdgeSmoothness;
    const float rimTransition = smoothstep(rimSpread - rimEdgeSmooth * 0.5, rimSpread + rimEdgeSmooth * 0.5, rim);
    c = lerp(c, _FlatRimColor, rimTransition);
#endif  // DR_RIM_ON

#if defined(DR_SPECULAR_ON)
    // Halfway between lighting direction and view vector.
    const float3 halfVector = normalize(light.direction + viewDirectionWS);
    const float NdotH = dot(normalWS, halfVector) * 0.5 + 0.5;
    const float specular = saturate(pow(abs(NdotH), 100.0 * (1.0 - _FlatSpecularSize) * (1.0 - _FlatSpecularSize)));
    const float specularTransition = smoothstep(0.5 - _FlatSpecularEdgeSmoothness * 0.1,
                                                0.5 + _FlatSpecularEdgeSmoothness * 0.1, specular);
    c = lerp(c, _FlatSpecularColor, specularTransition);
#endif  // DR_SPECULAR_ON

#if defined(_UNITYSHADOW_OCCLUSION)
    const float occludedAttenuation = smoothstep(0.25, 0.0, -min(NdotL, 0));
    light.shadowAttenuation *= occludedAttenuation;
    light.distanceAttenuation *= occludedAttenuation;
#endif

#if defined(_UNITYSHADOWMODE_MULTIPLY)
    c *= lerp(1, light.shadowAttenuation, _UnityShadowPower);
#endif
#if defined(_UNITYSHADOWMODE_COLOR)
    c = lerp(lerp(c, _UnityShadowColor, _UnityShadowColor.a), c, light.shadowAttenuation);
#endif

    c.rgb *= light.color * light.distanceAttenuation;

    return c.rgb;
}

void StylizeLight(inout Light light)
{
    const half shadowAttenuation = saturate(light.shadowAttenuation * _UnityShadowSharpness);
    light.shadowAttenuation = shadowAttenuation;

    const half distanceAttenuation = smoothstep(0, _LightFalloffSize + 0.001, light.distanceAttenuation);
    light.distanceAttenuation = distanceAttenuation;

    const half3 lightColor = lerp(half3(1, 1, 1), light.color, _LightContribution);
    light.color = lightColor;
}

half4 UniversalFragment_DSTRM(InputData inputData, half3 albedo, half3 emission, half alpha)
{
    // To ensure backward compatibility we have to avoid using shadowMask input, as it is not present in older shaders
    #if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
    const half4 shadowMask = inputData.shadowMask;
    #elif !defined (LIGHTMAP_ON)
    const half4 shadowMask = unity_ProbesOcclusion;
    #else
    const half4 shadowMask = half4(1, 1, 1, 1);
    #endif

    #if VERSION_GREATER_EQUAL(10, 0)
    Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, shadowMask);
    #else
    Light mainLight = GetMainLight(inputData.shadowCoord);
    #endif

#if LIGHTMAP_ON
    mainLight.distanceAttenuation = 1.0;
#endif
    StylizeLight(mainLight);

    #if defined(_SCREEN_SPACE_OCCLUSION)
        AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(inputData.normalizedScreenSpaceUV);
        mainLight.color *= aoFactor.directAmbientOcclusion;
        inputData.bakedGI *= aoFactor.indirectAmbientOcclusion;
    #endif

    MixRealtimeAndBakedGI(mainLight, inputData.normalWS, inputData.bakedGI, shadowMask);

    // Apply Flat Kit stylizing to `inputData.bakedGI` (which is half3).
#if LIGHTMAP_ON
    #if defined(_UNITYSHADOWMODE_MULTIPLY)
        inputData.bakedGI *= _UnityShadowPower;
    #endif
    #if defined(_UNITYSHADOWMODE_COLOR)
        float giLength = length(inputData.bakedGI);
        inputData.bakedGI = lerp(giLength, _UnityShadowColor.rgb, _UnityShadowColor.a * giLength);
    #endif
#endif

    BRDFData brdfData;
    // Albedo should be set to 0 here because it is applied in `StylizedPassFragment`.
    InitializeBRDFData(albedo, 1.0 - 1.0 / kDieletricSpec.a, 0, 0, alpha, brdfData);
    half3 color = GlobalIllumination(brdfData, inputData.bakedGI, 1.0, inputData.normalWS, inputData.viewDirectionWS);
    color += LightingPhysicallyBased_DSTRM(mainLight, inputData.normalWS, inputData.viewDirectionWS, inputData.positionWS);

#ifdef _ADDITIONAL_LIGHTS
    const uint pixelLightCount = GetAdditionalLightsCount();
    for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
    {
        #if VERSION_GREATER_EQUAL(10, 0)
        Light light = GetAdditionalLight(lightIndex, inputData.positionWS, shadowMask);
        #else
        Light light = GetAdditionalLight(lightIndex, inputData.positionWS);
        #endif

        #if defined(_SCREEN_SPACE_OCCLUSION)
            light.color *= aoFactor.directAmbientOcclusion;
        #endif

        StylizeLight(light);
        color += LightingPhysicallyBased_DSTRM(light, inputData.normalWS, inputData.viewDirectionWS, inputData.positionWS);
    }
#endif

#ifdef _ADDITIONAL_LIGHTS_VERTEX
    color += inputData.vertexLighting * brdfData.diffuse;
#endif

    color += emission;
    return half4(color, alpha);
}

#endif // FLATKIT_LIGHTING_DR_INCLUDED