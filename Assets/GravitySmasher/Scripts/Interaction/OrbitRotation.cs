using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitRotation : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float rotationSpeed;

    float radius;
    float angle;

    // Set starting angle and radius
    void Start()
    {
        radius = Vector3.Distance(transform.position, target.position);
        angle = Mathf.Deg2Rad * Vector3.Angle(target.position, transform.position);
    }

    void Update()
    {
        Orbit();
    }

    // Orbit object using polar coordinates
    void Orbit()
    {
        angle += rotationSpeed * Time.deltaTime;

        float x = radius * Mathf.Cos(angle);
        float y = radius * Mathf.Sin(angle);

        Vector3 polarVector = new Vector3(x, y, 0);

        transform.position = target.position + polarVector;
    }
}
