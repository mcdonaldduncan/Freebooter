using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class MeleeTank : NewAgentBase, IDissolvable
{
    [Header("Shield GameObject")]
    public GameObject Shield; //for cycle

    [Header("Animator")]
    [SerializeField] Animator m_Animator;
    [SerializeField] Rigidbody m_torsoRB;
    [SerializeField] GameObject m_ragdollParent;
    [SerializeField] private float ragdollForce;
    [SerializeField] private float ragdollForceScale;

    //[Header("ExplosiveGun")]
    //[SerializeField] protected GameObject m_ProjectilePrefabExplosive;
    //[SerializeField] protected Transform m_ShootFromTopGun;

    //[Header("Shooting Options for ExplosiveGun")] 
    //[SerializeField] protected float m_RangeExplosive;
    //[SerializeField] float m_TimeBetweenShotsExplosive;

    [Header("Melee parameters")]
    [SerializeField] Transform m_raycastSource;
    [SerializeField] Transform m_lobSource;
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

    [Header("Custom Behaviour")]
    [SerializeField] bool shouldReturn = true;

    //possibly add a force variable for push back in the future

    protected float lastShotTimeExplosive; //made new timer for the explosive gun to shoot slower then normal guns
    //protected bool shouldShootExplosive => Time.time > m_TimeBetweenShotsExplosive + lastShotTimeExplosive;

    float lastChargeTime;
    float lastMeleeTime;
    float lastChargeHitTime;
    bool shouldMelee => Time.time > m_TimeBetweenMeleeHits + lastMeleeTime && Tracking.DistanceToTarget < m_meleeRange;//timer for the melee
    bool shouldShoot => Time.time > m_TimeBetweenMeleeHits + lastMeleeTime && !charging;//timer for the ranged
    bool shouldCharge => Time.time > m_TimeBetweenCharges + lastChargeTime && Tracking.DistanceToTarget < m_chargeRange;//timer for the charge
    bool shouldDealDamageInCharge => Time.time > m_TimeBetweenChargeHits + lastChargeHitTime;
    bool charging; //Im using this to prevent melee atacking while charging
    bool shooting;

    bool resetChargeParam = false;
    float originalchargetimer;
    float originalSpeed;
    float originalAccel;

    private Dictionary<Transform, Vector3[]> m_ragdollLimbStartingVectors;

    public DissolvableDelegate EnemyDied { get; set; }
    //public delegate void MetalonDelegate();
    //public event MetalonDelegate MetalonDeath;

    private void Awake()
    {
        AwakeSetup();
        m_ragdollLimbStartingVectors = new Dictionary<Transform, Vector3[]>();
    }

    private void OnEnable()
    {
        EnableSetup();
        Shooting.AltShootFrom = m_lobSource.transform;
    }

    private void Start()
    {
        StartSetup();
        Agent = gameObject.GetComponent<NavMeshAgent>();
        originalchargetimer = m_ChargeLifeTime;
        originalAccel = Agent.acceleration;
        originalSpeed = Agent.speed;
        GetRagdollLimbs();
        DisableRagdoll();
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
    private void GetRagdollLimbs()
    {
        Transform[] ragdollLimbs = m_ragdollParent.GetComponentsInChildren<Transform>();

        foreach (var limb in ragdollLimbs)
        {
            m_ragdollLimbStartingVectors.Add(limb, new Vector3[] { limb.transform.position, limb.transform.eulerAngles, limb.transform.localScale });
        }
    }

    private void ResetLimbs()
    {
        foreach (var kvPair in m_ragdollLimbStartingVectors)
        {
            kvPair.Key.position = kvPair.Value[0];
            kvPair.Key.rotation = Quaternion.Euler(kvPair.Value[1]);
            kvPair.Key.localScale = kvPair.Value[2];
        }
    }

    public override void HandleAgentState()
    {
        if (IsDead) return;

        switch (State)
        {
            case AgentState.GUARD:
                Tracking.TrackTarget2D();
                if (Tracking.CheckFieldOfView()) State = AgentState.CHASE;
                if (IsInCombat) HandleCombatStateChange();
                break;
            case AgentState.WANDER:
                Navigation.Wander();
                if (Tracking.CheckFieldOfView()) State = AgentState.CHASE;
                if (IsInCombat) HandleCombatStateChange();
                break;
            case AgentState.CHASE:
                if (NavmeshOnPlayer()) { Navigation.ChaseTarget(); }
                Tracking.TrackTarget2D();
                if (!Tracking.InRange && shouldReturn == true) State = AgentState.RETURN;
                if (!IsInCombat) HandleCombatStateChange();
                if (shouldMelee && NavmeshOnPlayer()) { AttackHandler(true); }
                if (shouldShoot && !NavmeshOnPlayer()) { AttackHandler(false); }
                if (shouldCharge) { ChargeAttack(); }
                if (charging) {  }
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

    bool NavmeshOnPlayer()
    {
        var pos = LevelManager.Instance.Player.transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(pos, out hit, 1.2f, NavMesh.AllAreas))
        {
            return true;
        }
        else { return false; }
    }

    public void AttackEnded()
    {
        ResetBashParam();
        ResetLobParam();
    }

    private void AttackHandler(bool attacktype) // true  = melee, false = ranged
    {
        if (Shield != null)
        {
            if (Shield.gameObject.activeSelf == false) { return; }
        }
        if (charging) { return; }
        if (bashOnce != true && attacktype == true)
        {
            bashOnce = true;
            m_Animator.SetBool("Bash", true);
        }
        if (bashOnce != true && attacktype == false)
        {
            bashOnce = true;
            m_Animator.SetBool("Lob", true);
        }
    }

    public void ResetBashParam() { bashOnce = false; m_Animator.SetBool("Bash", false); }
    public void ResetLobParam() { bashOnce = false; m_Animator.SetBool("Lob", false); }

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

    public void LobAttack()
    {
        Shooting.Shoot(m_lobSource);
    }

    private void GiveDamage(float damageToDeal)
    {
        LevelManager.Instance.Player.TakeDamage(damageToDeal, HitBoxType.normal);
    }

    private void ChargeAttack()
    {
        if (!shouldCharge  && !shooting) { return; }
        charging = true;
        Agent.speed = m_VelocityLimit / 2;
        Agent.acceleration = m_VelocityLimit;
        Agent.stoppingDistance = m_ChargeStoppingDistance;

        m_ChargeLifeTime -= Time.deltaTime;
        
        Navigation.ChaseTargetDirect();
        
        m_Animator.SetBool("Charge", true);

        ChargingRayCast();
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

    public override void TakeDamage(float damageTaken, HitBoxType hitbox, Vector3 hitPoint = default(Vector3))
    {
        State = AgentState.CHASE;
        Health -= damageTaken;
        Damageable.InstantiateDamageNumber(damageTaken, hitbox);
        if(Health <= 0)
        {
            IsDead = true;
            OnDeathRagdoll(hitPoint);
        }
    }

    public override void CheckForDeath()
    {
        if (Health <= 0)
        {
            IsDead = true;
            m_Animator.SetBool("Death", true);
        }
    }

    private void OnDeathRagdoll(Vector3 hitPoint)
    {
        EnableRagdoll(hitPoint);
        base.OnDeath();
        gameObject.SetActive(true);
        EnemyDied?.Invoke();
    }

    public override void OnPlayerRespawn()
    {
        IsDead = false;
        DisableRagdoll();
        base.OnPlayerRespawn();
    }

    private void DisableRagdoll()
    {
        ResetLimbs();
        Agent.speed = originalSpeed;
        m_Animator.enabled = true;
        BoxCollider[] boxColliders = GetComponentsInChildren<BoxCollider>();
        Rigidbody[] rigidBones = GetComponentsInChildren<Rigidbody>();
        
        foreach (Collider c in boxColliders)
        {
            c.enabled = true;
        }

        foreach (Rigidbody r in rigidBones)
        {
            r.isKinematic = true;
        }
    }

    private void EnableRagdoll(Vector3 hitPoint)
    {
        Agent.speed = 0;
        m_Animator.enabled = false;
        BoxCollider[] boxColliders = GetComponentsInChildren<BoxCollider>();
        Rigidbody[] rigidBones = GetComponentsInChildren<Rigidbody>();
        foreach(BoxCollider c in boxColliders)
        {
            c.enabled = false;
        }

        foreach(Rigidbody r in rigidBones)
        {
            r.isKinematic = false;
        }
        Vector3 forceDirection = m_torsoRB.position - hitPoint;

        m_torsoRB.AddForce(forceDirection.normalized * ragdollForce * ragdollForceScale, ForceMode.Impulse);
    }

    public void DeathAnimationEnd()
    {
        OnDeath();
        m_Animator.SetBool("Death", false);
    }
}
