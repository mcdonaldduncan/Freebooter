using System.Collections;
using System.Collections.Generic;
//using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public abstract class NewAgentBase : MonoBehaviour, IDamageable, INavigation, ITracking, IShooting, IRespawn
{
    [Header("Projectile Prefab and Projectile Spawn Point")]
    [SerializeField] GameObject m_ProjectilePrefab;
    [SerializeField] Transform m_ShootFrom;
    [SerializeField] GameObject m_OnKillHealFVX;

    [Header("Damage Display Options")]
    [SerializeField] GameObject m_DamageTextPrefab;
    [SerializeField] Transform m_TextSpawnLocation;
    [SerializeField] float m_FontSize;
    [SerializeField] bool m_ShowDamageNumbers;

    [Header("Layer mask Options")]
    [SerializeField] LayerMask m_WalkableLayers;
    [SerializeField] LayerMask m_PlayerLayer;

    [Header("Movement Options")]
    [SerializeField] float m_StoppingDistance;
    [SerializeField] float m_RotationSpeed;
    [SerializeField] float m_MovementSampleRadius;

    [Header("Wander Options")]
    [SerializeField] float m_WanderDelay;
    [SerializeField] float m_WanderDistance;

    [Header("Shooting Options")]
    [SerializeField] float m_Range;
    [SerializeField] float m_TimeBetweenShots;

    [Header("Tracking Options")]
    [SerializeField] float m_FieldOfView;

    [Header("Health Options")]
    [SerializeField] float m_MaxHealth;

    [Header("Activation Options")]
    [SerializeField] bool m_ShouldSleep;
    [SerializeField] GameObject m_Activator;

    AgentState m_State;
    AgentState m_StartingState;

    INavigation m_Navigation;
    ITracking m_Tracking;
    IShooting m_Shooting;
    IRespawn m_Respawn;
    IDamageable m_Damageable;

    Vector3 m_StartingPosition;
    Quaternion m_StartingRotation;

    protected bool IsDead;

    public float Health { get; set; }
    
    public NavMeshAgent Agent { get; set; }

    public LayerMask WalkableLayers => m_WalkableLayers;

    public float StoppingDistance => m_StoppingDistance;

    public float RotationSpeed => m_RotationSpeed;

    public float MovementSampleRadius => m_MovementSampleRadius;

    public float WanderDelay => m_WanderDelay;

    public float WanderDistance => m_WanderDistance;

    public float LastWanderTime { get; set; }

    public Transform TrackingTransform => transform;

    public Vector3 RayPoint => m_ShootFrom.position;

    public LayerMask TargetLayer => m_PlayerLayer;

    public float Range => m_Range;

    public float FOV => m_FieldOfView;

    public GameObject ProjectilePrefab => m_ProjectilePrefab;

    public Vector3 ShootFrom => m_ShootFrom.position;

    public float TimeBetweenShots => m_TimeBetweenShots;

    public float LastShotTime { get; set; }
    public Vector3 StartingPosition { get; set; }
    public bool ShouldSleep { get; set; }
    public IActivator Activator { get; set; }

    public GameObject DamageTextPrefab => m_DamageTextPrefab;

    public Transform TextSpawnLocation => m_TextSpawnLocation;

    public TextMeshPro Text { get; set; }

    public float FontSize => m_FontSize;

    public bool ShowDamageNumbers => m_ShowDamageNumbers;

    protected void AwakeSetup()
    {
        Agent = GetComponent<NavMeshAgent>();

        m_Navigation = this;
        m_Tracking = this;
        m_Shooting = this;
        m_Respawn = this;
        m_Damageable = this;
    }

    protected void EnableSetup()
    {
        Health = m_MaxHealth;
        m_StartingPosition = transform.position;
        m_StartingRotation = transform.rotation;

        m_State = m_ShouldSleep ? AgentState.SLEEP : AgentState.WANDER;
        m_StartingState = m_State;

        m_Damageable.SetupDamageText();
    }

    protected void StartSetup()
    {
        m_Respawn.SubscribeToRespawn();
    }

    public void HandleAgentState()
    {
        switch (m_State)
        {
            case AgentState.GUARD:
                m_Tracking.TrackTarget2D();
                if (m_Tracking.CheckFieldOfView()) m_State = AgentState.CHASE;
                break;
            case AgentState.WANDER:
                m_Navigation.Wander();
                if (m_Tracking.CheckFieldOfView()) m_State = AgentState.CHASE;
                break;
            case AgentState.CHASE:
                m_Navigation.ChaseTarget();
                m_Tracking.TrackTarget2D();
                if (m_Tracking.CheckLineOfSight()) m_Shooting.Shoot();
                if (!m_Tracking.InRange) m_State = AgentState.RETURN;
                break;
            case AgentState.RETURN:
                m_Navigation.MoveToLocationDirect(m_StartingPosition);
                if (m_Navigation.CheckReturned(m_StartingPosition)) m_State = m_StartingState;
                if (m_Tracking.CheckFieldOfView()) m_State = AgentState.CHASE;
                break;
            case AgentState.SLEEP:
                m_Navigation.Sleep();
                break;
            default:
                break;
        }
    }

    public void CheckForDeath()
    {
        
        if (Health <= 0)
        {
            IsDead = true;
            OnDeath();
        }
    }

    public void TakeDamage(float damageTaken)
    {
        Debug.Log($"{gameObject.name} took damage");
        m_State = AgentState.CHASE;
        Health -= damageTaken;
        m_Damageable.GenerateDamageInfo(damageTaken, HitBoxType.normal);
        CheckForDeath();
        Debug.Log(Health);
    }

    public virtual void OnDeath()
    {
        if (m_Tracking.DistanceToTarget <= LevelManager.Instance.Player.DistanceToHeal)
        {
            ProjectileManager.Instance.TakeFromPool(m_OnKillHealFVX, transform.position);
            LevelManager.Instance.Player.Health += (LevelManager.Instance.Player.PercentToHeal * m_MaxHealth);
        }
        gameObject.SetActive(false);
        m_Respawn.SubscribeToCheckpointReached();
    }

    void ResetValues()
    {
        m_Navigation.CycleAgent(m_StartingPosition);
        IsDead = false;
        transform.rotation = m_StartingRotation;
        Health = m_MaxHealth;
    }

    public virtual void OnPlayerRespawn()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        ResetValues();
    }

}
