using UnityEngine;
using UnityEngine.Rendering.Universal;

// ReSharper disable RedundantDefaultMemberInitializer

namespace FlatKit {
[CreateAssetMenu(fileName = "FogSettings", menuName = "FlatKit/Fog Settings")]
public class FogSettings : ScriptableObject {
    [Header("Distance Fog")]
    public bool useDistance = true;
    public Gradient distanceGradient;
    public float near = 0;
    public float far = 100;

    [Range(0, 1)]
    public float distanceFogIntensity = 1.0f;
    public bool useDistanceFogOnSky = false;

    [Header("Height Fog")]
    [Space]
    public bool useHeight = false;
    public Gradient heightGradient;
    public float low = 0;
    public float high = 10;

    [Range(0, 1)]
    public float heightFogIntensity = 1.0f;
    public bool useHeightFogOnSky = false;

    [Header("Blending")]
    [Space]
    [Range(0, 1)]
    public float distanceHeightBlend = 0.5f;

    [Header("Advanced settings")]
    [Space, Tooltip("The render stage at which the effect is applied. To exclude transparent objects, like water or " +
                    "UI elements, set this to \"Before Transparent\".")]
    public RenderPassEvent renderEvent = RenderPassEvent.BeforeRenderingPostProcessing;

    [Tooltip("Whether the effect should be applied in the Scene view as well as in the Game view. Please keep in " +
             "mind that Unity always renders the scene view with the default Renderer settings of the URP config.")]
    public bool applyInSceneView = true;

    private void OnValidate() {
        low = Mathf.Min(low, high);
        high = Mathf.Max(low, high);
    }
}
}