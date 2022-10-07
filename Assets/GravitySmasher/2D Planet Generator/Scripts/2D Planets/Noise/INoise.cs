using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INoise
{
    float GetNoise(int x, int y);

    Dictionary<XYCoordinate, float> GetNoiseDictionary();
}
