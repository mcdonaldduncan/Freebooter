using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemySwarmerBehavior : MonoBehaviour, IDamageable
{
    public float Health { get { return health; } set { health = value; } }

    [SerializeField] private bool ignorePlayer;
    
    [SerializeField] private float health = 75;
    [SerializeField] private float damageToDeal = 20;
    [SerializeField] private GameObject player;
    [SerializeField] private float distanceToFollow = 20;
    [SerializeField] private float distanceToAttack = 2;
    [SerializeField] private float attackReach = 3;
    [SerializeField] private float attackRotateSpeed = 10;
    [SerializeField] private Transform raycastSource;

    private FirstPersonController playerController;
    private NavMeshAgent navMeshAgent;
    private RaycastHit hitInfo;
    private Animator animator;
    private float mostRecentHit;
    private float distanceToPlayer;
    private bool chasePlayer;
    private bool attackingPlayer;
    private bool inAttackAnim;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        chasePlayer = false;
        animator.SetBool("PlayerTooFar", true);
        animator.SetBool("ChasePlayer", false);
        animator.SetBool("AttackPlayer", false);
        playerController = player.GetComponent<FirstPersonController>();
    }

    private void Update()
    {
        distanceToPlayer = Vector3.Distance(gameObject.transform.position, player.transform.position);


        if (distanceToPlayer <= distanceToFollow)
        {
            chasePlayer = true;
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
        inAttackAnim = true;
        if (Physics.Raycast(raycastSource.position, gameObject.transform.forward, out hitInfo, attackReach))
        {
            Debug.Log($"Swarmer Hit Player!");
            if (hitInfo.transform.CompareTag("Player"))
            {
                GiveDamage(damageToDeal);
            }
        }
        else
        {
            Debug.Log($"Swarmer hit nothing");
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
            DestroyImmediate(gameObject);
        }
    }
}
