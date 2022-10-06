using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealOrbit : MonoBehaviour
{
    [SerializeField] Transform locus;
    [SerializeField] float repulsionMultiplier;
    [SerializeField] float attractionMultiplier;
    [SerializeField] float communalMultiplier;
    [SerializeField] float maxForce;

    Vector3 velocity;

    [SerializeField] int targetIndex;

    ArrayManager arrayManager;

    void Start()
    {
        arrayManager = GetComponentInParent<ArrayManager>();
        FindIndex();
    }

    void Update()
    {
        ApplyOrbit();
    }

    void FindIndex()
    {
        for (int i = 0; i < arrayManager.array.Length; i++)
        {
            if (!(arrayManager.array[i] == transform))
                continue;
            targetIndex = i + 1;
            if (targetIndex >= arrayManager.array.Length)
            {
                targetIndex = 0;
            }
        }
    }

    Vector3 AttractionForce(Transform attractor)
    {
        Vector3 force = attractor.position - transform.position;
        float distance = force.magnitude;
        force.Normalize();
        force *= distance;
        return force * attractionMultiplier;
    }

    Vector3 RepulsionForce(Transform repulsor)
    {
        Vector3 force = transform.position - repulsor.position;
        float distance = force.magnitude;
        force.Normalize();
        force *= (1 / distance);
        return force * repulsionMultiplier;
    }

    Vector3 CommunalForce(Transform target)
    {
        Vector3 force = target.position - transform.position;
        force.Normalize();
        return force * communalMultiplier;
    }

    void ApplyOrbit()
    {
        Vector3 aForce = AttractionForce(locus);
        Vector3 rForce = RepulsionForce(arrayManager.array[targetIndex]);
        Vector3 cForce = CommunalForce(arrayManager.array[targetIndex]);
        Vector3 force = aForce + rForce + cForce - velocity;
        force = force.normalized * maxForce;
        //Vector3 steer = force - velocity;
        velocity += force;
        transform.position += velocity * Time.deltaTime;
    }
}
