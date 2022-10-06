using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PlanetSettings : ScriptableObject
{
    [HideInInspector]
    public int planetPotentialOuterRadius;
    public int seed;
    [Range(1, 1000)]
    public int planetRadius;
    [Space(20)]
    public List<NoiseLayer> noiseLayers;
    [Space(20)]
    public Gradient coreColor;
    [Space(20)]
    public Gradient crustColor;
    public int crustThickness;
    [Space(20)]
    public Gradient waterColor;
    public int waterDepth;

    // On the inspector, unity seems to instantiate objects with something similar to OnValidate()
    // However, if user tries to push create planet without triggering OnValidate(), error occurs
    // Therefore, objects need to be instantiated with constructor
    public PlanetSettings()
    {
        planetRadius = 100;
        noiseLayers = new List<NoiseLayer>();
        noiseLayers.Add(new NoiseLayer());
        coreColor = new Gradient();
        crustColor = new Gradient();
        waterColor = new Gradient();
        // OnValidate is called to make sure planetPotentialOuterRadius is correct
        OnValidate();
    }

    void OnValidate()
    {
        // creates the maximum potential radius required to fit the planet in the sprite
        // "potential" is important here because the actual radius will be lower
        float constant = 1;
        foreach (var noiseLayer in noiseLayers)
        {
            constant += noiseLayer.noiseSettings.amplitude;
        }
        planetPotentialOuterRadius = (int)(constant * planetRadius);
    }

    [System.Serializable]
    public class NoiseLayer
    {
        public bool enabled;
        public int maskedBy;
        public NoiseSettings noiseSettings;

        public NoiseLayer()
        {
            enabled = true;
            maskedBy = -1;
            noiseSettings = new NoiseSettings();
            noiseSettings.octaves = 1;
            noiseSettings.lacunarity = 1;
        }
    }
}
