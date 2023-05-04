using UnityEngine;
using UnityEngine.AI;
using TMPro;
using Assets.Scripts.Enemies.Agent_Base.Interfaces;
using Unity.VisualScripting;
using System;
using System.Collections.Generic;
using System.Collections;

public sealed class EnemySwarmerBehavior : MonoBehaviour, IDamageable, IGroupable, IDissolvable, IEnemy//, IShooting - LOBBING BEHAVIOR
{
    public float Health { get { return health; } set { health = value; } }
    [SerializeField] Rigidbody TorsoRigidBody;
    [SerializeField] GameObject m_ragdollParent;
    [SerializeField] private bool ignorePlayer;

    IDamageable m_IDamageable;

    [Header("Hide Properties")]
    [Tooltip("The swarmer will hide if not accompanied by this many other enemies (0 = never hide)")]
    [SerializeField] private float hideThreshold;

    [Header("Health and Damage")]
    [SerializeField] private float maxHealth = 75;
    [SerializeField] private float health = 75;
    [SerializeField] private float damageToDeal = 20;

    [Header("OnDeath Options")]
    [SerializeField] private bool m_shouldHitStop;
    [SerializeField] private float m_hitStopDuration;
    [SerializeField] private AudioClip m_onDeathSFX;

    [Header("Attacks and Movement")]
    [SerializeField] private float distanceToFollow = 20;
    [SerializeField] private float distanceToAttack = 2;
    [SerializeField] private float attackReach = 3;
    [SerializeField] private float attackRotateSpeed = 10;

    //[Header("Throwing")] - LOBBING BEHAVIOR
    //[SerializeField] private float distanceToThrow = 10;
    //[SerializeField] GameObject m_ProjectilePrefab;
    //[SerializeField] Transform m_ShootFrom;
    //[SerializeField] float m_TimeBetweenShots;

    [Header("DamagePopUp")]
    [SerializeField] GameObject m_DamagePopUpPrefab;
    [SerializeField] Transform m_PopupFromHere;
    [SerializeField] bool m_showDamageNumbers;
    float m_fontSize = 5;

    [Header("Sounds")]
    [SerializeField] AudioClip m_AttackSound;

    [Header("Misc")]
    [SerializeField] private Transform raycastSource;
    [Tooltip("The layer of colliders that will be considered when counting nearby enemies")]
    [SerializeField] private LayerMask enemies;
    [SerializeField] private GameObject m_OnKillHealFVX;

    //private FirstPersonController playerController;
    //private GameObject player;
    private Transform m_Target;
    private HideBehavior hideBehavior;
    private LineOfSightChecker checker;
    private NavMeshAgent navMeshAgent;
    private RaycastHit hitInfo;
    private Animator animator;
    private float mostRecentHit;
    private float distanceToPlayer;
    private float originalSpeed;
    [SerializeField] private float ragdollForce;
    [SerializeField] private float ragdollForceScale;
    private bool chasePlayer;
    private bool hideFromPLayer;
    private bool meleeAttackingPlayer;
    //private bool throwingAtPlayer; - LOBBING BEHAVIOR
    //private bool shouldThrow; - LOBBING BEHAVIOR
    private bool inAttackAnim;
    private bool m_updateAnims;
    private bool m_isDead;
    private Vector3 prevPos;

    private Rigidbody[] rigidBones;

    private Fracture fractureScript;

    private Coroutine processingRoutine;


    //protected IShooting Shooting; - LOBBING BEHAVIOR

    private bool isSwarm => Physics.OverlapSphereNonAlloc(transform.position, 15f, hits, enemies) >= hideThreshold;

    #region IShootable Properties - LOBBING BEHAVIOR
    //public GameObject ProjectilePrefab => m_ProjectilePrefab;
    //public Transform ShootFrom => m_ShootFrom;
    //public float TimeBetweenShots => m_TimeBetweenShots;
    //public float LastShotTime { get; set; }
    //public bool AltShootFrom { get; set; }
    #endregion

    public Vector3 StartingPosition { get { return m_StartingPosition; } set { m_StartingPosition = value; } }

    //public float MovementSampleRadius { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //public bool ShouldSleep { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //public IActivator Activator { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    private Vector3 m_StartingPosition;

    private Collider[] hits = new Collider[5];
    private Dictionary<Transform, Vector3[]> m_ragdollLimbStartingVectors;

    public GameObject DamageTextPrefab => m_DamagePopUpPrefab;
    public Transform TextSpawnLocation => m_PopupFromHere;
    public float FontSize => m_fontSize;
    public bool ShowDamageNumbers => m_showDamageNumbers;

    public TextMeshPro Text { get; set; }

    public bool IsDead { get; set; } = false;
    public bool IsInCombat { get; set; }

    public CombatStateEventHandler CombatStateChanged { get; set; }
    public CombatStateEventHandler EnemyDefeated { get; set; }
    public DissolvableDelegate EnemyDied { get; set; }
    //public delegate void DissolvableDelegate();
    //public event DissolvableDelegate EnemyDied;

    AudioSource m_AudioSource;

    bool defaultIgnorePlayer;

    int temp;

    private void Awake()
    {
        hideBehavior = GetComponent<HideBehavior>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        m_IDamageable = this;
        m_AudioSource = GetComponent<AudioSource>();
        rigidBones = gameObject.GetComponentsInChildren<Rigidbody>();
        m_ragdollLimbStartingVectors = new Dictionary<Transform, Vector3[]>();
        //Shooting = this; - LOBBING BEHAVIOR
    }

    private void Start()
    {
        m_Target = LevelManager.Instance.Player.transform;
        checker = GetComponentInChildren<LineOfSightChecker>();
        m_updateAnims = true;
        m_isDead = false;
        defaultIgnorePlayer = ignorePlayer;
        m_StartingPosition = transform.position;
        Health = maxHealth;
        chasePlayer = false;
        meleeAttackingPlayer = false;
        //throwingAtPlayer = false; - LOBBING BEHAVIOR
        animator.SetBool("PlayerTooFar", true);
        animator.SetBool("ChasePlayer", false);
        animator.SetBool("PunchPlayer", false);
        animator.SetBool("ThrowStuff", false);
        LevelManager.Instance.PlayerRespawn += OnPlayerRespawn;
        //hideBehavior.enabled = false;
        m_IDamageable.SetupDamageText();
        fractureScript = GetComponentInChildren<Fracture>();
        originalSpeed = navMeshAgent.speed;
        GetRagdollLimbs();
        DisableRagdoll();

        //prevPos = Vector3.zero;
    }

    private void Update()
    {
        distanceToPlayer = Vector3.Distance(transform.position, m_Target.position);

        if (IsDead) return;
        if (distanceToPlayer <= distanceToFollow)
        {
            navMeshAgent.isStopped = false;
            if (isSwarm)
            {
                if (navMeshAgent.isPathStale)
                {
                    navMeshAgent.ResetPath();
                }
                chasePlayer = true;
                foreach (var item in hits)
                {
                    if (item == null) continue;
                    if (item.TryGetComponent(out EnemySwarmerBehavior temp))
                    {
                        temp.chasePlayer = true;
                        temp.hideBehavior.enabled = false;
                    }
                }
                if (hideBehavior.enabled == true)
                {
                    hideBehavior.EndHideProcessRemote();
                    hideBehavior.enabled = false;
                }
            }
            else
            {
                chasePlayer = false;
                //FacePlayer();

                if (hideBehavior.enabled == false)
                {
                    if (navMeshAgent.isPathStale)
                    {
                        navMeshAgent.ResetPath();
                        //hideBehavior.StartHideProcessRemote(LevelManager.Instance.Player.transform);
                    }

                    //navMeshAgent.ResetPath();
                    hideBehavior.enabled = true;
                    hideBehavior.StartHideProcessRemote(m_Target);
                }
            }
        }
        

        if (!ignorePlayer && chasePlayer)
        {
            if (!meleeAttackingPlayer /* && !throwingAtPlayer && !shouldThrow - LOBBING BEHAVIOR*/)
            {
                if (navMeshAgent.isPathStale)
                {
                    navMeshAgent.ResetPath();
                }
                Vector3 targetDest = LevelManager.Instance.Player.transform.position;
                navMeshAgent.SetDestination(targetDest);
                //StartCoroutine(WaitForPathProcessing()); - LOBBING BEHAVIOR
                //prevPos = transform.position;
            }
            if (meleeAttackingPlayer /*|| throwingAtPlayer - LOBBING BEHAVIOR*/)
            {
                navMeshAgent.isStopped = true;
                if (!inAttackAnim)
                {
                    FacePlayer();
                }
            }
        }

        if (m_updateAnims) UpdateAnimations();
    }

    //IEnumerator WaitForPathProcessing() - LOBBING BEHAVIOR
    //{
    //    int counter = 0;
    //    while (navMeshAgent.pathPending)
    //    {
    //        counter++;
    //        yield return null;
    //        if (counter > 3)
    //        {
    //            break;
    //        }
    //    }

    //    if (navMeshAgent.pathStatus != NavMeshPathStatus.PathComplete && prevPos == transform.position && distanceToPlayer > distanceToAttack)
    //    {
    //        Vector3 prevPosition = transform.position;
    //        Debug.Log("shouldThrow pass");
    //        while(Vector3.Distance(transform.position, m_Target.position) < distanceToThrow)
    //        {
    //            var dir = transform.position - m_Target.position;
    //            dir.y = 0;

    //            navMeshAgent.SetDestination(dir.normalized);
    //            yield return null;
    //        }
    //        shouldThrow = true;
    //    }
    //    //else
    //    //{
    //    //    shouldThrow = false;
    //    //}
    //}

    //IEnumerator WaitForPathProcessing(Vector3 target)
    //{
    //    //Vector3 currentPos = transform.position;
    //    //Vector3 prevPos = Vector3.zero;

    //    while (navMeshAgent.)
    //    {
    //        yield return null;
    //        //temp++;
    //    }

    //    Vector3 agentDest = navMeshAgent.destination;
    //    Debug.Log($"Target Destination: {target}");
    //    Debug.Log($"Agent Destination: {agentDest}");
    //    //Debug.Log($"Loop iterations: {temp}");
    //    Vector3 xzTarget = new Vector3(target.x, 0, target.z);
    //    Vector3 xzAgentDest = new Vector3(agentDest.x, 0, agentDest.z);
    //    if (Vector3.Distance(target, agentDest) >= distanceToThrow)
    //    {
    //        Debug.Log("Throw stuff");
    //    }

    //    //temp = 0;
    //}

    private void GetRagdollLimbs()
    {
        Transform[] ragdollLimbs = m_ragdollParent.GetComponentsInChildren<Transform>();

        foreach(var limb in ragdollLimbs)
        {
            m_ragdollLimbStartingVectors.Add(limb, new Vector3[] {limb.transform.position, limb.transform.eulerAngles, limb.transform.localScale});
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

    public void HandleCombatStateChange()
    {
        IsInCombat = !IsInCombat;
        CombatStateChanged?.Invoke(IsInCombat);
    }

    private void FacePlayer()
    {
        Vector3 lookPos = LevelManager.Instance.Player.transform.position - transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, attackRotateSpeed * Time.deltaTime);
    }

    private void UpdateAnimations()
    {
        //if the player is too far to be attacked but close enough to be chased
        if (distanceToPlayer > distanceToAttack && distanceToPlayer <= distanceToFollow /*&& distanceToPlayer > distanceToThrow - LOBBING BEHAVIOR*/)
        {
            //shouldThrow = false; - LOBBING BEHAVIOR
            if (!IsInCombat) HandleCombatStateChange();
            animator.SetBool("PunchPlayer", false);
            animator.SetBool("ChasePlayer", true);
            animator.SetBool("PlayerTooFar", false);
            animator.SetBool("ThrowStuff", false);
        }

        //if the player is close enough to be attacked
        if (distanceToPlayer <= distanceToAttack /*&& !shouldThrow - LOBBING BEHAVIOR*/)
        {
            if (!IsInCombat) HandleCombatStateChange();
            meleeAttackingPlayer = true;
            animator.SetBool("ChasePlayer", false);
            animator.SetBool("PunchPlayer", true);
            animator.SetBool("PlayerTooFar", false);
            animator.SetBool("ThrowStuff", false);
        }

        ////if the player is too far to be attacked, far enough to throw at, and in a place where the enemy can't reach them
        //if (distanceToPlayer <= distanceToThrow && shouldThrow) - LOBBING BEHAVIOR
        //{
        //    if (!IsInCombat) HandleCombatStateChange();
        //    throwingAtPlayer = true;
        //    animator.SetBool("ChasePlayer", false);
        //    animator.SetBool("PunchPlayer", false);
        //    animator.SetBool("PlayerTooFar", false);
        //    animator.SetBool("ThrowStuff", true);
        //}

        //If the player is too far away from the enemy
        if (distanceToPlayer >= distanceToFollow)
        {
            if (IsInCombat) HandleCombatStateChange();
            animator.SetBool("ChasePlayer", false);
            animator.SetBool("PunchPlayer", false);
            animator.SetBool("PlayerTooFar", true);
            animator.SetBool("ThrowStuff", false);
        }

        //if (animator.GetBool("ChasePlayer")) - LOBBING BEHAVIOR
        //{
        //    throwingAtPlayer = false;
        //    meleeAttackingPlayer = false;
        //}
    }

    private void MeleeAttackPlayer()
    {
        //a bool to make sure the swarmer doesn't move while trying to hit the player
        inAttackAnim = true;
        m_AudioSource.PlayOneShot(m_AttackSound);
        //send out raycast to see if enemy hit player
        if (Physics.Raycast(raycastSource.position, gameObject.transform.forward, out hitInfo, attackReach))
        {
            if (hitInfo.transform.CompareTag("Player"))
            {
                GiveDamage(damageToDeal);
            }
        }
    }

    private void MeleeAttackOver()
    {
        meleeAttackingPlayer = false;
        inAttackAnim = false;
        //animator.SetBool("PunchSwitch", !animator.GetBool("PunchSwitch"));
    }

    //private void ThrowAttack() - LOBBING BEHAVIOR
    //{
    //    inAttackAnim = true;
    //    //Debug.Log("Throw stuff");
    //    Shooting.Shoot();
    //}

    //private void ThrowAttackOver() - LOBBING BEHAVIOR
    //{
    //    throwingAtPlayer = false;
    //    shouldThrow = false;
    //    //meleeAttackingPlayer = false;
    //    inAttackAnim = false;
    //}

    private void GiveDamage(float damageToDeal)
    {
        LevelManager.Instance.Player.TakeDamage(damageToDeal, HitBoxType.normal);
        mostRecentHit = Time.time;
    }

    public void CheckForDeath()
    {
        if (Health <= 0 && !m_isDead)
        {
            //dead = true;
            //if (animator.GetInteger("Death") != 0) return;
            //int deathanimation = Random.Range(1, 4);
            //animator.SetInteger("Death", deathanimation);
            OnDeath();
        }
    }

    public void DeathAnimationOver()
    {
        OnDeath();
    }

    public void OnDeath()
    {
        m_isDead = true;
        IsDead = true;
        ignorePlayer = true;
        m_updateAnims = false;
        m_AudioSource.PlayOneShot(m_onDeathSFX);
        EnableRagdoll(Vector3.zero);

        if (IsInCombat) HandleCombatStateChange();

        if (m_shouldHitStop) LevelManager.TimeStop(m_hitStopDuration);

        if (distanceToPlayer <= LevelManager.Instance.Player.DistanceToHeal)
        {
            //ProjectileManager.Instance.TakeFromPool(m_OnKillHealFVX, transform.position);
            //LevelManager.Instance.Player.Health += (LevelManager.Instance.Player.PercentToHeal * maxHealth);
            LevelManager.Instance.Player.HealthRegen(LevelManager.Instance.Player.PercentToHeal * maxHealth, transform.position);
        }

        //if (fractureScript != null) fractureScript.Breakage();
        EnemyDied?.Invoke();
        EnemyDefeated?.Invoke(true);
        CycleAgent();
        //gameObject.SetActive(false);
        hideBehavior.EndHideProcessRemote();
        hideBehavior.enabled = false;
        LevelManager.Instance.CheckPointReached += OnCheckPointReached;
        //navMeshAgent.Warp(m_StartingPosition);
    }

    public void OnDeath(Vector3 hitPoint)
    {
        IsDead = true;
        ignorePlayer = true;
        m_updateAnims = false;
        m_AudioSource.PlayOneShot(m_onDeathSFX);
        EnableRagdoll(hitPoint);

        if (IsInCombat && !m_isDead) HandleCombatStateChange();

        if (m_shouldHitStop && !m_isDead) LevelManager.TimeStop(m_hitStopDuration);

        if (distanceToPlayer <= LevelManager.Instance.Player.DistanceToHeal && !m_isDead)
        {
            //ProjectileManager.Instance.TakeFromPool(m_OnKillHealFVX, transform.position);
            //LevelManager.Instance.Player.Health += (LevelManager.Instance.Player.PercentToHeal * maxHealth);
            LevelManager.Instance.Player.HealthRegen(LevelManager.Instance.Player.PercentToHeal * maxHealth, transform.position);
        }

        //if (fractureScript != null) fractureScript.Breakage();
        EnemyDied?.Invoke();
        CycleAgent();
        //gameObject.SetActive(false);
        hideBehavior.EndHideProcessRemote();
        hideBehavior.enabled = false;
        LevelManager.Instance.CheckPointReached += OnCheckPointReached;
        m_isDead = true;
        //navMeshAgent.Warp(m_StartingPosition);
    }

    private void DisableRagdoll()
    {
        ResetLimbs();
        animator.enabled = true;

        foreach (Rigidbody r in rigidBones)
        {
            r.isKinematic = true;
        }

        checker.gameObject.GetComponent<SphereCollider>().enabled = true;
        navMeshAgent.speed = originalSpeed;
    }

    private void EnableRagdoll(Vector3 hitPoint)
    {
        navMeshAgent.speed = 0;
        animator.enabled = false;
        checker.gameObject.GetComponent<SphereCollider>().enabled = false;

        foreach (Rigidbody r in rigidBones)
        {
            r.isKinematic = false;
            //r.AddExplosionForce(ragdollForce, gameObject.transform.position, 50, 70, ForceMode.Impulse);
            //r.AddForce(LevelManager.Instance.Player.transform.forward * ragdollForce, ForceMode.Impulse);
        }


        Vector3 forceDirection = TorsoRigidBody.position - hitPoint;
        
        TorsoRigidBody.AddForce(forceDirection.normalized * ragdollForce * ragdollForceScale, ForceMode.Impulse);

    }

    public void OnPlayerRespawn()
    {
        DisableRagdoll();
        IsDead = false;
        m_isDead = false;
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        navMeshAgent.Warp(m_StartingPosition);
        CycleAgent();
        Health = maxHealth;
        ignorePlayer = defaultIgnorePlayer;
        m_updateAnims = true;
    }

    void CycleAgent()
    {
        if (!navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.enabled = false;
            navMeshAgent.enabled = true;
        }
        else
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.isStopped = false;
            
        }

        meleeAttackingPlayer = false;
        inAttackAnim = false;
        chasePlayer = false;
        animator.SetInteger("Death", 0);
    }

    public void OnCheckPointReached()
    {
        LevelManager.Instance.PlayerRespawn -= OnPlayerRespawn;
    }

    public void ChangeIgnorePlayer(bool shouldIgnorePlayer)
    {
        ignorePlayer = shouldIgnorePlayer;
    }

    public void TakeDamage(float damageTaken, HitBoxType hitbox)
    {
        health -= damageTaken;
        ragdollForce = damageTaken;
        if(fractureScript != null) fractureScript.Health = health;
        CheckForDeath();
        m_IDamageable.InstantiateDamageNumber(damageTaken, hitbox);
    }

    public void TakeDamage(float damageTaken, HitBoxType hitbox, Vector3 hitPoint = default(Vector3))
    {
        health -= damageTaken;
        ragdollForce = damageTaken;
        if (fractureScript != null) fractureScript.Health = health;
        if (Health <= 0) OnDeath(hitPoint);
        m_IDamageable.InstantiateDamageNumber(damageTaken, hitbox);
    }
}
