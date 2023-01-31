using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.UIElements;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class Projectile : MonoBehaviour, IPoolable
{
    [SerializeField] GameObject m_Prefab;
    [SerializeField] GameObject m_ExplosionPrefab;
    [SerializeField] bool m_EnableGravity;
    [SerializeField] bool m_IsTracking;
    [SerializeField] bool m_IsExplosive;
    [SerializeField] float m_LaunchForce;
    [SerializeField] float m_TrackingForce;
    [SerializeField] float m_DamageAmount;
    [SerializeField] float m_ExplosionRadius;
    [SerializeField] float m_LifeTime = 15f;

    Transform m_Transform;
    Transform m_Target;

    Vector3 velocity;
    Vector3 acceleration;

    public bool EnableGravity { get { return m_EnableGravity; } set { m_EnableGravity = value; } }
    public bool IsTracking { get { return m_IsTracking; } set { m_IsTracking = value; } }
    public bool IsExplosive { get { return m_IsExplosive; } set { m_IsExplosive = value; } }

    public GameObject Prefab { get { return m_Prefab; } set { m_Prefab = value; } }

    Rigidbody m_Rigidbody;

    float startTime;
    

    bool hasCollided;

    private void OnEnable()
    {
        if (m_Rigidbody == null)
        {
            m_Rigidbody = GetComponent<Rigidbody>();
        }

        hasCollided = false;

        startTime = Time.time;

        m_Target = LevelManager.Instance.Player.transform;
        m_Transform = transform;
        m_Rigidbody.useGravity = m_EnableGravity;
        m_Rigidbody.velocity = Vector3.zero;
        m_Rigidbody.angularVelocity = Vector3.zero;

        velocity = Vector3.zero;
        acceleration = Vector3.zero;
    }

    private void OnDisable()
    {
        m_Rigidbody.velocity = Vector3.zero;
        m_Rigidbody.angularVelocity = Vector3.zero;
    }

    private void Update()
    {
        if (Time.time > startTime + m_LifeTime) ProjectileManager.Instance.ReturnToPool(gameObject);

        if (m_IsTracking) TrackTarget();

    }

    /// <summary>
    /// Launch projectile in given direction
    /// </summary>
    /// <param name="direction">direction to launch, direction is normalized before application</param>
    public void Launch(Vector3 direction)
    {
        transform.LookAt(transform.position + direction);
        m_Rigidbody.AddForce(m_LaunchForce * direction.normalized);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasCollided) return;

        if (m_IsExplosive)
        {
            var hits = Physics.OverlapSphere(transform.position, m_ExplosionRadius);

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(m_DamageAmount);
                }
            }

            ProjectileManager.Instance.TakeFromPool(m_ExplosionPrefab, transform.position);
        }
        else
        {
            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(m_DamageAmount);
            }
        }

        hasCollided = true;
        m_Rigidbody.velocity = Vector3.zero;
        m_Rigidbody.angularVelocity = Vector3.zero;
        transform.position = Vector3.zero;
        ProjectileManager.Instance.ReturnToPool(gameObject);
    }

    Vector3 CalculateSteering(Vector3 currentTarget)
    {
        Vector3 desired = currentTarget - m_Transform.position;
        Vector3 steer = desired - velocity;
        steer = steer.normalized;
        steer *= m_TrackingForce;
        return steer;
    }

    void TrackTarget()
    {
        acceleration += CalculateSteering(m_Target.position);
        velocity += acceleration;
        m_Transform.position += velocity * Time.deltaTime;
        m_Transform.LookAt(transform.position + velocity);
        acceleration = Vector3.zero;
    }

}
