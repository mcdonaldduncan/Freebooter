using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 * Hello, everyone. If you are looking through the code, I assume you would like to know how I managed to create a 2D planet.
 * Well, I will try my best to explain how it works. If there are areas that need improvement, questions or bugs, please email me at chihyunsong.com@gmail.com
 * 
 * You should start by reading the comments from PerlinNoise.cs if you would like to understand the code from ground up
 * because that is where noise calculations first start
 * 
 * Also, all my noises come from a third party library called FastNoise
*/
[RequireComponent(typeof(SpriteRenderer))]
public class Planet2D : MonoBehaviour
{
    // Scriptable objects contain all the settings required to create the planet.
    // These settings are visible on the inspector for easy changes
    public PlanetSettings planetSettings;
    // File path that the sprite can be exported as png file
    [HideInInspector]
    public string filePath;
    // Allows folding of settings in the inspector so that the inspector doesn't get too cluttered
    [HideInInspector]
    public bool planetSettingsFoldout;
    Texture2D tex;

    public void CreatePlanet2D()
    {
        // check if Settings are inputted correctly
        if (CheckSettings(planetSettings))
        {
            // ShapeGenerator contains all the noise values required to create the 2D Planets
            // More information on ShapeGenerator.cs
            ShapeGenerator shapeGenerator = new ShapeGenerator(planetSettings);
            SpriteRenderer rend = GetComponent<SpriteRenderer>();
            // returns minimum and maximum elevation of the planet
            float[] minMaxElevation = shapeGenerator.GetPlanetMinMaxElevation();
            int maxRadius = (int)(planetSettings.planetRadius * (1 + minMaxElevation[1]));
            int minRadius = (int)(planetSettings.planetRadius * (1 + minMaxElevation[0]));
            int maxRadiusWithWater = maxRadius;
            // Takes water depth into account when water depth is higher than the highest elevation
            // In other words, ocean planets!
            if (minRadius + planetSettings.waterDepth > maxRadiusWithWater)
            {
                maxRadiusWithWater = minRadius + planetSettings.waterDepth;
            }
            // MaxRadiusWithWater is very important here because 
            // it allows the program to limit the unnesscary calculation of pixels that will never be part of the 2D Planet
            tex = new Texture2D(maxRadiusWithWater * 2, maxRadiusWithWater * 2);
            Color[] colorArray = new Color[tex.width * tex.height];
            // This is where pixels get their color based on their distance from the center
            for (int y = 0; y < tex.height; y++)
            {
                for (int x = 0; x < tex.width; x++)
                {
                    int xAxis = x - maxRadiusWithWater;
                    int yAxis = y - maxRadiusWithWater;
                    int colorIndex = x + y * tex.width;
                    // distance of pixel from center of planet
                    float distance = Mathf.Sqrt(xAxis * xAxis + yAxis * yAxis);
                    // elevation of planet from angle that pixel is in
                    float elevation = planetSettings.planetRadius * (1 + shapeGenerator.GetElevation(xAxis, yAxis));

                    // if distance of the pixel from the center is less than elevation of the planet
                    // then it is part of the terrain and should be colored in
                    if (distance < elevation)
                    {
                        if (distance > maxRadius - planetSettings.crustThickness)
                        {
                            colorArray[colorIndex] = planetSettings.crustColor.Evaluate((distance + planetSettings.crustThickness - maxRadius) / 
                                (elevation + planetSettings.crustThickness - maxRadius));
                        }
                        else
                        {
                            colorArray[colorIndex] = planetSettings.coreColor.Evaluate(distance / maxRadius);
                        }
                    }
                    // if distance of the pixel from the center is more than elevation of the planet
                    // it should not be colored in unless it is water
                    else
                    {
                        if (distance < minRadius + planetSettings.waterDepth)
                        {
                            colorArray[colorIndex] = planetSettings.waterColor.Evaluate((distance - minRadius) / planetSettings.waterDepth);
                        }
                        else
                        {
                            colorArray[colorIndex] = Color.clear;
                        }
                    }
                }
            }

            tex.SetPixels(colorArray);
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
            rend.sprite = newSprite;
        }
    }

    // Exports the sprite as PNG file for later use
    public void ExportAsPNG()
    {
        if (!filePath.Substring(Mathf.Max(0, filePath.Length - 4)).Equals(".png"))
        {
            Debug.LogError("Export As PNG: Add .png at the end");
        } else
        {
            byte[] _bytes = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(Application.dataPath + "/" + filePath, _bytes);
        }
    }

    // stops incorrect, undesired or missing inputs
    // I put some limits here so that users wouldn't try outrageous values that will result in unity stopping
    // However, if you would like more control over it, change it here and in the NoiseSettings.cs
    bool CheckSettings(PlanetSettings planetSettings)
    {
        bool ok = true;
        if (planetSettings == null)
        {
            Debug.LogError("Planet Settings is missing. You are required to apply Planet Settings to Planet2D.cs");
            return false;
        }
        if (1 > planetSettings.planetRadius || 1000 < planetSettings.planetRadius)
        {
            Debug.LogError("Planet Radius out of bounds");
            ok = false;
        }

        if (planetSettings.noiseLayers != null && planetSettings.noiseLayers.Count > 3)
        {
            Debug.LogError("Amount of noise layers have exceeded 3. Due to potential significant slow down. " +
                "It won't be allowed to run. However, if you would like more layers, change the limit in Planet2D.cs, method CheckSettings()");
            ok = false;
        }
        if (planetSettings.noiseLayers != null)
        {
            for (int i = 0; i < planetSettings.noiseLayers.Count; i++)
            {
                if (!planetSettings.noiseLayers[i].enabled)
                {
                    Debug.LogWarning("Noise Layer " + i + " is currently disabled. Enable it to see the changes");
                }
                if (planetSettings.noiseLayers[i].maskedBy == i)
                {
                    Debug.LogError("Noise Layer " + i + ": Self masking is prohibited where MaskedBy value is equal to Noise Layers element index. " +
                        "Masking can be disabled by putting negative integer for MaskedBy value");
                    ok = false;
                }
                if (1 > planetSettings.noiseLayers[i].noiseSettings.octaves || 5 < planetSettings.noiseLayers[i].noiseSettings.octaves)
                {
                    Debug.LogError("Noise Layer " + i + ": number of octaves out of bounds");
                    ok = false;
                }
                if (0 > planetSettings.noiseLayers[i].noiseSettings.frequency || 10 < planetSettings.noiseLayers[i].noiseSettings.frequency)
                {
                    Debug.LogError("Noise Layer " + i + ": frequency out of bounds");
                    ok = false;
                }
                if (1 > planetSettings.noiseLayers[i].noiseSettings.lacunarity || 5 < planetSettings.noiseLayers[i].noiseSettings.lacunarity)
                {
                    Debug.LogError("Noise Layer " + i + ": lacunarity out of bounds");
                    ok = false;
                }
                if (0 > planetSettings.noiseLayers[i].noiseSettings.amplitude || 0.5f < planetSettings.noiseLayers[i].noiseSettings.amplitude)
                {
                    Debug.LogError("Noise Layer " + i + ": amplitude out of bounds");
                    ok = false;
                }
                if (0 > planetSettings.noiseLayers[i].noiseSettings.persistence || 1 < planetSettings.noiseLayers[i].noiseSettings.persistence)
                {
                    Debug.LogError("Noise Layer " + i + ": persistence out of bounds");
                    ok = false;
                }
            }
        }
        if (planetSettings.waterDepth > 300)
        {
            Debug.LogError("Depth of water is capped at 300. If you require higher depth of water, " +
                "change the limit in Planet2D.cs, method CheckSettings()");
            ok = false;
        }
        return ok;
    }
}