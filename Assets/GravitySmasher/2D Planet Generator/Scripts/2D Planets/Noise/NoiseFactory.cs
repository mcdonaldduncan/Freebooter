using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseFactory
{
    // creates noise class based on user's choice of which noise to use
    public static INoise CreateNoiseCombined(PlanetSettings planetSettings, NoiseSettings noiseSettings, int bump)
    {
        switch (noiseSettings.noiseType)
        {
            case NoiseSettings.NoiseType.Perlin:
                return new PerlinNoise(planetSettings, noiseSettings, bump);
            case NoiseSettings.NoiseType.PerlinPositive:
                return new PerlinNoisePositive(planetSettings, noiseSettings, bump);
            case NoiseSettings.NoiseType.PerlinNegative:
                return new PerlinNoiseNegative(planetSettings, noiseSettings, bump);
            default:
                return null;
        }
    }
}
