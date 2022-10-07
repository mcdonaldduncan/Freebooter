using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetRotation : MonoBehaviour
{
    // Rotation values for each axis
    float rotateX;
    float rotateY;
    float rotateZ;

    // Randomize rotation values
    void Start()
    {
        rotateX = Random.Range(-10f, 10f);
        rotateY = Random.Range(-10f, 10f);
        rotateZ = Random.Range(-10f, 10f);
    }

    // Rotate planets based off randomized rotation values
    void Update()
    {
        transform.Rotate(rotateX * Time.deltaTime, rotateY * Time.deltaTime, rotateZ * Time.deltaTime);
    }
}
