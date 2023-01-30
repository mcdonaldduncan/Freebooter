using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class AgentBase : MonoBehaviour, IDamageable, IEnemy
{
    [Header("Projectile Prefab and Projectile Spawn Point")]
    [SerializeField] GameObject m_ProjectilePrefab;
    [SerializeField] Transform m_ShootFrom;

    [Header("Walkable Layers")]
    [SerializeField] LayerMask m_WalkableLayers;

    [Header("Movement Options")]
    [SerializeField] float m_StoppingDistance;
    [SerializeField] float m_RotationSpeed;

    [Header("Wander Options")]
    [SerializeField] float m_WanderDelay;
    [SerializeField] float m_WanderDistance;

    [Header("Shooting Options")]
    [SerializeField] float m_Range;
    [SerializeField] float m_TimeBetweenShots;

    [Header("Health Options")]
    [SerializeField] float m_MaxHealth;
    [SerializeField] float m_Health;

    [NonSerialized] public NavMeshAgent m_Agent;
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

    [NonSerialized] public bool isDead;

    public float Health { get => m_Health; set => m_Health = value; }
    public Vector3 StartingPosition { get => m_StartingPosition; set => m_StartingPosition = value; }
    bool shouldShoot => Time.time > m_TimeBetweenShots + lastShotTime;
    bool shouldWander => Time.time > m_WanderDelay + lastWanderTime && m_Agent.pathStatus == NavMeshPathStatus.PathComplete;


    public virtual void HandleSetup()
    {
        m_Health = m_MaxHealth;
        m_StartingState = m_State;
        m_StartingPosition = transform.position;
        m_StartingRotation = transform.rotation;
        m_Target = LevelManager.Instance.Player.transform;
        m_Agent = GetComponent<NavMeshAgent>();

        LevelManager.PlayerRespawn += OnPlayerRespawn;
    }

    public virtual void HandleAgentState()
    {
        distanceToPlayer = Vector3.Distance(transform.position, m_Target.position);

        switch (m_State)
        {
            case AgentState.GUARD:
                Aim();
                if (CheckLineOfSight()) m_State = AgentState.CHASE;
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

    public void Aim()
    {
        float tempSpeed = m_RotationSpeed;
        if (distanceToPlayer < m_Range)
        {
            m_TargetDirection = m_Target.position - m_ShootFrom.position;
            m_DesiredRotation = Quaternion.LookRotation(m_TargetDirection.normalized);
            transform.rotation = Quaternion.RotateTowards(new Quaternion(0, transform.rotation.y, 0, 360), m_DesiredRotation, tempSpeed * Time.deltaTime * 180);
        }
    }

    public bool CheckLineOfSight()
    {
        Physics.Raycast(m_ShootFrom.position, m_TargetDirection, out RaycastHit hit, m_Range);

        if (hit.collider == null) return false;

        if (!hit.collider.CompareTag("Player")) return false;

        return true;
    }

    public void Shoot()
    {
        if (!shouldShoot) return;

        GameObject newObj = ProjectileManager.Instance.TakeFromPool(m_ProjectilePrefab, m_ShootFrom.position, out Projectile projectile);
        projectile.Launch(m_TargetDirection);
        projectile.transform.LookAt(projectile.transform.position + m_TargetDirection);

        lastShotTime = Time.time;
    }

    public bool CheckRange()
    {
        return distanceToPlayer < m_Range;
    }

    void Wander()
    {
        if (!shouldWander) return;

        m_Agent.SetDestination(RandomPosInSphere(transform.position, m_WanderDistance, m_WalkableLayers));
    }

    Vector3 RandomPosInSphere(Vector3 origin, float distance, LayerMask layerMask)
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * distance;
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
        if (m_Agent != null) m_Agent.SetDestination(m_StartingPosition);

        if (Vector3.Distance(transform.position, m_StartingPosition) < 1f)
        {
            transform.rotation = m_StartingRotation;
            m_State = m_StartingState;
        }
    }

    void CycleAgent()
    {
        if (m_Agent == null) return;

        if (!m_Agent.isOnNavMesh)
        {
            m_Agent.enabled = false;
            m_Agent.enabled = true;
        }
        else
        {
            m_Agent.isStopped = true;
            m_Agent.isStopped = false;
        }
        m_Agent.Warp(m_StartingPosition);
    }

    public void Resetvalues()
    {
        CycleAgent();
        
        m_Health = m_MaxHealth;
        m_State = m_StartingState;
        isDead = false;
    }

    public void CheckForDeath()
    {
        if (m_Health <= 0)
        {
            isDead = true;
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
        gameObject.SetActive(false);
        LevelManager.CheckPointReached += OnCheckPointReached;
    }

    public virtual void OnPlayerRespawn()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        Resetvalues();
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

