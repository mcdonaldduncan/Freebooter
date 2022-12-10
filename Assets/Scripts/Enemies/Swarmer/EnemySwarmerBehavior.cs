using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemySwarmerBehavior : MonoBehaviour, IDamageable
{
    public float Health { get; set;}

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

    private FirstPersonController playerController;
    private GameObject player;
    private HideBehavior hideBehavior;
    private NavMeshAgent navMeshAgent;
    private RaycastHit hitInfo;
    private Animator animator;
    private float mostRecentHit;
    private float distanceToPlayer;
    private bool chasePlayer;
    private bool hideFromPLayer;
    private bool attackingPlayer;
    private bool inAttackAnim;


    private bool isSwarm => Physics.OverlapSphereNonAlloc(transform.position, 10f, hits, enemies) > hideThreshold;
    private Collider[] hits = new Collider[5];

    private void Awake()
    {
        hideBehavior = GetComponent<HideBehavior>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player");
    }

    private void Start()
    {
        Health = maxHealth;
        chasePlayer = false;
        animator.SetBool("PlayerTooFar", true);
        animator.SetBool("ChasePlayer", false);
        animator.SetBool("AttackPlayer", false);
        playerController = player.GetComponent<FirstPersonController>();
        //layer = playerController.gameObject;
    }

    private void Update()
    {
        distanceToPlayer = Vector3.Distance(gameObject.transform.position, player.transform.position);


        if (distanceToPlayer <= distanceToFollow)
        {
            if (isSwarm)
            {
                chasePlayer = true;

                if (hideBehavior.enabled == true) hideBehavior.enabled = false;
            }
            else
            {
                chasePlayer = false;
                //FacePlayer();

                if (hideBehavior.enabled == false)
                {
                    navMeshAgent.ResetPath();
                    hideBehavior.enabled = true;
                    hideBehavior.StartHideProcessRemote(player.transform);
                }
            }
            
        }
        

        if (!ignorePlayer && chasePlayer)
        {
            if (!attackingPlayer)
            {
                navMeshAgent.ResetPath();
                navMeshAgent.SetDestination(player.transform.position);
            }
            if (attackingPlayer)
            {
                navMeshAgent.Stop();
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
        Vector3 lookPos = player.transform.position - transform.position;
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
        playerController.TakeDamage(damageToDeal);
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
            //this.gameObject.GetComponent<CheckForDrops>().DropOrNot();
            //if (distanceToPlayer <= playerController.DistanceToHeal)
            //{
            //    playerController.Health += (playerController.PercentToHeal * maxHealth);
            //}
            Destroy(gameObject);
        }
    }
}
