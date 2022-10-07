using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoiseSettings
{
    public enum NoiseType { Perlin, PerlinPositive, PerlinNegative };
    public NoiseType noiseType;

    [Range(1, 5)]
    public int octaves;
    [Range(0f, 10f)]
    public float frequency;
    [Range(1f, 5f)]
    public float lacunarity;
    [Range(0f, 0.5f)]
    public float amplitude;
    [Range(0f, 1f)]
    public float persistence;
}
