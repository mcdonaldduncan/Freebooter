using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Calculates one layer of noise values and stores them into Dictionary
 * 
 * PerlinNoiseNegative.cs and PerlinNoisePositive.cs are very similar to PerlinNoise.cs
 * Only difference is that PerlinNoiseNegative.cs only generates negative noise values and
 * PerlinNoisePositive.cs only generates positive noise values
 * 
 * ShapeGenerator.cs will use this class. Therefore, go there next for more information after reading this part
*/
public class PerlinNoise : INoise
{
    // external library that generates noise
    FastNoise noise;
    PlanetSettings planetSettings;
    Dictionary<XYCoordinate, float> noiseValue;
    
    // bump is there to make sure every noise layer is unique
    public PerlinNoise(PlanetSettings planetSettings, NoiseSettings noiseSettings, int bump)
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
    /*
     * To create a 2-dimensional circular planet, the elevation of the outer edge of the planet is required.
     * These values of elevation can be retrieved from a noise map.
     * Even though these values of elevation are essentially 1-dimensional line around the planet,
     * the values must be from a cirle on 2-dimensional noise map so that the ends meet.
     * (Imagine drawing a circle on the 2-dimensional noise map. That will be the values used on the planet)
     * Again, it is important to emphasize that the values from 2-dimensional noise map must be circular
     * as sharp edges created by a square will create visible seams on the planet
     * These values are retrieved and stored in a Dictionary<XYCoordinate, float>
     * XYCoordinate represents basically what it says, "an XY coordinate"
     * However, it is important to know in what shape these coordinates are imbedded into the Dictionary
     * The shape is square. It may seem weird because a circle was used to get the noise from the 2-dimensional noise map.
     * However, when the noise values are stored, they are stored in a square shape with its center at (0,0).
     * This is to reduce the amount of Mathf.Sqrt() from being used.
     * Square allows the program to simply use linear extrapolation to get the noise values,
     * but the values it represents will be a circle on a 2-dimensional noise map.
     * Another reason for square shape is due to the shape of the sprite which will also be square shaped

     * TLDR: this function basically creates a Dictionary of noise values from a 2 Dimensional noise map
     */
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
            float topNoise = noise.GetNoise(xAxis, yAxis);
            float bottomNoise = noise.GetNoise(xAxis, -yAxis);
            noiseValue.Add(topCoordinate, topNoise);
            noiseValue.Add(bottomCoordinate, bottomNoise);
        }
        // + 1 due to overlap with x axis
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
            float leftNoise = noise.GetNoise (-xAxis, yAxis);
            float rightNoise = noise.GetNoise(xAxis, yAxis);
            noiseValue.Add(leftCoordinate, leftNoise);
            noiseValue.Add(rightCoordinate, rightNoise);
        }
    }

    // gets noise value for that coordinate
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
