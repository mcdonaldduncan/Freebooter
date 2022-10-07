using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SoldierEnemyScript : MonoBehaviour
{
    
    private enum SoldierState {guard, wanderer, chase, originalSpot};
    [Tooltip("/ Guard = Stand in one place until the player breaks line of sight / Wanderer = walks around / Chase = when the soldier goes after the enemy")]
    [SerializeField] private SoldierState st;
    [SerializeField] private SoldierState origianlst;
    [SerializeField] private GameObject target, tip, light, visionPoint;
    [SerializeField] private float rotationspeed, range;
    [SerializeField] private NavMeshAgent agent; 
    Vector3 targetDiretion, originalPos;
    Quaternion rotation, originalrot;

    [SerializeField] List<Transform> wanderSpots = new List<Transform>();
    [Tooltip("Distance to current wander spot before the player moves to next wander spot.")]
    [SerializeField] private float wanderSpotOffset = 1f, delayBeforeMove = 2, originalPosOFFSET = 1f;
    private float lastShot, ShootRate = .5f;
    int i = 0;
    bool changeDir = false;

    [SerializeField]
    private float health;

    public float Health { get { return health; } set { health = value; } }

    public void Damage(float damageTaken)
    {
        Health -= damageTaken;
        CheckForDeath();
    }

    public void CheckForDeath()
    {
        if (Health <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            return;
        }
    }

    private void Start()
    {
        origianlst = st;
        originalPos = transform.position;
        originalrot = this.transform.rotation;
    }
    // Update is called once per frame
    void Update()
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
    void Aim() //This is pointing the soldier towards the player as long as he is in range
    {
        float tempSpeed = rotationspeed;
        rotation = Quaternion.LookRotation(targetDiretion);
        if (Vector3.Distance(this.transform.position, target.transform.position) < range)
        {
            targetDiretion = target.transform.position - transform.position;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, new Quaternion(0, rotation.y, 0, rotation.w), tempSpeed * Time.deltaTime * rotationspeed);
        }
       
            // Lerp can be somewhat low performance because it starts dealing with extremely small increments at the end,
            // rotateTowards keeps the movement constant in degree/second
            
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
                    Debug.Log("Player was shot, dealing damage.");
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
        if(i >= wanderSpots.Count){i=0;}
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
          Debug.Log(wanderSpots[i].transform.position);
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
       if ( dist < range &&  dist > range/2)
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
}
