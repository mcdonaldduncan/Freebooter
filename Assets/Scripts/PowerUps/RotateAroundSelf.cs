using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAroundSelf : MonoBehaviour
{
    [SerializeField] private float speed = 10;
    void FixedUpdate()
    {
        this.transform.RotateAround(this.transform.position, Vector3.up, speed * Time.deltaTime);
    }
}
