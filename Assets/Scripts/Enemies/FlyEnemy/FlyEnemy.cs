using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class FlyEnemy : MonoBehaviour, IDamageable
{
    [SerializeField] private LayerMask layerMask = 0; // for wondering
    [SerializeField] private float minWaitTimeWander, maxWaitTimeWander, wonderDistanceRange; // wait timer for wandering

    float waitTimer;
    bool wanderMiniState; //state for wandering and wanderingIdle


    int shotCount;
    bool dead = false;
    private enum SoldierState { guard, wanderer, chase, originalSpot, Relocating, Death, retaliate};
    [Tooltip("/ Guard = Stand in one place until the player breaks line of sight / Wanderer = walks around / Chase = when the soldier goes after the enemy")]
    [SerializeField] private SoldierState st;
    private SoldierState origianlst;
    [SerializeField] private GameObject target, tip, light, visionPoint, body,SensorR,SensorL;
    [SerializeField] private float rotationspeed, range;
    [SerializeField] private UnityEngine.AI.NavMeshAgent agent;
    Vector3 targetDiretion, originalPos;
    Quaternion rotation, originalrot;

    [Tooltip("Distance to current wander spot before the player moves to next wander spot.")]
    [SerializeField] private float wanderSpotOffset = 1f, delayBeforeMove = 2, originalPosOFFSET = 1f;
    private float lastShot, ShootRate = .5f;
    int i = 0;
    bool changeDir = false;
    [SerializeField]

    public TrailRenderer BulletTrail;
    [SerializeField] private float Damage;

    public float Health { get { return health; } set { health = value; } }
    [SerializeField] private float health, maxHealth;

    float distanceToPlayer;
    FirstPersonController playerController;

    public void TakeDamage(float damageTaken)
    {
        if (st == SoldierState.guard || st == SoldierState.wanderer)
        {
            st = SoldierState.retaliate;
        }
        Health -= damageTaken;
        CheckForDeath();
    }

    public void CheckForDeath()
    {
        if (Health <= 0)
        {
            if (distanceToPlayer <= playerController.DistanceToHeal)
            {
                playerController.Health += (playerController.PercentToHeal * maxHealth);
            }
            //this.gameObject.GetComponent<CheckForDrops>().DropOrNot();
            Destroy(this.gameObject);
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
                if (wanderMiniState)
                {
                    ShouldIWander();
                }
                else if (!wanderMiniState)
                {
                    WanderIde();
                }
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
            case (SoldierState.retaliate):
                RetaliationAim();
                RetaliationShoot();
                RetaliationChasePlayer();
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

    private void ShouldIWander()
    {
        if (waitTimer > 0) // if wait timer is > 0 do nothing except waiting
        {
            waitTimer -= Time.deltaTime;
            return;
        }
        agent.SetDestination(RandomPosInSphere(originalPos, wonderDistanceRange, layerMask)); //Set destination inside a random sphere

        wanderMiniState = !wanderMiniState;
        // I just made a bool to make sure it doesnt call again after it sets path to make animating it easier and prevent setting more paths while its not completed
    }

    private void WanderIde()
    {
        if (agent.pathStatus != NavMeshPathStatus.PathComplete)
            return;

        waitTimer = Random.Range(minWaitTimeWander, maxWaitTimeWander);
        wanderMiniState = !wanderMiniState; // I just made a bool to make sure it doesnt call again after it completes path to make animating it easier.
    }

    Vector3 RandomPosInSphere(Vector3 origin, float distance, LayerMask layerMask)
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * distance; //Create a sphere
        randomDirection += origin; //add origin to place it at the origin

        NavMeshHit navHit;

        NavMesh.SamplePosition(randomDirection, out navHit, distance, layerMask); //get an appropriate navHit where we can set destination later

        return navHit.position; //return hit position to set destination on it.
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
            var lastState = st;
            st = SoldierState.Relocating;
            Relocate(lastState);
        }
    }

    void Relocate(SoldierState lastState)
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
        st = lastState;
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


    void RetaliationAim() //This is pointing the soldier towards the player as long as he is in range
    {
        float tempSpeed = rotationspeed;

        targetDiretion = target.transform.position - transform.position;
        rotation = Quaternion.LookRotation(targetDiretion);
        body.transform.rotation = Quaternion.RotateTowards(body.transform.rotation, rotation, tempSpeed * Time.deltaTime * 180);

    }

    void RetaliationShoot()
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
        if (shotCount%5 == 0)
        {
            Relocate(st);
        }
    }

    void RetaliationChasePlayer()
    {
        var dist = Vector3.Distance(this.transform.position, target.transform.position);
        if (dist <= range / 3)
        {
            agent.ResetPath();
        }
        else if (dist > range)
        {
            agent.SetDestination(target.transform.position);
        }

    }
}
