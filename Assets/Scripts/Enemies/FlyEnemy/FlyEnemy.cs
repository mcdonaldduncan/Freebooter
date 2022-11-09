using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FlyEnemy : MonoBehaviour, IDamageable
{
    bool dead = false;
    private enum SoldierState { guard, wanderer, chase, originalSpot, Relocating, Death };
    [Tooltip("/ Guard = Stand in one place until the player breaks line of sight / Wanderer = walks around / Chase = when the soldier goes after the enemy")]
    [SerializeField] private SoldierState st;
    private SoldierState origianlst;
    [SerializeField] private GameObject target, tip, light, visionPoint, body,SensorR,SensorL;
    [SerializeField] private float rotationspeed, range;
    [SerializeField] private UnityEngine.AI.NavMeshAgent agent;
    Vector3 targetDiretion, originalPos;
    Quaternion rotation, originalrot;
    
    [SerializeField] List<Vector3> wanderSpots = new List<Vector3>();
    [Tooltip("Distance to current wander spot before the player moves to next wander spot.")]
    [SerializeField] private float wanderSpotOffset = 1f, delayBeforeMove = 2, originalPosOFFSET = 1f, wanderDistanceR, wanderDistanceL, wanderDistanceF, wanderDistanceB;
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
            st = SoldierState.Death;
        }
    }

    private void Start()
    {
        target = GameObject.FindWithTag("Player");
        origianlst = st;
        originalPos = transform.position;
        originalrot = this.transform.rotation;
        var pos = this.transform.position;
        wanderSpots.Add(new Vector3(pos.x + wanderDistanceR, pos.y, pos.z));
        wanderSpots.Add(new Vector3(pos.x - wanderDistanceL, pos.y, pos.z));
        wanderSpots.Add(new Vector3(pos.x, pos.y, pos.z + wanderDistanceF));
        wanderSpots.Add(new Vector3(pos.x, pos.y, pos.z - wanderDistanceB));
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (health <= 0)
        {
            st = SoldierState.Death;
        }
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
                if (health > 0)
                {
                    Aim();
                    Shoot();
                    ChasePlayer();
                }
               
                break;
            case (SoldierState.originalSpot):
                ReturnToOriginalSpot();
                break;
            case SoldierState.Relocating:
                //Im calling Relocate() somehwere else to not call it many times in update
                Aim();
                Shoot();
                break;
            case SoldierState.Death:
                if (dead == false)
                {
                    dead = true;
                    agent.ResetPath();
                    body.GetComponent<OnDeathExplosion>().OnDeathVariables();
                }
                break;
            default:
                break;
        }
    }

    void Aim() //This is pointing the torret towards the player as long as he is in range
    {
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
        RaycastHit hit;
        Debug.DrawRay(visionPoint.transform.position, targetDiretion, Color.green);
        Physics.Raycast(visionPoint.transform.position, targetDiretion, out hit, range / 1.2f);
        if (hit.collider != null)
        {
            if (hit.collider.tag == target.tag)
            {
                Debug.Log("Player Detected");
                Invoke("StateChase", 2);
            }
            else if (hit.collider.tag != target.tag)
            {
                Debug.Log("Player NOT Detected");
            }
        }
        else { }
    }

    void Shoot() //Shoots at the player
    {
        RaycastHit hit;
        Debug.DrawRay(tip.transform.position, targetDiretion, Color.red);

        Physics.Raycast(tip.transform.position, targetDiretion, out hit, range);
        if (hit.collider != null)
        {
            if (hit.collider.tag == target.tag)
            {
                if (Time.time > ShootRate + lastShot)
                {
                    var bt = Instantiate(BulletTrail, tip.transform.position, rotation);
                    bt.GetComponent<MoveForward>().origin = this.gameObject.transform.rotation;
                    bt.GetComponent<MoveForward>().target = target;
                    //bt.GetComponent<MoveForward>().damage = Damage;
                    Debug.Log("Player was shot, dealing damage.");
                    target.GetComponent<FirstPersonController>().TakeDamage(Damage);
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
        if (Vector3.Distance(this.transform.position, wanderSpots[i]) < wanderSpotOffset)
        {
            if (changeDir == false)
            {
                Invoke("WaitBeforeWanderToNextSpot", delayBeforeMove);
                changeDir = true;
            }
        }
        else if (Vector3.Distance(this.transform.position, wanderSpots[i]) >= wanderSpotOffset)
        {
            agent.SetDestination(wanderSpots[i]);
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
        var dist = Vector3.Distance(this.transform.position, target.transform.position);
        if (dist < range && dist > range / 2)
        {
            agent.SetDestination(target.transform.position);
        }
    }

    void StateChase() //swaps state to Chase
    {
        if (st != SoldierState.Death)
        {
            st = SoldierState.chase;
            Invoke("RecolcateState", 5);
        }
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

    void RecolcateState()
    {
        if (health > 0)
        {
            st = SoldierState.Relocating;
            Relocate();
        }
    }

    void Relocate()
    {
        var pos = this.transform.position;
        float dist = range / 2;
        if (Right(pos, dist) == true)
        {
            agent.SetDestination(new Vector3(pos.x + dist, pos.y, pos.z));
        }
        else if (Left(pos, dist) == true)
        {
            agent.SetDestination(new Vector3(pos.x - dist, pos.y, pos.z + dist));
        }
        StateChase();
        
    }

    bool Right(Vector3 pos, float dist)
    {
        RaycastHit hit;
        Debug.DrawRay(SensorR.transform.position, Vector3.right, Color.blue);
        Physics.Raycast(SensorR.transform.position, Vector3.right, out hit, range / 2);
        if (hit.collider == null)
        {
            return true;
        }
        return false;
    }

    bool Left(Vector3 pos, float dist)
    {
        RaycastHit hit;
        Debug.DrawRay(SensorL.transform.position, -Vector3.right, Color.blue);
        Physics.Raycast(SensorR.transform.position, -Vector3.right, out hit, range / 2);
        if (hit.collider == null)
        {
            return true;
        }
        return false;
    }
}
