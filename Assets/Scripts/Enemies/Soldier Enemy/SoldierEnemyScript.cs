using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SoldierEnemyScript : MonoBehaviour, IDamageable
{
    [SerializeField]
    private Animator animator;

    [SerializeField] private LayerMask layerMask = 0; // for wondering
    [SerializeField] private float minWaitTimeWander, maxWaitTimeWander, wonderDistanceRange; // wait timer for wandering
    
    float waitTimer;
    bool wanderMiniState; //state for wandering and wanderingIdle

    private enum SoldierState {guard, wanderer, chase, originalSpot, retaliate, emergencyHeal};
    [Tooltip("/ Guard = Stand in one place until the player breaks line of sight / Wanderer = walks around / Chase = when the soldier goes after the enemy")]
    [SerializeField] private SoldierState st;
    private SoldierState origianlst;
    [SerializeField] private GameObject target, tip, visionPoint, body;
    [SerializeField] private float rotationspeed, range;
    [SerializeField] private NavMeshAgent agent; 
    Vector3 targetDiretion, originalPos;
    Quaternion rotation, originalrot;

    [SerializeField] List<Vector3> wanderSpots = new List<Vector3>();
    [Tooltip("Distance to current wander spot before the player moves to next wander spot.")]
    [SerializeField] private float wanderSpotOffset = 1f, delayBeforeMove = 2, originalPosOFFSET = 1f;
    private float lastShot, ShootRate = .5f;
    int i = 0;
    bool changeDir = false;

   
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
        }
    }

    private void Start()
    {
        maxHealth = health;
        target = GameObject.FindWithTag("Player");
        playerController = target.GetComponent<FirstPersonController>();
        origianlst = st;
        originalPos = transform.position;
        originalrot = this.transform.rotation;
        var pos = this.transform.position;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        animator.SetFloat("Blend", agent.velocity.magnitude);
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
                Aim();
                Shoot();
                ChasePlayer();
                break;
            case (SoldierState.originalSpot):
                ReturnToOriginalSpot();
                break;
            case (SoldierState.retaliate):
                RetaliationAim();
                RetaliationShoot();
                RetaliationChasePlayer();
                break;

            //Doesnt work yet
            case (SoldierState.emergencyHeal): // 1 = got to destination, 0 = no destination, -1 = going to destination
                if (RunAway() == 1)
                {
                    HealAfterRunAway();
                }
                if (RunAway() == 0)
                {
                    st = SoldierState.retaliate; // no closest edge with cover from player = last stand fight to death
                }
                if (RunAway() == -1)
                {
                    RunAway();
                }
                break;

            default:
                break;
        }
        distanceToPlayer = Vector3.Distance(gameObject.transform.position, target.transform.position);
    }
    void Aim() //This is pointing the soldier towards the player as long as he is in range
    {
        float tempSpeed = rotationspeed;
        if (distanceToPlayer < range)
        {
            targetDiretion = target.transform.position - tip.transform.position;
            rotation = Quaternion.LookRotation(targetDiretion);
            body.transform.rotation = Quaternion.RotateTowards(body.transform.rotation, rotation, tempSpeed * Time.deltaTime * 180);
        }
    }
    
    void LineOfSightWithPlayer()
    {
        RaycastHit hit;
        Debug.DrawRay(visionPoint.transform.position, targetDiretion, Color.green);
        Physics.Raycast(visionPoint.transform.position, targetDiretion, out hit, range/1.2f);
        if (hit.collider != null)
        {
            if (hit.collider.tag == target.tag)
            {
                //Debug.Log("Player Detected");
                Invoke("StateChase", 2);
            }
        }
        else { }
    }

    void Shoot() //Shoots at the player
    {
        RaycastHit hit, hit2;
        Debug.DrawRay(tip.transform.position, targetDiretion, Color.red);
        var offsetx = 0;
        var offsety = 0;
        if (Vector3.Distance(tip.transform.position, target.transform.position) > range/2)
        {
            offsetx = Random.Range(-5, 5);
            offsety = Random.Range(0, 5);
        }
        if (Vector3.Distance(tip.transform.position, target.transform.position) > ((range / 3) * 2))
        {
            offsetx = Random.Range(-10, 10);
            offsety = Random.Range(0, 5);
        }
        Physics.Raycast(tip.transform.position, new Vector3(targetDiretion.x + offsetx, targetDiretion.y + offsety, targetDiretion.z), out hit, range);
        if (hit.collider != null)
        {
            if (Physics.Raycast(tip.transform.position, targetDiretion, out hit2, range)) //check line of sight
            {
                if (hit2.collider.tag == target.tag) //if player is in line of sight, shoot
                {
                    if (Time.time > ShootRate + lastShot)
                    {
                        var bt = Instantiate(BulletTrail, tip.transform.position, rotation);
                        bt.GetComponent<MoveForward>().origin = this.gameObject.transform.rotation;
                        bt.GetComponent<MoveForward>().target = hit.point;
                        //bt.GetComponent<MoveForward>().damage = Damage;
                        //Debug.Log("Player was shot, dealing damage.");
                        if (hit.collider.tag == target.tag)
                        {
                            target.GetComponent<FirstPersonController>().TakeDamage(Damage);
                        }
                        lastShot = Time.time;
                    }
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
       if (dist <= range/3)
       {
            agent.ResetPath();
       }
       else if ( dist < range &&  dist > range/1.25)
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

    void RetaliationAim() //This is pointing the soldier towards the player as long as he is in range
    {
        float tempSpeed = rotationspeed;

        targetDiretion = target.transform.position - transform.position;
        rotation = Quaternion.LookRotation(targetDiretion);
        body.transform.rotation = Quaternion.RotateTowards(body.transform.rotation, rotation, tempSpeed * Time.deltaTime * 180);

    }

    void RetaliationShoot()
    {
        RaycastHit hit, hit2;
        Debug.DrawRay(tip.transform.position, targetDiretion, Color.red);
        var offsetx = 0;
        var offsety = 0;
        if (Vector3.Distance(tip.transform.position, target.transform.position) > range / 2)
        {
            offsetx = Random.Range(-5, 5);
            offsety = Random.Range(0, 5);
        }
        if (Vector3.Distance(tip.transform.position, target.transform.position) > ((range / 3)*2))
        {
            offsetx = Random.Range(-10, 10);
            offsety = Random.Range(0, 5);
        }
        Physics.Raycast(tip.transform.position, new Vector3(targetDiretion.x + offsetx, targetDiretion.y + offsety, targetDiretion.z), out hit, range);
        if (hit.collider != null)
        {
            if (Physics.Raycast(tip.transform.position, targetDiretion, out hit2, range)) //check line of sight
            {
                if (hit2.collider.tag == target.tag) //if player is in line of sight, shoot
                {
                    if (Time.time > ShootRate + lastShot)
                    {
                        var bt = Instantiate(BulletTrail, tip.transform.position, rotation);
                        bt.GetComponent<MoveForward>().origin = this.gameObject.transform.rotation;
                        bt.GetComponent<MoveForward>().target = hit.point;
                        //bt.GetComponent<MoveForward>().damage = Damage;
                        //Debug.Log("Player was shot, dealing damage.");
                        if (hit.collider.tag == target.tag)
                        {
                            target.GetComponent<FirstPersonController>().TakeDamage(Damage);
                        }
                        lastShot = Time.time;
                    }
                }
            }
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

    int RunAway()
    {
        NavMeshHit hit;
        RaycastHit hitRC;
        if (NavMesh.FindClosestEdge(transform.position, out hit, NavMesh.AllAreas))
        {
            Physics.Raycast(tip.transform.position, targetDiretion, out hitRC);
            //if (hitRC.collider.tag != target.tag)
            //{
                agent.SetDestination(hit.position);
            //}
            if (this.transform.position == hit.position)
            {
                agent.ResetPath();
                
                return 1;
            }
        }
        else
        {
            return 0;
        }
        return -1;
    }

    void HealAfterRunAway()
    {
        if (Time.time > ShootRate + lastShot)
        {
            TakeDamage(-10);
            lastShot = Time.time;
        }
    }
}
