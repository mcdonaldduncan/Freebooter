using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetFlyEnemy : NetworkBehaviour, IDamageable
{

    private enum SoldierState { guard, wanderer, chase, originalSpot };
    [Tooltip("/ Guard = Stand in one place until the player breaks line of sight / Wanderer = walks around / Chase = when the soldier goes after the enemy")]
    [SerializeField] private SoldierState st;
    private SoldierState origianlst;
    [SerializeField] private GameObject tip, light, visionPoint, body;
    [SerializeField] private float rotationspeed, range;
    [SerializeField] private UnityEngine.AI.NavMeshAgent agent;
    Vector3 targetDiretion, originalPos;
    Quaternion rotation, originalrot;

    private NetworkPlayerController target;

    [SerializeField] List<Transform> wanderSpots = new List<Transform>();
    [Tooltip("Distance to current wander spot before the player moves to next wander spot.")]
    [SerializeField] private float wanderSpotOffset = 1f, delayBeforeMove = 2, originalPosOFFSET = 1f;
    private float lastShot, ShootRate = .5f;
    int i = 0;
    bool changeDir = false;
    [SerializeField]

    public TrailRenderer BulletTrail;
    [SerializeField] private float Damage;

    public float Health { get { return health; } set { health = value; } }
    [SerializeField] private float health;
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
            //Destroy(this.gameObject);
            DestroyObjectServerRPC();
        }
    }
    private void Start()
    {
        origianlst = st;
        originalPos = transform.position;
        originalrot = this.transform.rotation;
    }

    private void Update()
    {
        FindTarget();
    }

    void FindTarget()
    {
        if (TargetManager.Instance == null) return;


        float distanceToPlayer1 = 0;
        float distanceToPlayer2 = 0;
        for (int i = 0; i < TargetManager.Instance.targets.Length; i++)
        {
            if (i == 0)
            {
                distanceToPlayer1 = Vector3.Distance(gameObject.transform.position, TargetManager.Instance.targets[i].transform.position);
            }
            else
            {
                distanceToPlayer2 = Vector3.Distance(gameObject.transform.position, TargetManager.Instance.targets[i].transform.position);
            }
        }
        if (distanceToPlayer1 > distanceToPlayer2)
        {
            target = TargetManager.Instance.targets[0];
        }
        if (distanceToPlayer2 > distanceToPlayer1)
        {
            target = TargetManager.Instance.targets[1];
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        switch (st)
        {
            case SoldierState.guard:
                Aim();
                LineOfSightWithPlayer();

                break;
            case SoldierState.wanderer:
                Aim();
                LineOfSightWithPlayer();
                Wander();
                break;

            case (SoldierState.chase):
                Aim();
                Shoot();
                ChasePlayer();
                break;
            case (SoldierState.originalSpot):
                ReturnToOriginalSpot();
                break;
            default:
                break;
        }
    }
    void Aim() //This is pointing the torret towards the player as long as he is in range
    {
        if (target == null) return;
        float tempSpeed = rotationspeed;
        if (Vector3.Distance(this.transform.position, target.transform.position) < range)
        {
            targetDiretion = target.transform.position - transform.position;
            rotation = Quaternion.LookRotation(targetDiretion);
            body.transform.rotation = Quaternion.RotateTowards(body.transform.rotation, rotation, tempSpeed * Time.deltaTime * 180);
        }

    }

    void LineOfSightWithPlayer()
    {
        if (target == null) return;
        RaycastHit hit;
        //Debug.DrawRay(visionPoint.transform.position, targetDiretion, Color.green);
        Physics.Raycast(visionPoint.transform.position, targetDiretion, out hit, range / 1.2f);
        if (hit.collider != null)
        {
            if (hit.collider.tag == target.tag)
            {
                //Debug.Log("Player Detected");
                Invoke("StateChase", 2);
            }
            else if (hit.collider.tag != target.tag)
            {
                //Debug.Log("Player NOT Detected");
            }
        }
        else { }
    }
    void Shoot() //Shoots at the player
    {
        if (target == null) return;
        RaycastHit hit;
        //Debug.DrawRay(tip.transform.position, targetDiretion, Color.red);

        Physics.Raycast(tip.transform.position, targetDiretion, out hit, range);
        if (hit.collider != null)
        {
            if (hit.collider.tag == target.tag)
            {
                if (Time.time > ShootRate + lastShot)
                {
                    var bt = Instantiate(BulletTrail, tip.transform.position, rotation);
                    bt.GetComponent<MoveForward>().origin = this.gameObject.transform.rotation;
                    bt.GetComponent<MoveForward>().target = hit.point;
                    //bt.GetComponent<MoveForward>().damage = Damage;
                    //Debug.Log("Player was shot, dealing damage.");
                    target.TakeDamage(Damage);
                    lastShot = Time.time;
                }
            }
        }
        if (Vector3.Distance(this.transform.position, target.transform.position) > range)
        {
            StateReturnOriginalSpot();
        }
    }
    void Wander()
    {
        if (i >= wanderSpots.Count) { i = 0; }
        if (Vector3.Distance(this.transform.position, wanderSpots[i].transform.position) < wanderSpotOffset)
        {
            if (changeDir == false)
            {
                Invoke("WaitBeforeWanderToNextSpot", delayBeforeMove);
                changeDir = true;
            }
        }
        else if (Vector3.Distance(this.transform.position, wanderSpots[i].transform.position) >= wanderSpotOffset)
        {
            agent.SetDestination(wanderSpots[i].transform.position);
            //Debug.Log(wanderSpots[i].transform.position);
        }

    }
    void WaitBeforeWanderToNextSpot()
    {
        if (i + 1 >= wanderSpots.Count)
        {
            i = 0;
        }
        else
        {
            i++;
        }
        changeDir = false;
    }
    void ChasePlayer()
    {
        if (target != null) return;
        var dist = Vector3.Distance(this.transform.position, target.transform.position);
        if (dist < range && dist > range / 2)
        {
            agent.SetDestination(target.transform.position);
        }
    }
    void StateChase() //swaps state to Chase
    {
        st = SoldierState.chase;
    }
    void StateReturnOriginalSpot()
    {
        st = SoldierState.originalSpot;
    }
    void ReturnToOriginalSpot()
    {
        agent.SetDestination(originalPos);
        if (Vector3.Distance(this.transform.position, originalPos) < originalPosOFFSET)
        {
            this.transform.rotation = originalrot;
            ReturnToOriginalState();
        }
    }
    void ReturnToOriginalState()
    {
        st = origianlst;
    }

    [ServerRpc]
    void DestroyObjectServerRPC()
    {
        Destroy(gameObject);
    }
}
