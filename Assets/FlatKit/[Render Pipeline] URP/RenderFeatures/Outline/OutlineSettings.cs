using UnityEngine;
using UnityEngine.Rendering.Universal;

// ReSharper disable RedundantDefaultMemberInitializer

namespace FlatKit {
[CreateAssetMenu(fileName = "OutlineSettings", menuName = "FlatKit/Outline Settings")]
public class OutlineSettings : ScriptableObject {
    public Color edgeColor = Color.white;

    [Range(0, 5)]
    public int thickness = 1;

    [Tooltip("If enabled, the line width will stay constant regardless of the rendering resolution. " +
             "However, some of the lines may appear blurry.")]
    public bool resolutionInvariant = false;

    [Space]
    public bool useDepth = true;
    public bool useNormals = false;
    public bool useColor = false;

    [Header("Advanced settings")]
    public float minDepthThreshold = 0f;
    public float maxDepthThreshold = 0.25f;

    [Space]
    public float minNormalsThreshold = 0f;
    public float maxNormalsThreshold = 0.25f;

    [Space]
    public float minColorThreshold = 0f;
    public float maxColorThreshold = 0.25f;

    [Space, Tooltip("The render stage at which the effect is applied. To exclude transparent objects, " +
                    "like water or UI elements, set this to \"Before Transparent\".")]
    public RenderPassEvent renderEvent = RenderPassEvent.BeforeRenderingPostProcessing;

    public bool outlineOnly = false;

    [Tooltip("Whether the effect should be applied in the Scene view as well as in the Game view. Please keep in " +
             "mind that Unity always renders the scene view with the default Renderer settings of the URP config.")]
    public bool applyInSceneView = true;

    private void OnValidate() {
        if (minDepthThreshold > maxDepthThreshold + float.Epsilon) {
            Debug.LogWarning("<b>[Flat Kit]</b> Outline configuration error: 'Min Depth Threshold' must not " +
                             "be greater than 'Max Depth Threshold'");
        }

        if (minNormalsThreshold > maxNormalsThreshold + float.Epsilon) {
            Debug.LogWarning("<b>[Flat Kit]</b> Outline configuration error: 'Min Normals Threshold' must not " +
                             "be greater than 'Max Normals Threshold'");
        }

        if (minColorThreshold > maxColorThreshold + float.Epsilon) {
            Debug.LogWarning("<b>[Flat Kit]</b> Outline configuration error: 'Min Color Threshold' must not " +
                             "be greater than 'Max Color Threshold'");
        }
    }
}
}