using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald
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

    public GameObject DamageTextPrefab => throw new System.NotImplementedException();

    public Transform TextSpawnLocation => throw new System.NotImplementedException();

    public float FontSize => throw new System.NotImplementedException();

    public bool ShowDamageNumbers => throw new System.NotImplementedException();

    public TextMeshPro Text { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    // Set starting angle and radius
    void Start()
    {
        _Transform = transform;
        Health = StartingHealth;
        _Inquisitor = GetComponentInParent<Inquisitor>();
        radius = Random.Range(5f, 7f);
        angle = Mathf.Deg2Rad * Random.Range(0f, 360f);
        startY = transform.position.y - target.position.y;
    }

    void Update()
    {
        Orbit();
    }

    // Orbit object using polar coordinates
    void Orbit()
    {
        angle += rotationSpeed * Time.deltaTime;
        //transform.RotateAround(new Vector3(target.position.x, startY, target.position.z), Vector3.up, angle);


        float x = radius * Mathf.Cos(angle);
        float z = radius * Mathf.Sin(angle);

        Vector3 polarVector = new Vector3(x, startY, z);

        _Transform.position = target.position + polarVector;
    }

    public void TakeDamage(float damageTaken, HitBoxType hitbox, Vector3 hitPoint = default(Vector3))
    {
        Health -= damageTaken;
        //Debug.Log("Orbit Damaged");
        CheckForDeath();
    }

    public void CheckForDeath()
    {
        if (Health <= 0)
        {
            //Debug.Log("Orbit Destroyed");
            //_Inquisitor.orbits.Remove(this);
            gameObject.SetActive(false);
            _Inquisitor.CheckOrbits();
            
        }
    }
}
