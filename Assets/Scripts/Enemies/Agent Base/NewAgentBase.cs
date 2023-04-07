using Assets.Scripts.Enemies.Agent_Base.Interfaces;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public abstract class NewAgentBase : MonoBehaviour, IDamageable, INavigation, ITracking, IShooting, IRespawn, IGroupable
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
    [SerializeField] LayerMask m_SightLayers;

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

    [Header("OnDeath Options")]
    [SerializeField] bool m_ShouldHitStop;
    [SerializeField] float m_HitStopDuration;

    [Header("Activation Options")]
    [SerializeField] bool m_ShouldSleep;
    //[SerializeField] GameObject m_Activator;

    protected AgentState State;
    protected AgentState StartingState;

    protected INavigation Navigation;
    protected ITracking Tracking;
    protected IShooting Shooting;
    protected IRespawn Respawn;
    protected IDamageable Damageable;

    protected Vector3 StartingPosition;
    protected Quaternion StartingRotation;

    public bool IsDead { get; set; }

    public float Health { get; set; }
    
    public NavMeshAgent Agent { get; set; }


    public float StoppingDistance => m_StoppingDistance;

    public float RotationSpeed => m_RotationSpeed;

    public float MovementSampleRadius => m_MovementSampleRadius;

    public float WanderDelay => m_WanderDelay;

    public float WanderDistance => m_WanderDistance;

    public float LastWanderTime { get; set; }

    public virtual Transform TrackingTransform => transform;

    public Transform RayPoint => m_ShootFrom;

    public LayerMask SightLayers => m_SightLayers;

    public float Range => m_Range;

    public float FOV => m_FieldOfView;

    public GameObject ProjectilePrefab => m_ProjectilePrefab;

    public Transform ShootFrom => m_ShootFrom;

    public float TimeBetweenShots => m_TimeBetweenShots;

    public float LastShotTime { get; set; }

    public GameObject DamageTextPrefab => m_DamageTextPrefab;

    public Transform TextSpawnLocation => m_TextSpawnLocation;

    public TextMeshPro Text { get; set; }

    public float FontSize => m_FontSize;

    protected bool IsInCombat { get; set; }

    public bool AltShootFrom { get; set; }

    public delegate void CombatStateEventHandler(bool combatState);
    public event CombatStateEventHandler CombatStateChanged;

    protected void AwakeSetup()
    {
        Agent = GetComponent<NavMeshAgent>();

        Navigation = this;
        Tracking = this;
        Shooting = this;
        Respawn = this;
        Damageable = this;
    }

    protected void EnableSetup()
    {
        Health = m_MaxHealth;
        StartingPosition = transform.position;
        StartingRotation = transform.rotation;

        State = m_ShouldSleep ? AgentState.SLEEP : AgentState.WANDER;
        StartingState = State;

        Damageable.SetupDamageText();
    }

    protected void StartSetup()
    {
        Respawn.SubscribeToRespawn();
    }

    public virtual void HandleAgentState()
    {
        switch (State)
        {
            case AgentState.GUARD:
                Tracking.TrackTarget();
                if (Tracking.CheckFieldOfView()) State = AgentState.CHASE;
                if (IsInCombat) HandleCombatStateChange();
                break;
            case AgentState.WANDER:
                Navigation.Wander();
                if (Tracking.CheckFieldOfView()) State = AgentState.CHASE;
                if (IsInCombat) HandleCombatStateChange();
                break;
            case AgentState.CHASE:
                Navigation.ChaseTarget();
                Tracking.TrackTarget();
                if (Tracking.CheckLineOfSight()) Shooting.Shoot();
                else if (Tracking.InRange) State = AgentState.GUARD;
                else State = AgentState.RETURN;
                if (!IsInCombat) HandleCombatStateChange();
                break;
            case AgentState.RETURN:
                Navigation.MoveToLocationDirect(StartingPosition);
                if (Navigation.CheckReturned(StartingPosition)) State = StartingState;
                if (Tracking.CheckFieldOfView()) State = AgentState.CHASE;
                if (IsInCombat) HandleCombatStateChange();
                break;
            case AgentState.SLEEP:
                Navigation.Sleep();
                if (IsInCombat) HandleCombatStateChange();
                break;
            default:
                break;
        }
    }

    public void HandleCombatStateChange()
    {
        IsInCombat = !IsInCombat;
        CombatStateChanged?.Invoke(IsInCombat);
    }

    public virtual void CheckForDeath()
    {
        if (Health <= 0)
        {
            IsDead = true;
            OnDeath();
        }
    }

    public virtual void TakeDamage(float damageTaken, HitBoxType hitbox, Vector3 hitPoint = default(Vector3))
    {
        //Debug.Log($"{gameObject.name} took damage");
        State = AgentState.CHASE;
        Health -= damageTaken;
        Damageable.InstantiateDamageNumber(damageTaken, hitbox);
        CheckForDeath();
        //Debug.Log(Health);
    }

    public virtual void OnDeath()
    {
        if (m_ShouldHitStop) LevelManager.TimeStop(m_HitStopDuration);

        if (Tracking.DistanceToTarget <= LevelManager.Instance.Player.DistanceToHeal)
        {
            ProjectileManager.Instance.TakeFromPool(m_OnKillHealFVX, transform.position);
            LevelManager.Instance.Player.Health += (LevelManager.Instance.Player.PercentToHeal * m_MaxHealth);
        }

        if (IsInCombat) CombatStateChanged?.Invoke(false);

        gameObject.SetActive(false);
        Respawn.SubscribeToCheckpointReached();
    }

    void ResetValues()
    {
        Navigation.CycleAgent(StartingPosition);
        IsDead = false;
        IsInCombat = false;
        transform.rotation = StartingRotation;
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
