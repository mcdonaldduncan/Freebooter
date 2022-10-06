using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Go to PerlinNoise.cs for more indepth look
*/
public class PerlinNoisePositive : INoise
{
    FastNoise noise;
    PlanetSettings planetSettings;
    Dictionary<XYCoordinate, float> noiseValue;

    public PerlinNoisePositive(PlanetSettings planetSettings, NoiseSettings noiseSettings, int bump)
    {
        this.planetSettings = planetSettings;
        noise = new FastNoise();
        noise.SetNoiseType(FastNoise.NoiseType.PerlinFractal);
        noise.SetSeed(planetSettings.seed + bump);
        noise.SetFractalOctaves(noiseSettings.octaves);
        noise.SetFrequency(noiseSettings.frequency);
        noise.SetFractalLacunarity(noiseSettings.lacunarity);
        noise.SetFractalGain(noiseSettings.persistence);
        noiseValue = new Dictionary<XYCoordinate, float>();
        CalculateNoise();
    }
    void CalculateNoise()
    {
        for (int x = -planetSettings.planetPotentialOuterRadius; x <= planetSettings.planetPotentialOuterRadius; x++)
        {
            float constant = Mathf.Sqrt(planetSettings.planetPotentialOuterRadius * planetSettings.planetPotentialOuterRadius + x * x);
            constant = planetSettings.planetPotentialOuterRadius / constant;
            float xAxis = constant * x;
            float yAxis = constant * planetSettings.planetPotentialOuterRadius;
            XYCoordinate topCoordinate = new XYCoordinate(x, planetSettings.planetPotentialOuterRadius);
            XYCoordinate bottomCoordinate = new XYCoordinate(x, -planetSettings.planetPotentialOuterRadius);
            xAxis /= planetSettings.planetPotentialOuterRadius;
            yAxis /= planetSettings.planetPotentialOuterRadius;
            float topNoise = (noise.GetNoise(xAxis, yAxis) + 1) * 0.5f;
            float bottomNoise = (noise.GetNoise(xAxis, -yAxis) + 1) * 0.5f;
            noiseValue.Add(topCoordinate, topNoise);
            noiseValue.Add(bottomCoordinate, bottomNoise);
        }
        for (int y = -planetSettings.planetPotentialOuterRadius + 1; y < planetSettings.planetPotentialOuterRadius; y++)
        {
            float constant = Mathf.Sqrt(planetSettings.planetPotentialOuterRadius * planetSettings.planetPotentialOuterRadius + y * y);
            constant = planetSettings.planetPotentialOuterRadius / constant;
            float xAxis = constant * planetSettings.planetPotentialOuterRadius;
            float yAxis = constant * y;
            XYCoordinate leftCoordinate = new XYCoordinate(-planetSettings.planetPotentialOuterRadius, y);
            XYCoordinate rightCoordinate = new XYCoordinate(planetSettings.planetPotentialOuterRadius, y);
            xAxis /= planetSettings.planetPotentialOuterRadius;
            yAxis /= planetSettings.planetPotentialOuterRadius;
            float leftNoise = (noise.GetNoise(-xAxis, yAxis) + 1) * 0.5f;
            float rightNoise = (noise.GetNoise(xAxis, yAxis) + 1) * 0.5f;
            noiseValue.Add(leftCoordinate, leftNoise);
            noiseValue.Add(rightCoordinate, rightNoise);
        }
    }

    public float GetNoise(int x, int y)
    {
        if (x == 0 && y == 0) return 0;
        int absX = Mathf.Abs(x);
        int absY = Mathf.Abs(y);
        float constant = (absX >= absY) ? (float)planetSettings.planetPotentialOuterRadius / absX : (float)planetSettings.planetPotentialOuterRadius / absY;
        XYCoordinate coordinate = new XYCoordinate(Round(constant * x), Round(constant * y));
        noiseValue.TryGetValue(coordinate, out float value);
        return value;
    }
    public Dictionary<XYCoordinate, float> GetNoiseDictionary()
    {
        return noiseValue;
    }

    int Round(float x)
    {
        if (x < 0)
        {
            return (x - (int)x <= -0.5) ? (int)x - 1 : (int)x;
        }
        else if (x > 0)
        {
            return (x - (int)x >= 0.5) ? (int)x + 1 : (int)x;
        }
        return 0;
    }
}
