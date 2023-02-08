using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelGroupBehavior : MonoBehaviour
{
    public delegate void BarrelGroupDelegate();
    public event BarrelGroupDelegate fractureChildren;
    [HideInInspector]
    public bool activated = false;

    public void FractureChildren()
    {
        activated = true;
        fractureChildren?.Invoke();
    }
}
