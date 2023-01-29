using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class AgentBase : MonoBehaviour, IDamageable, IEnemy
{
    [SerializeField] GameObject m_ProjectilePrefab;
    [SerializeField] LayerMask m_WalkableLayers;
    [SerializeField] GameObject m_ShootFrom;
    [SerializeField] GameObject m_VisionPoint;
    [SerializeField] GameObject m_Body;
    [SerializeField] GameObject m_Weapon;
    [SerializeField] Transform m_Hand;

    [SerializeField] float m_StoppingDistance;

    [SerializeField] float m_RotationSpeed;
    [SerializeField] float m_Range;

    [SerializeField] float m_WanderDelay;
    [SerializeField] float m_WanderDistance;

    [SerializeField] float m_TimeBetweenShots;

    [SerializeField] float m_MaxHealth;
    [SerializeField] float m_Health;

    NavMeshAgent m_Agent;
    Transform m_Target;

    AgentState m_State;
    AgentState m_StartingState;

    Vector3 m_TargetDirection;
    Vector3 m_StartingPosition;

    Quaternion m_DesiredRotation;
    Quaternion m_StartingRotation;

    float lastShotTime;
    float lastWanderTime;
    float distanceToPlayer;

    public float Health { get => m_Health; set => m_Health = value; }
    public Vector3 StartingPosition { get => m_StartingPosition; set => m_StartingPosition = value; }
    bool shouldShoot => Time.time > m_TimeBetweenShots + lastShotTime;
    bool shouldWander => Time.time > m_WanderDelay + lastWanderTime && m_Agent.pathStatus == NavMeshPathStatus.PathComplete;

    private void Start()
    {
        m_Health = m_MaxHealth;
        m_StartingState = m_State;
        m_StartingPosition = transform.position;
        m_StartingRotation = transform.rotation;
        m_Target = LevelManager.Instance.Player.transform;

        LevelManager.PlayerRespawn += OnPlayerRespawn;
    }

    private void Update()
    {
        distanceToPlayer = Vector3.Distance(transform.position, m_Target.position);

        switch (m_State)
        {
            case AgentState.GUARD:
                Aim();
                CheckLineOfSight();
                break;
            case AgentState.WANDER:
                Wander();
                break;
            case AgentState.CHASE:
                Aim();
                Shoot();
                ChasePlayer();
                break;
            case AgentState.RETURN:
                ReturnToOrigin();
                break;
            default:
                break;
        }
    }

    void Aim()
    {
        float tempSpeed = m_RotationSpeed;
        if (distanceToPlayer < m_Range)
        {
            m_TargetDirection = m_Target.position - m_ShootFrom.transform.position;
            m_DesiredRotation = Quaternion.LookRotation(m_TargetDirection.normalized);
            transform.rotation = Quaternion.RotateTowards(new Quaternion(0, transform.rotation.y, 0, 360), m_DesiredRotation, tempSpeed * Time.deltaTime * 180);
        }
    }

    void CheckLineOfSight()
    {
        Physics.Raycast(m_VisionPoint.transform.position, m_TargetDirection, out RaycastHit hit, m_Range);

        if (hit.collider == null) return;

        if (!hit.collider.CompareTag("Player")) return;

        m_State = AgentState.CHASE;
    }

    void Shoot()
    {
        if (!shouldShoot) return;

        GameObject newObj = ProjectileManager.Instance.TakeFromPool(m_ProjectilePrefab, m_ShootFrom.transform.position, out Projectile projectile);
        projectile.Launch(m_TargetDirection);

        lastShotTime = Time.time;
    }

    void Wander()
    {
        if (!shouldWander) return;

        m_Agent.SetDestination(RandomPosInSphere(transform.position, m_WanderDistance, m_WalkableLayers));
    }

    Vector3 RandomPosInSphere(Vector3 origin, float distance, LayerMask layerMask)
    {
        Vector3 randomDirection = Random.insideUnitSphere * distance;
        NavMesh.SamplePosition(randomDirection + origin, out NavMeshHit navHit, distance, layerMask);
        return navHit.position;
    }

    void ChasePlayer()
    {
        if (distanceToPlayer < m_Range && distanceToPlayer > m_Range / 2)
        {
            Vector3 FromPlayerToAgent = transform.position - m_Target.position ;

            m_Agent.SetDestination(m_Target.position + FromPlayerToAgent.normalized * m_StoppingDistance);
        }
    }

    void ReturnToOrigin()
    {
        m_Agent.SetDestination(m_StartingPosition);

        if (Vector3.Distance(transform.position, m_StartingPosition) < 1f)
        {
            transform.rotation = m_StartingRotation;
            m_State = m_StartingState;
        }
    }

    public void CheckForDeath()
    {
        if (m_Health <= 0)
        {
            OnDeath();
        }
    }

    public void OnCheckPointReached()
    {
        LevelManager.PlayerRespawn -= OnPlayerRespawn;
    }

    public void OnDeath()
    {
        if (distanceToPlayer <= LevelManager.Instance.Player.DistanceToHeal)
        {
            LevelManager.Instance.Player.Health += (LevelManager.Instance.Player.PercentToHeal * m_MaxHealth);
        }
        m_Agent.Warp(m_StartingPosition);
        gameObject.SetActive(false);
        LevelManager.CheckPointReached += OnCheckPointReached;
    }

    public void OnPlayerRespawn()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        m_Agent.Warp(m_StartingPosition);
        m_Health = m_MaxHealth;
    }

    public void TakeDamage(float damageTaken)
    {
        if (m_State != AgentState.CHASE)
        {
            m_State = AgentState.CHASE;
        }
        m_Health -= damageTaken;
        CheckForDeath();
    }
}

public enum AgentState
{
    GUARD,
    WANDER,
    CHASE,
    RETURN

}