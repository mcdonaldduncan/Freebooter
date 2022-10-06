using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeOrbit : MonoBehaviour, IDamageable
{
    [SerializeField] float StartingHealth;
    [SerializeField] Transform target;
    [SerializeField] float rotationSpeed;
    //[SerializeField] bool direction;

    float radius;
    float angle;

    float startY;

    Inquisitor _Inquisitor;
    Transform _Transform;
    Follower _Follower;

    public float Health { get; set; }

    // Set starting angle and radius
    void Start()
    {
        _Transform = transform;
        Health = StartingHealth;
        _Inquisitor = GetComponentInParent<Inquisitor>();
        radius = Vector3.Distance(_Transform.position, target.position);
        angle = Mathf.Deg2Rad * Random.Range(0f, 360f);
        startY = transform.localPosition.y;
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
        float z = radius * Mathf.Sin(angle);

        Vector3 polarVector = new Vector3(x, startY, z);

        _Transform.position = target.position + polarVector;
    }

    public void Damage(float damageTaken)
    {
        Health -= damageTaken;
        Debug.Log("Orbit Damaged");
        CheckForDeath();
    }

    public void CheckForDeath()
    {
        if (Health <= 0)
        {
            Debug.Log("Orbit Destroyed");
            _Inquisitor.orbits.Remove(this);
            Destroy(gameObject);
        }
    }
}
