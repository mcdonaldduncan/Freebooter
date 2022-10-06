using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Unused because of the increased requirement to write data constantly back to struct
public struct AttractionData
{
    public Vector3 position;
    public float mass;

    public AttractionData(Vector3 _position, float _mass)
    {
        position = _position;
        mass = _mass;
    }
    
}
