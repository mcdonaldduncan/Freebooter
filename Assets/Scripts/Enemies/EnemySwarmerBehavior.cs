using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySwarmerBehavior : MonoBehaviour, IDamageable
{
    public float Health { get { return health; } set { health = value; } }

    [SerializeField] private bool ignorePlayer;
    
    [SerializeField] private float health;
    [SerializeField] private float damageToDeal;
    [SerializeField] private GameObject player;
    [SerializeField] private float timeBetweenHits;

    private FirstPersonController playerController;
    private Collider playerCollider;    
    private NavMeshAgent navMeshAgent;
    private float mostRecentHit;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        playerController = player.GetComponent<FirstPersonController>();
        playerCollider = player.GetComponent<Collider>();
    }

    private void Update()
    {
        if (!ignorePlayer)
        {
            navMeshAgent.destination = player.transform.position;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider == playerCollider)
        {
            if (mostRecentHit + timeBetweenHits < Time.time)
            {
                GiveDamage(damageToDeal);
            }
        }
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
            this.gameObject.GetComponent<CheckForDrops>().DropOrNot();
            Destroy(gameObject);
        }
        else
        {
            return;
        }
    }
}
