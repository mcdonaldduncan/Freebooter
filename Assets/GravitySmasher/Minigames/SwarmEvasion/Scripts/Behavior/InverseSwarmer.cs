using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InverseSwarmer : MonoBehaviour
{
    [SerializeField] float maxSpeed;
    [SerializeField] float maxForce;
    [SerializeField] float minForce;
    [SerializeField] float baseStrength;

    Transform target;

    Vector2 velocity;
    Vector2 acceleration;

    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        ApplySteering();   
    }

    void ApplySteering()
    {
        acceleration += CalculateForce();
        velocity += acceleration;
        transform.position += (Vector3)velocity * Time.deltaTime;
        acceleration = Vector2.zero;
    }

    Vector2 CalculateForce()
    {
        Vector2 desired = target.position - transform.position;
        desired.Normalize();
        desired *= maxSpeed;

        Vector2 steer = desired - velocity;
        steer.Normalize();
        steer *= maxForce;

        return steer;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            EndManager.instance.gameOver = true;
        }
    }

}
