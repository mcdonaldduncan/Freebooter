using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Responsible for getting elevation of the planet
 * Takes into account layering/combining of noise as well as masking
 * 
 * Planet2D.cs will use this class. Therefore, go there next for more information after reading this part
*/
public class ShapeGenerator
{
    PlanetSettings planetSettings;
    INoise[] noiseLayers;

    public ShapeGenerator(PlanetSettings planetSettings)
    {
        this.planetSettings = planetSettings;
        noiseLayers = new INoise[planetSettings.noiseLayers.Count];
        // On the inspector, you manipulated each noiseLayer.
        // noiseLayer is currently either PerlinNoise.cs, PerlinNoisePositive.cs or PerlinNoiseNegative.cs
        // Take a look at PerlinNoise.cs for more details
        for (int i = 0; i < noiseLayers.Length; i++)
        {
            noiseLayers[i] = NoiseFactory.CreateNoiseCombined(planetSettings, planetSettings.noiseLayers[i].noiseSettings, i);
        }
        RecalculateWithMask();
    }

    // returns elevation for each pixel
    public float GetElevation(int x, int y)
    {
        float elevation = 0;
        for (int i = 0; i < noiseLayers.Length; i++)
        {
            if (planetSettings.noiseLayers[i].enabled)
            {
                // here amplitude is finally applied to the noise
                elevation += planetSettings.noiseLayers[i].noiseSettings.amplitude * noiseLayers[i].GetNoise(x, y);
            }
        }
        return elevation;
    }

    // Finds out maximum elevation so that unused pixels are not calculated (saves runtime)
    public float[] GetPlanetMinMaxElevation()
    {
        float[] elevation = new float[2];
        elevation[0] = float.PositiveInfinity;
        elevation[1] = float.NegativeInfinity;
        if (noiseLayers.Length > 0)
        {
            List<XYCoordinate> keys = new List<XYCoordinate>(noiseLayers[0].GetNoiseDictionary().Keys);
            foreach (XYCoordinate key in keys)
            {
                float tempElevation = 0;
                for (int i = 0; i < noiseLayers.Length; i++)
                {
                    if (planetSettings.noiseLayers[i].enabled)
                    {
                        tempElevation += planetSettings.noiseLayers[i].noiseSettings.amplitude * noiseLayers[i].GetNoiseDictionary()[key];
                    }
                }
                if (tempElevation > elevation[1])
                {
                    elevation[1] = tempElevation;
                }
                if (tempElevation < elevation[0])
                {
                    elevation[0] = tempElevation;
                }
            }
        }
        else
        {
            elevation[0] = 0;
            elevation[1] = 0;
        }
        return elevation;
    }

    // responsible for recalculating noise value if its masked by another noise value
    void RecalculateWithMask()
    {
        if (noiseLayers.Length > 0)
        {
            List<XYCoordinate> keys = new List<XYCoordinate>(noiseLayers[0].GetNoiseDictionary().Keys);
            for (int i = 0; i < noiseLayers.Length; i++)
            {
                foreach (XYCoordinate key in keys)
                {
                    float original = noiseLayers[i].GetNoiseDictionary()[key];
                    float mask = 1;
                    if (planetSettings.noiseLayers[i].maskedBy >= 0 && planetSettings.noiseLayers[i].maskedBy < noiseLayers.Length)
                    {
                        mask = noiseLayers[planetSettings.noiseLayers[i].maskedBy].GetNoiseDictionary()[key];
                        if (mask * original > 0 && mask < 0)
                        {
                            mask = -mask;
                        }
                        else if (mask * original < 0)
                        {
                            mask = 0;
                        }
                    }
                    // mask is multiplied into the noise value
                    noiseLayers[i].GetNoiseDictionary()[key] = mask * original;
                }
            }
        }
    }
}
