using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.UIElements;
using System.Linq;

/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald
[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class Projectile : MonoBehaviour, IPoolable
{
    [SerializeField] GameObject m_Prefab;
    [SerializeField] GameObject m_ExplosionPrefab;
    [SerializeField] bool m_IsShootable;
    [SerializeField] bool m_DamageAll;
    [SerializeField] bool m_EnableGravity;
    [SerializeField] bool m_IsTracking;
    [SerializeField] bool m_IsExplosive;
    [SerializeField] bool m_IsLobbed;
    [SerializeField] bool m_HasTrail;
    [SerializeField] float m_LaunchForce;
    [SerializeField] float m_LaunchAngle;
    [SerializeField] float m_TrackingForce;
    [SerializeField] float m_DamageAmount;
    [SerializeField] float m_ExplosionRadius;
    [SerializeField] float m_VelocityLimit;
    [SerializeField] float m_LifeTime = 15f;

    Transform m_Transform;
    Transform m_Target;

    Vector3 velocity;
    Vector3 acceleration;

    TrailRenderer trailRenderer;
    public bool EnableGravity { get { return m_EnableGravity; } set { m_EnableGravity = value; } }
    public bool IsTracking { get { return m_IsTracking; } set { m_IsTracking = value; } }
    public bool IsExplosive { get { return m_IsExplosive; } set { m_IsExplosive = value; } }
    public bool IsLobbed { get { return m_IsLobbed; } set { m_IsLobbed = value; } }

    public GameObject Prefab { get { return m_Prefab; } set { m_Prefab = value; } }

    Rigidbody m_RigidBody;

    float startTime;

    bool hasCollided;

    private void OnEnable()
    {
        if (m_Transform == null || m_Transform != transform)
        {
            m_Transform = transform;
        }

        if (m_RigidBody == null)
        {
            m_RigidBody = GetComponent<Rigidbody>();
        }

        if (m_Target == null)
        {
            m_Target = LevelManager.Instance.Player.transform;
        }

        if (m_HasTrail && trailRenderer == null)
        {
            trailRenderer = GetComponentInChildren<TrailRenderer>();
        }

        if (m_HasTrail) trailRenderer.Clear();

        LevelManager.Instance.PlayerRespawn += ResetProjectile;

        hasCollided = false;

        startTime = Time.time;

        
        m_RigidBody.useGravity = m_EnableGravity;
        m_RigidBody.velocity = Vector3.zero;
        m_RigidBody.angularVelocity = Vector3.zero;

        velocity = Vector3.zero;
        acceleration = Vector3.zero;
    }

    private void Start()
    {
        if (m_Target == null)
        {
            m_Target = LevelManager.Instance.Player.transform;
        }
    }

    private void OnDisable()
    {
        if (LevelManager.Instance == null) return;
        LevelManager.Instance.PlayerRespawn -= ResetProjectile;
        m_RigidBody.velocity = Vector3.zero;
        m_RigidBody.angularVelocity = Vector3.zero;

    }

    private void Update()
    {
        if (Time.time > startTime + m_LifeTime)
        {
            if (m_IsExplosive) TriggerExplosion();
            ProjectileManager.Instance.ReturnToPool(gameObject);
        }
    }

    private void FixedUpdate()
    {
        if (m_IsTracking) TrackTarget();
    }

    /// <summary>
    /// Launch projectile in given direction
    /// </summary>
    /// <param name="direction">direction to launch, direction is normalized before application</param>
    public void Launch(Vector3 direction)
    {
        if (m_EnableGravity && m_IsLobbed)
        {
            Vector3 targetPos = m_Target.position;

            float gravity = Physics.gravity.magnitude;
            float angle = m_LaunchAngle * Mathf.Deg2Rad;

            Vector3 planarTarget = new Vector3(targetPos.x, 0, targetPos.z);
            Vector3 planarPostion = new Vector3(transform.position.x, 0, transform.position.z);

            float distance = Vector3.Distance(planarTarget, planarPostion);
            float yOffset = transform.position.y - targetPos.y;

            // Original equation https://physics.stackexchange.com/questions/27992/solving-for-initial-velocity-required-to-launch-a-projectile-to-a-given-destinat?rq=1
            float initialVelocity = (1 / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * gravity * Mathf.Pow(distance, 2)) / (distance * Mathf.Tan(angle) + yOffset));

            Vector3 velocity = new Vector3(0, initialVelocity * Mathf.Sin(angle), initialVelocity * Mathf.Cos(angle));

            float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPostion);
            if (targetPos.x < transform.position.x) angleBetweenObjects *= -1;
            Vector3 finalVelocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;

            m_RigidBody.useGravity = true;
            m_RigidBody.velocity = finalVelocity;
        }
        else
        {
            transform.LookAt(transform.position + direction);
            m_RigidBody.AddForce(m_LaunchForce * direction.normalized);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasCollided) return;
        //if (collision.gameObject == null) return;

        if (m_IsExplosive)
        {
            TriggerExplosion();
        }
        else if (m_DamageAll || collision.gameObject.CompareTag("Player"))
        {
            if (collision.gameObject.TryGetComponent(out IDamageable temp)) 
                temp.TakeDamage(m_DamageAmount, HitBoxType.normal);

            //IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
            //if (damageable != null)
            //{
            //    damageable.TakeDamage(m_DamageAmount);
            //}
        }

        hasCollided = true;
        ResetProjectile();
    }
    
    public void ProjectileHit()
    {
        if (!m_IsShootable) return;

        if (m_IsExplosive)
        {
            TriggerExplosion();
        }
        ResetProjectile();
    }

    void ResetProjectile()
    {
        m_RigidBody.velocity = Vector3.zero;
        m_RigidBody.angularVelocity = Vector3.zero;
        transform.position = Vector3.zero;
        ProjectileManager.Instance.ReturnToPool(gameObject);
    }

    void TriggerExplosion()
    {
        var hits = Physics.OverlapSphere(transform.position, m_ExplosionRadius);

        if (!m_DamageAll) hits = hits.Where(x => x.CompareTag("Player")).ToArray();
        else
        {
            List<GameObject> found = new List<GameObject>();
            for (int i = 0; i < hits.Length; i++)
            {
                if (!found.Contains(hits[i].gameObject)) found.Add(hits[i].gameObject);
                else hits[i] = null;
            }
        }

        foreach (var hit in hits)
        {
            if (hit == null) continue;
            if (hit.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(m_DamageAmount, HitBoxType.normal);
            }
        }

        ProjectileManager.Instance.TakeFromPool(m_ExplosionPrefab, transform.position);
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
        if (m_RigidBody.velocity.magnitude > m_VelocityLimit) m_RigidBody.AddForce(-m_RigidBody.velocity.normalized * (m_RigidBody.velocity.magnitude - m_VelocityLimit), ForceMode.Impulse);
        m_RigidBody.AddForce((m_Target.position - transform.position).normalized * m_TrackingForce, ForceMode.Impulse);
        m_Transform.LookAt(transform.position + m_RigidBody.velocity);

        //acceleration += CalculateSteering(m_Target.position);
        //velocity += acceleration;
        //m_Transform.position += velocity * Time.deltaTime;
        //m_Transform.LookAt(transform.position + velocity);
        //acceleration = Vector3.zero;
    }

}
