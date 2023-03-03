using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using TMPro;

public abstract class AgentBase : MonoBehaviour, IDamageable, IEnemy
{
    [Header("Projectile Prefab and Projectile Spawn Point")]
    [SerializeField] GameObject m_ProjectilePrefab;
    [SerializeField] protected Transform m_ShootFrom;

    [Header("Walkable Layers")]
    [SerializeField] LayerMask m_WalkableLayers;

    [Header("Movement Options")]
    [SerializeField] float m_StoppingDistance;
    [SerializeField] float m_RotationSpeed;
    [SerializeField] float m_SampleRadius;

    [Header("Wander Options")]
    [SerializeField] float m_WanderDelay;
    [SerializeField] float m_WanderDistance;

    [Header("Shooting Options")]
    [SerializeField] protected float m_Range;
    [SerializeField] float m_TimeBetweenShots;

    [Header("Health Options")]
    [SerializeField] float m_MaxHealth;
    [SerializeField] protected float m_Health;
    [SerializeField] private GameObject m_OnKillHealFVX;

    [Header("Activation Options")]
    [SerializeField] bool m_ShouldSleep;
    [SerializeField] GameObject m_Activator;

    [Header("DamagePopUp")]
    [SerializeField] GameObject m_DamagePopUpPrefab;
    [SerializeField] Transform m_PopupFromHere;
    float m_fontSize = 5;

    protected NavMeshAgent m_Agent;
    protected Transform m_Target;
    protected Transform m_Transform;

    protected AgentState m_State;
    AgentState m_StartingState;

    protected Vector3 m_TargetDirection;
    protected Vector3 m_StartingPosition;

    Quaternion m_DesiredRotation;
    Quaternion m_StartingRotation;

    [NonSerialized] protected float distanceToPlayer;
    protected float lastShotTime;
    float lastWanderTime;

    [NonSerialized] protected bool isDead;
    [NonSerialized] protected bool altShoootFrom;

    public float Health { get => m_Health; set => m_Health = value; }
    protected bool shouldShoot => Time.time > m_TimeBetweenShots + lastShotTime;
    bool shouldWander => Time.time > m_WanderDelay + lastWanderTime && m_Agent.pathStatus == NavMeshPathStatus.PathComplete;

    public Vector3 StartingPosition { get => m_StartingPosition; set => m_StartingPosition = value; }
    public bool ShouldSleep { get => m_ShouldSleep; set => m_ShouldSleep = value; }
    public IActivator Activator { get; set; }
    public float MovementSampleRadius { get => m_SampleRadius; }

    public virtual void HandleSetup()
    {
        m_Transform = transform;
        m_Health = m_MaxHealth;
        m_StartingPosition = transform.position;
        m_StartingRotation = transform.rotation;
        m_Target = LevelManager.Instance.Player.transform;
        m_Agent = GetComponent<NavMeshAgent>();

        m_State = m_ShouldSleep ? AgentState.SLEEP : AgentState.WANDER;
        m_StartingState = m_State;

        LevelManager.PlayerRespawn += OnPlayerRespawn;

        if (m_Activator == null) return;

        try
        {
            // Get the IActivator component from the activator object
            Activator = (IActivator)m_Activator.GetComponent(typeof(IActivator));
            Activator.Activate += OnActivate;
            Activator.Deactivate += OnDeactivate;
        }
        catch (Exception)
        {
            Debug.LogError("Valid IActivator Not Found");
        }

    }

    public virtual void HandleAgentState()
    {
        distanceToPlayer = Vector3.Distance(transform.position, m_Target.position);

        switch (m_State)
        {
            case AgentState.SLEEP:
                Sleep();
                break;
            case AgentState.GUARD:
                AimRestricted();
                if (CheckLineOfSight()) m_State = AgentState.CHASE;
                break;
            case AgentState.WANDER:
                Wander();
                AimRestricted();
                if (CheckLineOfSight()) m_State = AgentState.CHASE;
                break;
            case AgentState.CHASE:
                AimRestricted();
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
        if (distanceToPlayer < m_Range)
        {
            m_Transform.LookAt(m_Target.position);

            m_TargetDirection = m_Target.position - m_ShootFrom.position;
            //m_DesiredRotation = Quaternion.LookRotation(m_TargetDirection.normalized);
            //transform.rotation = Quaternion.RotateTowards(transform.rotation, m_DesiredRotation, m_RotationSpeed * Time.deltaTime * 180);
        }
    }

    public void AimRestricted()
    {
        if (distanceToPlayer < m_Range)
        {
            m_Transform.LookAt(new Vector3(m_Target.position.x, transform.position.y, m_Target.position.z));

            m_TargetDirection = m_Target.position - m_ShootFrom.position;
            //m_DesiredRotation = Quaternion.LookRotation(m_TargetDirection.normalized);
            //transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(transform.rotation.x, m_DesiredRotation.y, transform.rotation.x), m_RotationSpeed * Time.deltaTime * 180);
        }
    }

    public bool CheckLineOfSight()
    {
        Physics.Raycast(m_ShootFrom.position, m_TargetDirection, out RaycastHit hit, m_Range);

        if (hit.collider == null) return false;

        if (!hit.collider.CompareTag("Player")) return false;

        return true;
    }

    public virtual void Shoot()
    {
        if (!shouldShoot || distanceToPlayer > m_Range) return;

        GameObject newObj = ProjectileManager.Instance.TakeFromPool(m_ProjectilePrefab, m_ShootFrom.position, out Projectile projectile);
        projectile.Launch(m_TargetDirection);
        projectile.transform.LookAt(projectile.transform.position + m_TargetDirection);

        altShoootFrom = !altShoootFrom;
        lastShotTime = Time.time;
    }

    public void Shoot(Transform shootFrom)
    {
        if (!shouldShoot) return;
        Vector3 tempDirection = m_Target.position - shootFrom.position;

        GameObject newObj = ProjectileManager.Instance.TakeFromPool(m_ProjectilePrefab, shootFrom.position, out Projectile projectile);
        projectile.Launch(tempDirection);
        projectile.transform.LookAt(projectile.transform.position + tempDirection);

        altShoootFrom = !altShoootFrom;
        lastShotTime = Time.time;
    }

    public bool CheckRange()
    {
        return distanceToPlayer < m_Range;
    }

    public void Wander()
    {
        if (!shouldWander) return;

        m_Agent.SetDestination(RandomPosInSphere(transform.position, m_WanderDistance, m_WalkableLayers));

        lastWanderTime = Time.time;
    }

    Vector3 RandomPosInSphere(Vector3 origin, float distance, LayerMask layerMask)
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * distance;
        NavMesh.SamplePosition(randomDirection + origin, out NavMeshHit navHit, distance, layerMask);
        return navHit.position;
    }

    public virtual void ChasePlayer()
    {
        if (distanceToPlayer < m_Range && distanceToPlayer > m_Range / 2)
        {
            Vector3 FromPlayerToAgent = transform.position - m_Target.position;


            MoveToLocation(m_Target.position + FromPlayerToAgent.normalized * m_StoppingDistance);
        }
    }

    public void ReturnToOrigin()
    {
        if (m_Agent != null) m_Agent.SetDestination(m_StartingPosition);

        if (Vector3.Distance(transform.position, m_StartingPosition) < 1f)
        {
            transform.rotation = m_StartingRotation;
            m_State = m_StartingState;
        }
    }

    protected virtual void CycleAgent()
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

        transform.rotation = m_StartingRotation;
        transform.position = m_StartingPosition;
        m_Health = m_MaxHealth;
        m_State = m_ShouldSleep ? AgentState.SLEEP : m_StartingState;
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

    public virtual void OnDeath()
    {
        CameraShake.ShakeCamera();
        if (distanceToPlayer <= LevelManager.Instance.Player.DistanceToHeal)
        {
            ProjectileManager.Instance.TakeFromPool(m_OnKillHealFVX, transform.position);
            LevelManager.Instance.Player.HealthRegen(LevelManager.Instance.Player.PercentToHeal * m_MaxHealth);
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

    //public virtual void TakeDamage(float damageTaken)
    //{
    //    if (m_State != AgentState.CHASE)
    //    {
    //        m_State = AgentState.CHASE;
    //    }
    //    m_Health -= damageTaken;
    //    DamageNumbers(damageTaken);
    //    CheckForDeath();
    //}

    public virtual void TakeDamage(float damageTaken, HitBoxType? hitType = null)
    {
        if (m_State != AgentState.CHASE)
        {
            m_State = AgentState.CHASE;
        }
        m_Health -= damageTaken;
        DamageNumbers(damageTaken, hitType);
        CheckForDeath();
    }

    public virtual void DamageNumbers(float DamageNumber)
    { //if not a special hitbox use this one
        var txtpro = m_DamagePopUpPrefab.GetComponent<TextMeshPro>();
        ResetDamageNumberValuers();
        txtpro.color = Color.gray;
        txtpro.text = DamageNumber.ToString("0");
        InstantiateDamageNumber();
    }

    public virtual void DamageNumbers(float DamageNumber, HitBoxType? hitType)
    {//if special hitbox use this one
        var txtpro = m_DamagePopUpPrefab.GetComponent<TextMeshPro>();
        ResetDamageNumberValuers();
        if (hitType != null)
        {
            switch (hitType)
            {
                case HitBoxType.critical:
                    txtpro.color = Color.red;
                    txtpro.fontSize = m_fontSize*2;
                    break;
                case HitBoxType.armored:
                    txtpro.color = Color.blue;
                    break;
                case HitBoxType.shield:
                    //for now we dont have any shielded enemies.
                    //TODO : make the shields also show damage numbers
                    txtpro.color = Color.blue;
                    break;
            }
        }
        else if (hitType == null)
        {
            txtpro.color = Color.gray;
        }
        txtpro.text = DamageNumber.ToString("0");
        InstantiateDamageNumber();
    }

    void InstantiateDamageNumber()
    {
       ProjectileManager.Instance.TakeFromPool(m_DamagePopUpPrefab, new Vector3(m_PopupFromHere.transform.position.x + UnityEngine.Random.Range(-1f,1f), m_PopupFromHere.transform.position.y, m_PopupFromHere.transform.position.z + UnityEngine.Random.Range(-1f,1f)));
    }

    void ResetDamageNumberValuers() 
    {
        var txtpro = m_DamagePopUpPrefab.GetComponent<TextMeshPro>();
        txtpro.color = Color.gray;
        txtpro.fontSize = m_fontSize;
    }


    void Sleep()
    {
        if (m_Agent.isStopped) return;
        m_Agent.ResetPath();
        m_Agent.isStopped = true;
    }

    public void MoveToLocation(Transform location)
    {
        if (NavMesh.SamplePosition(location.position, out NavMeshHit hit, MovementSampleRadius, NavMesh.AllAreas))
        {
            m_Agent.SetDestination(hit.position);

            Debug.Log("Destination set to " + hit.position);
        }
        else
        {
            //Debug.Log($"The agent attached to {gameObject.name} was directed to a location with no navmesh");
        }

    }

    public void MoveToLocation(Vector3 location)
    {
        if (NavMesh.SamplePosition(location, out NavMeshHit hit, MovementSampleRadius, NavMesh.AllAreas))
        {
            m_Agent.SetDestination(hit.position);
            //Debug.Log("Overload Destination set to " + hit.position);
        }
        else
        {
            //Debug.Log($"The agent attached to {gameObject.name} was directed to a location with no navmesh");
        }

    }

    public void OnActivate()
    {
        ActivateAggro();
    }

    public void OnDeactivate()
    {
        DeactivateAggro();
    }

    public void ActivateAggro()
    {
        if (!m_Agent.isActiveAndEnabled) return;

        m_State = AgentState.CHASE;
        m_Agent.isStopped = false;
    }

    public void DeactivateAggro()
    {
        m_State = AgentState.SLEEP;
    }

}

public enum AgentState
{
    
    GUARD,
    WANDER,
    CHASE,
    RETURN,
    SLEEP

}

