using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class NetSwarmer : NetworkBehaviour, IDamageable
{
    public float Health { get { return health; } set { health = value; } }

    public GameObject DamagePopUpPrefab => throw new System.NotImplementedException();

    public Transform PopupFromHere => throw new System.NotImplementedException();

    public float fontSize => throw new System.NotImplementedException();

    public bool showDamageNumbers => throw new System.NotImplementedException();

    [SerializeField] private bool ignorePlayer;

    [SerializeField] private float health;
    [SerializeField] private float damageToDeal;
    [SerializeField] private float timeBetweenHits;
    [SerializeField] private float distanceToStartFollow;

    private NavMeshAgent navMeshAgent;
    private float mostRecentHit;

    private NetworkObject netObj;

    private NetworkPlayerController target;

    public NonPooledDynamicSpawner spawner;

    ulong networkID;

    public override void OnNetworkSpawn()
    {
        //if (!IsOwnedByServer) Destroy(gameObject);
        //enabled = IsOwnedByServer ? true : false;
        networkID = netObj.NetworkObjectId;
    }

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        netObj = GetComponent<NetworkObject>();
    }

    private void Update()
    {
        if (!IsOwnedByServer) return;

        FindTarget();
        if (target == null) return;

        float distanceToPlayer = Vector3.Distance(gameObject.transform.position, target.transform.position);

        if (!ignorePlayer && distanceToPlayer <= distanceToStartFollow)
        {
            navMeshAgent.destination = target.transform.position;
        }
    }

    void FindTarget()
    {
        if (TargetManager.Instance == null) return;
        if (TargetManager.Instance.targets.Length == 0) return;
        if (TargetManager.Instance.targets.Length == 1)
        {
            target = TargetManager.Instance.targets[0];
        }

        float distanceToPlayer1 = 0;
        float distanceToPlayer2 = 0;
        for (int i = 0; i < TargetManager.Instance.targets.Length; i++)
        {
            if (TargetManager.Instance.targets[i] == null) return;
            if (i == 0)
            {
                distanceToPlayer1 = Vector3.Distance(gameObject.transform.position, TargetManager.Instance.targets[i].transform.position);
            }
            else
            {
                distanceToPlayer2 = Vector3.Distance(gameObject.transform.position, TargetManager.Instance.targets[i].transform.position);
            }
        }
        if (distanceToPlayer1 < distanceToPlayer2)
        {
            target = TargetManager.Instance.targets[0];
        }
        if (distanceToPlayer2 < distanceToPlayer1)
        {
            if (TargetManager.Instance.targets.Length <= 1) return;
            target = TargetManager.Instance.targets[1];
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider == target.playerCol)
        {
            if (mostRecentHit + timeBetweenHits < Time.time)
            {
                GiveDamage(damageToDeal);
            }
        }
    }

    private void GiveDamage(float damageToDeal)
    {
        target.TakeDamage(damageToDeal);
        mostRecentHit = Time.time;
    }

    public void TakeDamage(float damageTaken, HitBoxType? hitType = null)
    {
        Health -= damageTaken;
        CheckForDeath();
    }


    public void CheckForDeath()
    {
        if (Health <= 0)
        {
            //this.gameObject.GetComponent<CheckForDrops>().DropOrNot();
            //DestroyImmediate(gameObject);
            //Destroy(gameObject);
            DestroyObjectServerRPC(networkID);
            //netObj.Despawn();
            //spawner.RunDestroy();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void DestroyObjectServerRPC(ulong id)
    {
        var objects = FindObjectsOfType<NetworkObject>().Where(n => n.NetworkObjectId == id).ToArray();
        foreach (NetworkObject networkObject in objects)
        {
            if (networkObject != null) networkObject.Despawn();
        }
    }

    public void TakeDamage(float damageTaken)
    {
        throw new System.NotImplementedException();
    }

    public void GenerateDamageInfo(float damageTaken, HitBoxType hitType)
    {
        throw new System.NotImplementedException();
    }
}
