using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelPieceCollisionBehavior : MonoBehaviour
{
    private void OnEnable()
    {
        Physics.IgnoreLayerCollision(8,14);
    }
}
