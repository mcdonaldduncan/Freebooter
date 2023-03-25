using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class MeleeTank : NewAgentBase
{
    [Header("Shield GameObject")]
    public GameObject Shield; //for cycle

    [Header("Animator")]
    [SerializeField] Animator m_Animator;

    //[Header("ExplosiveGun")]
    //[SerializeField] protected GameObject m_ProjectilePrefabExplosive;
    //[SerializeField] protected Transform m_ShootFromTopGun;

    //[Header("Shooting Options for ExplosiveGun")] 
    //[SerializeField] protected float m_RangeExplosive;
    //[SerializeField] float m_TimeBetweenShotsExplosive;

    [Header("Melee parameters")]
    [SerializeField] Transform m_raycastSource;
    [SerializeField] float m_meleeRange;
    [SerializeField] float m_meleeDamage;
    [SerializeField] float m_animationSpeed;
    [SerializeField] float m_TimeBetweenMeleeHits;

    bool bashOnce;
    RaycastHit hitInfo;

    [Header("Charge parameters")]
    [SerializeField] float m_chargeRange;
    [SerializeField] float m_chargeHitRange;
    [SerializeField] float m_TimeBetweenCharges;
    [SerializeField] float m_TimeBetweenChargeHits;
    [SerializeField] float m_ChargeDamage;
    [SerializeField] float m_VelocityLimit;
    [SerializeField] float m_ChargeLifeTime = 5f;
    [SerializeField] float m_ChargeStoppingDistance = 1f;

    //possibly add a force variable for push back in the future

    protected float lastShotTimeExplosive; //made new timer for the explosive gun to shoot slower then normal guns
    //protected bool shouldShootExplosive => Time.time > m_TimeBetweenShotsExplosive + lastShotTimeExplosive;

    float lastChargeTime;
    float lastMeleeTime;
    float lastChargeHitTime;
    bool shouldMelee => Time.time > m_TimeBetweenMeleeHits + lastMeleeTime && m_Tracking.DistanceToTarget < m_meleeRange;//timer for the melee
    bool shouldCharge => Time.time > m_TimeBetweenCharges + lastChargeTime && m_Tracking.DistanceToTarget < m_chargeRange;//timer for the charge
    bool shouldDealDamageInCharge => Time.time > m_TimeBetweenChargeHits + lastChargeHitTime;
    bool charging; //Im using this to prevent melee atacking while charging

    bool resetChargeParam = false;
    float originalchargetimer;
    float originalSpeed;
    float originalAccel;

    private void Awake()
    {
        AwakeSetup();
    }

    private void OnEnable()
    {
        EnableSetup();
    }

    private void Start()
    {
        Agent = gameObject.GetComponent<NavMeshAgent>();
        originalchargetimer = m_ChargeLifeTime;
        originalAccel = Agent.acceleration;
        originalSpeed = Agent.speed;
    }


    //public override void Shoot()
    //{
    //    //shoots two lasers at a time
    //    if (!shouldShoot || distanceToPlayer > m_Range) return; // change logic to stop shooting when out of range because the aim method doesnt update the direction when out of range so they would shoot forward without aiming


    //    //laser 1
    //    GameObject obj = ProjectileManager.Instance.TakeFromPool(m_ProjectilePrefab, m_ShootFrom.position, out Projectile projectile);
    //    projectile.Launch(m_TargetDirection);
    //    projectile.transform.LookAt(projectile.transform.position + m_TargetDirection);

    //    //laser 2
    //    GameObject obj2 = ProjectileManager.Instance.TakeFromPool(m_ProjectilePrefab, m_ShootFrom2.position, out Projectile projectile2);
    //    projectile2.Launch(m_TargetDirection);
    //    projectile2.transform.LookAt(projectile2.transform.position + m_TargetDirection);

    //    altShoootFrom = !altShoootFrom;
    //    lastShotTime = Time.time;
    //}

    //public void ShootExplosiveGun()
    //{
    //    //shoots rockets on a different timer
    //    if (!shouldShootExplosive || distanceToPlayer > m_RangeExplosive) return; // change logic to stop shooting when out of range because the aim method doesnt update the direction when out of range so they would shoot forward without aiming

    //    GameObject obj = ProjectileManager.Instance.TakeFromPool(m_ProjectilePrefabExplosive, m_ShootFromTopGun.position, out Projectile projectile);
    //    projectile.Launch(m_TargetDirection);
    //    projectile.transform.LookAt(projectile.transform.position + m_TargetDirection);

    //    altShoootFrom = !altShoootFrom;
    //    lastShotTimeExplosive = Time.time;
    //}

    private void Update()
    {
        m_Animator.SetFloat("Blend", Agent.velocity.magnitude);
        HandleAgentState();
    }

    public override void HandleAgentState()
    {
        switch (m_State)
        {
            case AgentState.GUARD:
                m_Tracking.TrackTarget2D();
                if (m_Tracking.CheckFieldOfView()) m_State = AgentState.CHASE;
                if (IsInCombat) HandleCombatStateChange();
                break;
            case AgentState.WANDER:
                m_Navigation.Wander();
                if (m_Tracking.CheckFieldOfView()) m_State = AgentState.CHASE;
                if (IsInCombat) HandleCombatStateChange();
                break;
            case AgentState.CHASE:
                m_Navigation.ChaseTarget();
                m_Tracking.TrackTarget2D();
                if (!m_Tracking.InRange) m_State = AgentState.RETURN;
                if (!IsInCombat) HandleCombatStateChange();
                if (shouldMelee) { MeleeHandler(); }
                if (shouldCharge) { ChargeAttack(); }
                break;
            case AgentState.RETURN:
                m_Navigation.MoveToLocationDirect(m_StartingPosition);
                if (m_Navigation.CheckReturned(m_StartingPosition)) m_State = m_StartingState;
                if (m_Tracking.CheckFieldOfView()) m_State = AgentState.CHASE;
                if (IsInCombat) HandleCombatStateChange();
                break;
            case AgentState.SLEEP:
                m_Navigation.Sleep();
                if (IsInCombat) HandleCombatStateChange();
                break;
            default:
                break;
        }
    }

    private void MeleeHandler()
    {
        if (Shield != null)
        {
            if (Shield.gameObject.activeSelf == false) { return; }
        }
        if (charging) { return; }
        if (bashOnce != true)
        {
            bashOnce = true;
            m_Animator.SetTrigger("Bash");
            Invoke("RestBashParam", m_TimeBetweenMeleeHits);
        }
    }

    void RestBashParam() { bashOnce = false; }

    public void MeleeAttack()
    {
        //shoot raycast in a short range when in melee range
        //do melee atk
        if (Physics.Raycast(m_raycastSource.position, gameObject.transform.forward, out hitInfo, m_meleeRange))
        {
            if (hitInfo.transform.CompareTag("Player"))
            {
                GiveDamage(m_meleeDamage);
            }
        }
        lastMeleeTime = Time.time;
    }

    private void GiveDamage(float damageToDeal)
    {
        LevelManager.Instance.Player.TakeDamage(damageToDeal, HitBoxType.normal);
    }

    private void ChargeAttack()
    {
        if (!shouldCharge) { return; }
        charging = true;
        Agent.speed = m_VelocityLimit / 2;
        Agent.acceleration = m_VelocityLimit;
        Agent.stoppingDistance = m_ChargeStoppingDistance;

        //m_Agent.SetDestination(m_Target.transform.position);
        Vector3 FromPlayerToAgent = transform.position - LevelManager.Instance.Player.transform.position;
        Agent.SetDestination(LevelManager.Instance.Player.transform.position + FromPlayerToAgent.normalized * Agent.stoppingDistance);
        m_Animator.SetBool("Charge", true);

        //if (m_RigidBody.velocity.magnitude > m_VelocityLimit) m_RigidBody.AddForce(-m_RigidBody.velocity.normalized * (m_RigidBody.velocity.magnitude - m_VelocityLimit), ForceMode.Impulse);
        //m_RigidBody.AddForce((m_Target.position - transform.position) * m_TrackingForce, ForceMode.Impulse);
        //transform.LookAt(m_Target.transform.position);

        ChargingRayCast();

        m_ChargeLifeTime -= Time.deltaTime;
        if (resetChargeParam == false && m_ChargeLifeTime < 0) { ChangeChargingToFalse(); }
        resetChargeParam = false;
    }

    void ChargingRayCast()
    {
        if (shouldDealDamageInCharge)
        {
            if (Physics.Raycast(m_raycastSource.position, gameObject.transform.forward, out hitInfo, m_chargeHitRange))
            {
                if (hitInfo.transform.CompareTag("Player"))
                {
                    GiveDamage(m_ChargeDamage);
                }
            }
            lastChargeHitTime = Time.time;
        }
    }

    void ChangeChargingToFalse()
    {
        resetChargeParam = true;
        charging = !charging;
        lastChargeTime = Time.time;
        Agent.speed = originalSpeed;
        Agent.acceleration = originalAccel;
        m_ChargeLifeTime = originalchargetimer;
        Agent.ResetPath();
        m_Animator.SetBool("Charge", false);
    }

    public override void CheckForDeath()
    {
        if (Health <= 0)
        {
            IsDead = true;
            m_Animator.SetBool("Death", true);
        }
    }

    public void DeathAnimationEnd()
    {
        OnDeath();
        m_Animator.SetBool("Death", false);
    }
}
