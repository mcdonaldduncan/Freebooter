using UnityEngine;
using UnityEngine.AI;

public sealed class EnemySwarmerBehavior : MonoBehaviour, IDamageable, IEnemy
{
    public float Health { get { return health; } set { health = value; } }

    [SerializeField] private bool ignorePlayer;


    [Header("Hide Properties")]
    [Tooltip("The swarmer will hide if not accompanied by this many other enemies (0 = never hide)")]
    [SerializeField] private float hideThreshold;

    [Header("Health and Damage")]
    [SerializeField] private float maxHealth = 75;
    [SerializeField] private float health = 75;
    [SerializeField] private float damageToDeal = 20;

    [Header("Attacks and Movement")]
    [SerializeField] private float distanceToFollow = 20;
    [SerializeField] private float distanceToAttack = 2;
    [SerializeField] private float attackReach = 3;
    [SerializeField] private float attackRotateSpeed = 10;

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
    private bool chasePlayer;
    private bool hideFromPLayer;
    private bool attackingPlayer;
    private bool inAttackAnim;

    private bool isSwarm => Physics.OverlapSphereNonAlloc(transform.position, 15f, hits, enemies) >= hideThreshold;

    public Vector3 StartingPosition { get { return m_StartingPosition; } set { m_StartingPosition = value; } }

    public float MovementSampleRadius { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public bool ShouldSleep { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public IActivator Activator { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    private Vector3 m_StartingPosition;

    private Collider[] hits = new Collider[5];

    private void Awake()
    {
        hideBehavior = GetComponent<HideBehavior>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
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
        
    }

    private void Update()
    {
        distanceToPlayer = Vector3.Distance(transform.position, m_Target.position);


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
    }

    private void GiveDamage(float damageToDeal)
    {
        LevelManager.Instance.Player.TakeDamage(damageToDeal);
        mostRecentHit = Time.time;
    }

    public void TakeDamage(float damageTaken)
    {
        Health -= damageTaken;
        CheckForDeath();
    }

    public void CheckForDeath()
    {
        if (Health <= 0)
        {
            OnDeath();
        }
    }

    public void OnDeath()
    {
        if (distanceToPlayer <= LevelManager.Instance.Player.DistanceToHeal)
        {
            ProjectileManager.Instance.TakeFromPool(m_OnKillHealFVX, transform.position);
            //LevelManager.Instance.Player.Health += (LevelManager.Instance.Player.PercentToHeal * maxHealth);
            LevelManager.Instance.Player.HealthRegen(LevelManager.Instance.Player.PercentToHeal * maxHealth);
        }
        navMeshAgent.Warp(m_StartingPosition);
        CycleAgent();
        gameObject.SetActive(false);
        hideBehavior.EndHideProcessRemote();
        hideBehavior.enabled = false;
        LevelManager.CheckPointReached += OnCheckPointReached;
    }

    public void OnPlayerRespawn()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        navMeshAgent.Warp(m_StartingPosition);
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
    }

    public void OnCheckPointReached()
    {
        LevelManager.PlayerRespawn -= OnPlayerRespawn;
    }

    public void ChangeIgnorePlayer(bool shouldIgnorePlayer)
    {
        ignorePlayer = shouldIgnorePlayer;
    }

    public void ActivateAggro()
    {
        throw new System.NotImplementedException();
    }

    public void DeactivateAggro()
    {
        throw new System.NotImplementedException();
    }

    public void OnActivate()
    {
        throw new System.NotImplementedException();
    }

    public void OnDeactivate()
    {
        throw new System.NotImplementedException();
    }

    public void MoveToLocation(Transform location)
    {
        throw new System.NotImplementedException();
    }
}
