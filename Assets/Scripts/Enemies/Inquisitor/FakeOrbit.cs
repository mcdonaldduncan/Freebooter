using Assets.Scripts.Enemies.Agent_Base.Interfaces;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald
public class FakeOrbit : MonoBehaviour, IDamageable, IGroupable
{
    [SerializeField] float StartingHealth;
    [SerializeField] Transform target;
    [SerializeField] float rotationSpeed;
    //[SerializeField] bool direction;

    [Header("Damage Display Options")]
    [SerializeField] GameObject m_DamageTextPrefab;
    [SerializeField] Transform m_TextSpawnLocation;
    [SerializeField] float m_FontSize;
    [SerializeField] bool m_ShowDamageNumbers;

    public GameObject DamageTextPrefab => m_DamageTextPrefab;

    public Transform TextSpawnLocation => m_TextSpawnLocation;

    public float FontSize => m_FontSize;

    float radius;
    float angle;

    float startY;

    Inquisitor _Inquisitor;
    Transform _Transform;
    Follower _Follower;

    public float Health { get; set; }


    public bool IsDead { get; set; }

    IDamageable damageable;

    // Set starting angle and radius
    void Start()
    {
        _Transform = transform;
        Health = StartingHealth;
        _Inquisitor = GetComponentInParent<Inquisitor>();
        radius = Random.Range(5f, 7f);
        angle = Mathf.Deg2Rad * Random.Range(0f, 360f);
        startY = transform.position.y - target.position.y;
        damageable = this;
    }

    private void OnEnable()
    {
        IsDead = false;
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
        damageable.InstantiateDamageNumber(damageTaken, hitbox);
        CheckForDeath();
    }

    public void CheckForDeath()
    {
        if (Health <= 0)
        {
            //Debug.Log("Orbit Destroyed");
            //_Inquisitor.orbits.Remove(this);
            IsDead = true;
            gameObject.SetActive(false);
            _Inquisitor.CheckOrbits();
            
        }
    }
}
