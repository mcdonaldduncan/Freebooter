using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NewAgentBase : MonoBehaviour, IDamageable, INavigation, ITracking, IShooting
{
    [Header("Projectile Prefab and Projectile Spawn Point")]
    [SerializeField] protected GameObject m_ProjectilePrefab;
    [SerializeField] protected Transform m_ShootFrom;

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
    [SerializeField] float m_WanderSampleRadius;

    [Header("Shooting Options")]
    [SerializeField] protected float m_Range;
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

    Vector3 m_StartingPosition;
    Quaternion m_StartingRotation;
    

    public float Health { get; set; }
    
    public NavMeshAgent Agent { get; set; }

    public LayerMask WalkableLayers => m_WalkableLayers;

    public float StoppingDistance => m_StoppingDistance;

    public float RotationSpeed => m_RotationSpeed;

    public float MovementSampleRadius => m_MovementSampleRadius;

    public float WanderSampleRadius => m_WanderSampleRadius;

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

    void AwakeSetup()
    {
        Agent = GetComponent<NavMeshAgent>();

        m_Navigation = this;
        m_Tracking = this;
        m_Shooting = this;
    }

    void EnableSetup()
    {
        Health = m_MaxHealth;
        m_StartingPosition = transform.position;
        m_StartingRotation = transform.rotation;

        m_State = m_ShouldSleep ? AgentState.SLEEP : AgentState.WANDER;
        m_StartingState = m_State;
    }

    void HandleAgentState()
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
                m_Tracking.TrackTarget2D();
                m_Shooting.Shoot();
                m_Navigation.ChaseTarget();
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
            //isDead = true;
            OnDeath();
        }
    }

    public void TakeDamage(float damageTaken)
    {
        m_State = AgentState.CHASE;
        Health -= damageTaken;
        CheckForDeath();
    }

    void OnDeath()
    {
        if (m_Tracking.DistanceToTarget <= LevelManager.Instance.Player.DistanceToHeal)
        {
            LevelManager.Instance.Player.Health += (LevelManager.Instance.Player.PercentToHeal * m_MaxHealth);
        }
        gameObject.SetActive(false);
        //LevelManager.CheckPointReached += OnCheckPointReached;
    }

    void ResetValues()
    {
        m_Navigation.CycleAgent(m_StartingPosition);

        transform.rotation = m_StartingRotation;
        Health = m_MaxHealth;
    }
}
