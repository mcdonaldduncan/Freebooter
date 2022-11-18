using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SoldierEnemyScript : MonoBehaviour, IDamageable
{
    [SerializeField]
    Animator animator;

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
    [SerializeField] private float wanderSpotOffset = 1f, delayBeforeMove = 2, originalPosOFFSET = 1f, wanderDistanceR, wanderDistanceL, wanderDistanceF, wanderDistanceB;
    private float lastShot, ShootRate = .5f;
    int i = 0;
    bool changeDir = false;

   
    public TrailRenderer BulletTrail;
    [SerializeField] private float Damage;

    public float Health { get { return health; } set { health = value; } }
    [SerializeField] private float health, maxHealth;
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
            //this.gameObject.GetComponent<CheckForDrops>().DropOrNot();
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        maxHealth = health;
        target = GameObject.FindWithTag("Player");
        origianlst = st;
        originalPos = transform.position;
        originalrot = this.transform.rotation;
        var pos = this.transform.position;
        wanderSpots.Add(new Vector3(pos.x + wanderDistanceR, pos.y,pos.z));
        wanderSpots.Add(new Vector3(pos.x - wanderDistanceL, pos.y, pos.z));
        wanderSpots.Add(new Vector3(pos.x, pos.y, pos.z + wanderDistanceF));
        wanderSpots.Add(new Vector3(pos.x, pos.y, pos.z - wanderDistanceB));
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        Debug.Log(st);
        switch (st)
        {
            case SoldierState.guard:
                Aim();
                LineOfSightWithPlayer();
                animator.SetTrigger("Idle");
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
    }
    void Aim() //This is pointing the soldier towards the player as long as he is in range
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
        RaycastHit hit;
        Debug.DrawRay(tip.transform.position, targetDiretion, Color.red);

        Physics.Raycast(tip.transform.position, targetDiretion, out hit, range);
        if (hit.collider != null)
        {
            if (hit.collider.tag == target.tag || hit.collider.tag == "Gun")
            {
                if (Time.time > ShootRate + lastShot)
                {
                    var bt = Instantiate(BulletTrail, tip.transform.position, rotation);
                    bt.GetComponent<MoveForward>().origin = this.gameObject.transform.rotation;
                    bt.GetComponent<MoveForward>().target = target;
                    //bt.GetComponent<MoveForward>().damage = Damage;
                    //Debug.Log("Player was shot, dealing damage.");
                    target.GetComponent<FirstPersonController>().TakeDamage(Damage);
                    lastShot = Time.time;
                }
            }
            //else 
            //{
            //    agent.SetDestination(target.transform.position);
            //    animator.SetTrigger("RunAndShoot");
            //}

        }
        if (Vector3.Distance(this.transform.position, target.transform.position) > range)
        {
            StateReturnOriginalSpot();
        }
    }

    void Wander()
    {
        if(i >= wanderSpots.Count){i=0;}
        if (Vector3.Distance(this.transform.position, wanderSpots[i]) < wanderSpotOffset)
        {
            if (changeDir == false)
            {
                Invoke("WaitBeforeWanderToNextSpot", delayBeforeMove);
                animator.SetTrigger("Idle");
                changeDir = true;
            }
        }
        else if (Vector3.Distance(this.transform.position, wanderSpots[i]) >= wanderSpotOffset)
        {
          agent.SetDestination(wanderSpots[i]);
          animator.SetTrigger("RunAndShoot");
          //Debug.Log(wanderSpots[i].transform.position);
        }
        
    }
    void WaitBeforeWanderToNextSpot()
    {
        if (i+1 >= wanderSpots.Count)
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
       if (dist <= range/3)
       {
            agent.ResetPath();
            animator.SetTrigger("Shoot");
       }
       else if ( dist < range &&  dist > range/1.25)
       {
            agent.SetDestination(target.transform.position);
            animator.SetTrigger("RunAndShoot");
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
        animator.SetTrigger("RunAndShoot");
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
        RaycastHit hit;
        Debug.DrawRay(tip.transform.position, targetDiretion, Color.red);

        Physics.Raycast(tip.transform.position, targetDiretion, out hit, range);
        if (hit.collider != null)
        {
            if (hit.collider.tag == target.tag || hit.collider.tag == "Gun")
            {
                if (Time.time > ShootRate + lastShot)
                {
                    var bt = Instantiate(BulletTrail, tip.transform.position, rotation);
                    bt.GetComponent<MoveForward>().origin = this.gameObject.transform.rotation;
                    bt.GetComponent<MoveForward>().target = target;
                    //bt.GetComponent<MoveForward>().damage = Damage;
                    //Debug.Log("Player was shot, dealing damage.");
                    target.GetComponent<FirstPersonController>().TakeDamage(Damage);
                    lastShot = Time.time;
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
            animator.SetTrigger("Shoot");
        }
        else if (dist > range)
        {
            agent.SetDestination(target.transform.position);
            animator.SetTrigger("RunAndShoot");
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
                animator.SetTrigger("RunAndShoot");
            //}
            if (this.transform.position == hit.position)
            {
                agent.ResetPath();
                animator.SetTrigger("Idle"); // there was no animation for healing with bandages or anything of that type
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
