using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class Drone : MonoBehaviour, IDamageable, IEnemy
{
    [SerializeField] GameObject m_ProjectilePrefab;
    [SerializeField] private LayerMask layerMask = 0; // for wondering
    [SerializeField] private float minWaitTimeWander, maxWaitTimeWander, wonderDistanceRange; // wait timer for wandering

    float waitTimer;
    bool wanderMiniState; //state for wandering and wanderingIdle

    float radius = 5;
    bool dead = false;
    private enum SoldierState { guard, wanderer, chase, originalSpot, Relocating, Death, retaliate};
    [Tooltip("/ Guard = Stand in one place until the player breaks line of sight / Wanderer = walks around / Chase = when the soldier goes after the enemy")]
    [SerializeField] private SoldierState st;
    private SoldierState OriginalState;
    [SerializeField] private GameObject tip, light, visionPoint, body,SensorR,SensorL;
    [SerializeField] private float rotationspeed, range;
    [SerializeField] private NavMeshAgent agent;
    Vector3 targetDirection, originalPos;
    Quaternion rotation, originalrot;

    [Tooltip("Distance to current wander spot before the player moves to next wander spot.")]
    [SerializeField] private float wanderSpotOffset = 1f, delayBeforeMove = 2, originalPosOFFSET = 1f;
    private float lastShot;
    [SerializeField] private float ShootRate = .5f;

    public TrailRenderer BulletTrail;
    [SerializeField] private float Damage;

    public float Health { get { return health; } set { health = value; } }
    [SerializeField] private float health, maxHealth;

    float distanceToPlayer;

    string playerTag = "Player";

    public Vector3 StartingPosition { get { return m_StartingPosition; } set { m_StartingPosition = value; } }
    private Vector3 m_StartingPosition;

    bool shouldShoot => Time.time > ShootRate + lastShot;

    public void TakeDamage(float damageTaken)
    {
        if (st == SoldierState.guard || st == SoldierState.wanderer)
        {
            // do retaliate and chase do anything differently?
            st = SoldierState.chase;
        }
        Health -= damageTaken;
        CheckForDeath();
    }

    public void CheckForDeath()
    {
        if (Health <= 0)
        {
            st = SoldierState.Death;
            
            OnDeath();
        }
    }

    public void OnDeath()
    {
        if (distanceToPlayer <= LevelManager.Instance.Player.DistanceToHeal)
        {
            LevelManager.Instance.Player.Health += (LevelManager.Instance.Player.PercentToHeal * maxHealth);
        }
        LevelManager.CheckPointReached += OnCheckPointReached;
    }

    public void OnCheckPointReached()
    {
        LevelManager.PlayerRespawn -= OnPlayerRespawn;
    }

    public void OnPlayerRespawn()
    {
        if (!gameObject.activeSelf || st == SoldierState.Death)
        {
            Resetvalues();
            var script = body.GetComponent<OnDeathExplosion>();
            script.ResetVariables();
            gameObject.SetActive(true);
        }
    }

    void CycleAgent()
    {
        if (!agent.isOnNavMesh)
        {
            agent.enabled = false;
            agent.enabled = true;
        }
        else
        {
            agent.isStopped = true;
            agent.isStopped = false;
        }

        st = OriginalState;
        dead = false;
    }

    public void Resetvalues()
    {
        CycleAgent();
        agent.Warp(m_StartingPosition);
        Health = maxHealth;
    }

    private void Start()
    {
        m_StartingPosition = transform.position;
        LevelManager.PlayerRespawn += OnPlayerRespawn;
        OriginalState = st;
        originalPos = transform.position;
        originalrot = this.transform.rotation;
    }

    // Why are you using fixed update?
    // Update is called once per frame
    void FixedUpdate()
    {

        distanceToPlayer = Vector3.Distance(this.transform.position, LevelManager.Instance.Player.transform.position);

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
                // why do you have aim, shoot, chase and also retaliate aim, retaliate shoot, etc? dont they do the same thing?
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
        if (distanceToPlayer < range)
        {
            targetDirection = LevelManager.Instance.Player.transform.position - transform.position;
            rotation = Quaternion.LookRotation(targetDirection);
            body.transform.rotation = Quaternion.RotateTowards(body.transform.rotation, rotation, tempSpeed * Time.deltaTime * 180);
        }

    }

    void LineOfSightWithPlayer()
    {
        RaycastHit hit;
        //Debug.DrawRay(visionPoint.transform.position, targetDiretion, Color.green);
        Physics.Raycast(visionPoint.transform.position, targetDirection, out hit, range / 1.2f);
        if (hit.collider != null)
        {
            if (hit.collider.CompareTag(playerTag))
            {
                //Debug.Log("Player Detected");
                Invoke("StateChase", 2);
            }
        }
    }

    void Shoot() //Shoots at the player
    {
        if (!shouldShoot) return;

        GameObject newObj = ProjectileManager.Instance.TakeFromPool(m_ProjectilePrefab, tip.transform.position + body.transform.forward, out Projectile projectile);
        projectile.Launch(targetDirection);

        lastShot = Time.time;


        //RaycastHit hit, hit2;
        ////Debug.DrawRay(tip.transform.position, targetDiretion, Color.red);
        //var offsetx = 0;
        //var offsety = 0;
        //var offsetz = 0;
        //if (distanceToPlayer > range / 2)
        //{
        //    offsetx = Random.Range(-5, 5);
        //    offsety = Random.Range(0, 5);
        //    offsetz = Random.Range(-5, 5);

        //}
        //if (distanceToPlayer > ((range / 3) * 2))
        //{
        //    offsetx = Random.Range(-10, 10);
        //    offsety = Random.Range(0, 5);
        //    offsetz = Random.Range(-10, 10);
        //}
        //// Why dont you do this same step for z? Our game is 3 dimensional, if you are on the same x plane they would only have a chance to miss on y and given the player is tall missing slightly in y will probably still hit
        //Physics.Raycast(tip.transform.position, new Vector3(targetDiretion.x + offsetx, targetDiretion.y + offsety, targetDiretion.z + offsetz), out hit, range);
        //if (hit.collider != null)
        //{

        //    if (Physics.Raycast(tip.transform.position, targetDiretion, out hit2, range)) //check line of sight
        //    {
        //        if (hit2.collider.CompareTag(playerTag)) //if player is in line of sight, shoot
        //        {
        //            if (Time.time > ShootRate + lastShot)
        //            {
        //                var bt = Instantiate(BulletTrail, tip.transform.position, rotation);
        //                bt.GetComponent<MoveForward>().origin = this.gameObject.transform.rotation;
        //                bt.GetComponent<MoveForward>().target = hit.point;
        //                //bt.GetComponent<MoveForward>().damage = Damage;
        //                //Debug.Log("Player was shot, dealing damage.");
        //                if (hit.collider.CompareTag(playerTag))
        //                {
        //                    // you do not need to get the component out of the player, it is already an idamageable
        //                    LevelManager.Instance.Player.TakeDamage(Damage);
        //                }
        //                lastShot = Time.time;
        //            }
        //        }
        //    }
        //}
        if (distanceToPlayer > range)
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
        if (distanceToPlayer < range && distanceToPlayer > range / 2)
        {
            //var destinationX = (LevelManager.Instance.Player.transform.position.x + (distanceToPlayer / 2));
            //var destinationZ = (LevelManager.Instance.Player.transform.position.z + (distanceToPlayer / 2));
            //agent.SetDestination(new Vector3(destinationX,LevelManager.Instance.Player.transform.position.y,destinationZ));
            Vector3 FromPlayerToAgent = LevelManager.Instance.Player.transform.position - transform.position;

            agent.SetDestination(transform.position + FromPlayerToAgent.normalized * radius);
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
        st = OriginalState;
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
        if (!dead)
        {
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
    }

    bool Right(Vector3 pos, float dist)
    {
        RaycastHit hit;
        //Debug.DrawRay(SensorR.transform.position, Vector3.right, Color.blue);
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
        //Debug.DrawRay(SensorL.transform.position, -Vector3.right, Color.blue);
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

        targetDirection = LevelManager.Instance.Player.transform.position - transform.position;
        rotation = Quaternion.LookRotation(targetDirection);
        body.transform.rotation = Quaternion.RotateTowards(body.transform.rotation, rotation, tempSpeed * Time.deltaTime * 180);

    }

    void RetaliationShoot()
    {
        RaycastHit hit,hit2;
        //Debug.DrawRay(tip.transform.position, targetDiretion, Color.red);
        var offsetx = 0;
        var offsety = 0;
        if (distanceToPlayer > range / 2)
        {
            offsetx = Random.Range(-5, 5);
            offsety = Random.Range(0, 5);
        }
        if (distanceToPlayer > ((range / 3) * 2))
        {
            offsetx = Random.Range(-10, 10);
            offsety = Random.Range(0, 5);
        }
        Physics.Raycast(tip.transform.position, new Vector3(targetDirection.x + offsetx, targetDirection.y + offsety, targetDirection.z), out hit, range);
        if (hit.collider != null)
        {
            if (Physics.Raycast(tip.transform.position, targetDirection, out hit2, range)) //check line of sight
            {
                if (hit2.collider.CompareTag(playerTag)) //if player is in line of sight, shoot
                {
                    if (Time.time > ShootRate + lastShot)
                    {
                        var bt = Instantiate(BulletTrail, tip.transform.position, rotation);
                        bt.GetComponent<MoveForward>().origin = this.gameObject.transform.rotation;
                        bt.GetComponent<MoveForward>().target = hit.point;
                        //bt.GetComponent<MoveForward>().damage = Damage;
                        //Debug.Log("Player was shot, dealing damage.");
                        if (hit.collider.CompareTag(playerTag))
                        {
                            LevelManager.Instance.Player.TakeDamage(Damage);
                        }
                        lastShot = Time.time;
                    }
                }
            }
        }
    }

    void RetaliationChasePlayer()
    {
        if (distanceToPlayer > range)
        {
            Vector3 PointingToPlayer = LevelManager.Instance.Player.transform.position - transform.position;

            agent.SetDestination(transform.position + PointingToPlayer.normalized * radius);
        }

    }
}
