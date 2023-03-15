using UnityEngine;
using UnityEngine.AI;
using TMPro;

public sealed class EnemySwarmerBehavior : MonoBehaviour, IDamageable
{
    public float Health { get { return health; } set { health = value; } }

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

    [Header("Attacks and Movement")]
    [SerializeField] private float distanceToFollow = 20;
    [SerializeField] private float distanceToAttack = 2;
    [SerializeField] private float attackReach = 3;
    [SerializeField] private float attackRotateSpeed = 10;

    [Header("DamagePopUp")]
    [SerializeField] GameObject m_DamagePopUpPrefab;
    [SerializeField] Transform m_PopupFromHere;
    [SerializeField] bool m_showDamageNumbers;
    float m_fontSize = 5;

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
    private float ragdollForce;
    private bool chasePlayer;
    private bool hideFromPLayer;
    private bool attackingPlayer;
    private bool inAttackAnim;

    private Rigidbody[] rigidBones;

    private Fracture fractureScript;

    private bool isSwarm => Physics.OverlapSphereNonAlloc(transform.position, 15f, hits, enemies) >= hideThreshold;

    public Vector3 StartingPosition { get { return m_StartingPosition; } set { m_StartingPosition = value; } }

    //public float MovementSampleRadius { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //public bool ShouldSleep { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    //public IActivator Activator { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    private Vector3 m_StartingPosition;

    private Collider[] hits = new Collider[5];

    public GameObject DamageTextPrefab => m_DamagePopUpPrefab;
    public Transform TextSpawnLocation => m_PopupFromHere;
    public float FontSize => m_fontSize;
    public bool ShowDamageNumbers => m_showDamageNumbers;

    public TextMeshPro Text { get; set; }

    bool dead = false;

    public delegate void SwarmerDelegate();
    public event SwarmerDelegate SwarmerDeath;

    private void Awake()
    {
        hideBehavior = GetComponent<HideBehavior>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        m_IDamageable = this;
    }

    private void Start()
    {
        m_StartingPosition = transform.position;
        Health = maxHealth;
        chasePlayer = false;
        animator.SetBool("PlayerTooFar", true);
        animator.SetBool("ChasePlayer", false);
        animator.SetBool("AttackPlayer", false);
        LevelManager.PlayerRespawn += OnPlayerRespawn;
        m_Target = LevelManager.Instance.Player.transform;
        //hideBehavior.enabled = false;
        m_IDamageable.SetupDamageText();
        fractureScript = GetComponentInChildren<Fracture>();
        originalSpeed = navMeshAgent.speed;

        checker = GetComponentInChildren<LineOfSightChecker>();

        rigidBones = gameObject.GetComponentsInChildren<Rigidbody>();
        DisableRagdoll();
    }

    private void Update()
    {
        distanceToPlayer = Vector3.Distance(transform.position, m_Target.position);

        if (dead == true) return;
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
            if (!attackingPlayer)
            {
                //navMeshAgent.ResetPath();
                if (navMeshAgent.isPathStale)
                {
                    navMeshAgent.ResetPath();
                }

                navMeshAgent.SetDestination(LevelManager.Instance.Player.transform.position);
            }
            if (attackingPlayer)
            {
                navMeshAgent.isStopped = true;
                if (!inAttackAnim)
                {
                    FacePlayer();
                }
            }
        }

        UpdateAnimations();
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
        //if the player is close enough to be attacked
        if (distanceToPlayer <= distanceToAttack)
        {
            attackingPlayer = true;
            animator.SetBool("ChasePlayer", false);
            animator.SetBool("AttackPlayer", true);
            animator.SetBool("PlayerTooFar", false);
        }

        //if the player is too far to be attacked but close enough to be chased
        if (distanceToPlayer >= distanceToAttack && distanceToPlayer <= distanceToFollow)
        {
            animator.SetBool("AttackPlayer", false);
            animator.SetBool("ChasePlayer", true);
            animator.SetBool("PlayerTooFar", false);
        }
        
        //If the player is too far away from the enemy
        if (distanceToPlayer >= distanceToFollow)
        {
            animator.SetBool("ChasePlayer", false);
            animator.SetBool("AttackPlayer", false);
            animator.SetBool("PlayerTooFar", true);
        }

    }

    private void AttackPlayer()
    {
        //a bool to make sure the swarmer doesn't move while trying to hit the player
        inAttackAnim = true;

        //send out raycast to see if enemy hit player
        if (Physics.Raycast(raycastSource.position, gameObject.transform.forward, out hitInfo, attackReach))
        {
            if (hitInfo.transform.CompareTag("Player"))
            {
                GiveDamage(damageToDeal);
            }
        }
    }

    private void AttackOver()
    {
        attackingPlayer = false;
        inAttackAnim = false;
        animator.SetBool("PunchSwitch", !animator.GetBool("PunchSwitch"));
    }

    private void GiveDamage(float damageToDeal)
    {
        LevelManager.Instance.Player.TakeDamage(damageToDeal);
        mostRecentHit = Time.time;
    }

    public void CheckForDeath()
    {
        if (Health <= 0)
        {
            //dead = true;
            //if (animator.GetInteger("Death") != 0) return;
            //int deathanimation = Random.Range(1, 4);
            //animator.SetInteger("Death", deathanimation);

            if (distanceToPlayer <= LevelManager.Instance.Player.DistanceToHeal)
            {
                ProjectileManager.Instance.TakeFromPool(m_OnKillHealFVX, transform.position);
                //LevelManager.Instance.Player.Health += (LevelManager.Instance.Player.PercentToHeal * maxHealth);
                LevelManager.Instance.Player.HealthRegen(LevelManager.Instance.Player.PercentToHeal * maxHealth);
            }
            OnDeath();
        }
    }

    public void DeathAnimationOver()
    {
        OnDeath();
    }

    public void OnDeath()
    {
        EnableRagdoll();

        if (m_shouldHitStop) LevelManager.TimeStop(m_hitStopDuration);

        //if (fractureScript != null) fractureScript.Breakage();
        SwarmerDeath?.Invoke();
        CycleAgent();
        //gameObject.SetActive(false);
        hideBehavior.EndHideProcessRemote();
        hideBehavior.enabled = false;
        LevelManager.CheckPointReached += OnCheckPointReached;
        //navMeshAgent.Warp(m_StartingPosition);
    }

    private void DisableRagdoll()
    {
        animator.enabled = true;
        gameObject.GetComponent<CapsuleCollider>().enabled = true;
        checker.gameObject.GetComponent<SphereCollider>().enabled = true;

        foreach (Rigidbody r in rigidBones)
        {
            r.isKinematic = true;
        }
    }

    private void EnableRagdoll()
    {
        navMeshAgent.speed = 0;
        animator.enabled = false;
        gameObject.GetComponent<CapsuleCollider>().enabled = false;
        checker.gameObject.GetComponent<SphereCollider>().enabled = false;

        foreach (Rigidbody r in rigidBones)
        {
            r.isKinematic = false;
            //r.AddExplosionForce(ragdollForce, gameObject.transform.position, 50, 70, ForceMode.Impulse);
            r.AddForce(LevelManager.Instance.Player.transform.forward * ragdollForce, ForceMode.Impulse);
        }
    }

    public void OnPlayerRespawn()
    {
        DisableRagdoll();
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        navMeshAgent.Warp(m_StartingPosition);
        navMeshAgent.speed = originalSpeed;
        navMeshAgent.isStopped = false;
        CycleAgent();
        Health = maxHealth;
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

        attackingPlayer = false;
        inAttackAnim = false;
        chasePlayer = false;
        animator.SetInteger("Death", 0);
        dead = false;
    }

    public void OnCheckPointReached()
    {
        LevelManager.PlayerRespawn -= OnPlayerRespawn;
    }

    public void ChangeIgnorePlayer(bool shouldIgnorePlayer)
    {
        ignorePlayer = shouldIgnorePlayer;
    }

    public void TakeDamage(float damageTaken)
    {
        health -= damageTaken;
        ragdollForce = damageTaken;
        if(fractureScript != null) fractureScript.Health = health;
        CheckForDeath();
        m_IDamageable.InstantiateDamageNumber(damageTaken, HitBoxType.normal);
    }
}
