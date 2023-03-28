using Unity.VisualScripting;
using UnityEngine;

sealed class Turret : NewAgentBase
{
    [Header("Turret Body")]
    [SerializeField] Transform m_Body;

    [Header("Attack Delay")]
    [SerializeField] float m_AttackDelay;

    TurretState m_TurretState;

    public override Transform TrackingTransform => m_Body;

    private void Awake()
    {
        AwakeSetup();
    }

    private void OnEnable()
    {
        EnableSetup();
    }

    void Start()
    {
        StartSetup();
    }

    void Update()
    {
        switch (m_TurretState)
        {
            case TurretState.GUARD:
                m_Tracking.LimitedTrackTarget();
                if (m_Tracking.CheckFieldOfView()) Invoke(nameof(SetAttackState), m_AttackDelay);
                if (IsInCombat) HandleCombatStateChange();
                break;
            case TurretState.ATTACK:
                m_Tracking.LimitedTrackTarget();
                if (m_Tracking.CheckFieldOfView()) m_Shooting.Shoot(true);
                if (!m_Tracking.InRange) m_TurretState = TurretState.GUARD;
                if (!IsInCombat) HandleCombatStateChange();
                break;
            default:
                break;
        }
    }

    void SetAttackState()
    {
        m_TurretState = TurretState.ATTACK;
    }
}

public enum TurretState
{
    GUARD,
    ATTACK
}

/*
public class Turret : MonoBehaviour, IDamageable, IEnemy
{
    [SerializeField] GameObject m_ProjectilePrefab;
    [SerializeField] private GameObject body, tip, light;
    [SerializeField] private float rotationSpeed, range;
    private enum TurretState {LookingForTarget, ShootTarget};
    [SerializeField] private TurretState state;
    private enum TurretRotationType {full, half}; [Tooltip("Full = 360, Half = 180")]
    [SerializeField] private TurretRotationType rotationType;
    [SerializeField] private float delayBeforeFirstShot;

    Vector3 targetDirection;
    Quaternion rotation;

    float distanceToPlayer;

    private float lastShot;
    [SerializeField] private float ShootRate = .5f;

    private float lastValidY = 0f;
   

    public TrailRenderer BulletTrail;
    [SerializeField] private float Damage;

    public float Health { get { return health; } set { health = value; } }
    [SerializeField] private float health;

    [SerializeField] private float maxHealth = 75;

    string playerTag = "Player";

    bool shouldShoot => Time.time > ShootRate + lastShot;

    public Vector3 StartingPosition { get { return m_StartingPosition; } set { m_StartingPosition = value; } } // turret doesnt need this but its necessery to put this here for the interface
    private Vector3 m_StartingPosition; // turret doesnt need this but its necessery to put this here for the interface

    public void TakeDamage(float damageTaken)
    {
        Health -= damageTaken;
        CheckForDeath();
    }

    public void CheckForDeath()
    {
        if (Health <= 0)
        {
            if (distanceToPlayer <= LevelManager.Instance.Player.DistanceToHeal)
            {
                LevelManager.Instance.Player.Health += (LevelManager.Instance.Player.PercentToHeal * maxHealth);
            }
            OnDeath();
        }
    }

    public void OnDeath()
    {
        if (distanceToPlayer <= LevelManager.Instance.Player.DistanceToHeal)
        {
            LevelManager.Instance.Player.Health += (LevelManager.Instance.Player.PercentToHeal * maxHealth);
        }
        CycleAgent();
        gameObject.SetActive(false);
        LevelManager.CheckPointReached += OnCheckPointReached;
    }

    public void OnCheckPointReached()
    {
        LevelManager.PlayerRespawn -= OnPlayerRespawn;
    }

    public void OnPlayerRespawn()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        CycleAgent();
        Health = maxHealth;
    }

    void CycleAgent()
    {
        state = TurretState.LookingForTarget;
    }

    void Start()
    {
       LevelManager.PlayerRespawn += OnPlayerRespawn;
       state = TurretState.LookingForTarget;
       rotationType = TurretRotationType.full;
    }

    void Update()
    {
        distanceToPlayer = Vector3.Distance(gameObject.transform.position, LevelManager.Instance.Player.transform.position);
        switch (state) //handles what the turret shhould be doing at cetain states.
        {
            case TurretState.LookingForTarget:
                Aim();
                LookForLineOfSight();
                light.GetComponent<Light>().color = Color.green;
                //Debug.DrawRay();
                break;
            case TurretState.ShootTarget:
                Aim();
                Shoot();
                light.GetComponent<Light>().color = Color.yellow;

                break;
            default:
                break;
        }
    }
    void Aim() //This is pointing the torret towards the player as long as he is in range
    {
        float tempSpeed = rotationSpeed;
        if (distanceToPlayer < range)
        { 
            targetDirection = LevelManager.Instance.Player.transform.position - transform.position;

            if (rotationType == TurretRotationType.full)
            {
                rotation = Quaternion.LookRotation(targetDirection);
            }
            if (rotationType == TurretRotationType.half)
            {
                Vector3 tempRotation = Quaternion.LookRotation(targetDirection).eulerAngles;
                if (tempRotation.y > 270 && tempRotation.y > 180)
                {
                    tempSpeed *= .1f;
                    tempRotation.y = 0.5f;
                }
                if (tempRotation.y <= 270 && tempRotation.y > 180)
                {
                    tempSpeed *= .1f;
                    tempRotation.y = 179.5f;
                }

                rotation = Quaternion.Euler(tempRotation);
            }
            body.transform.rotation = Quaternion.RotateTowards(body.transform.rotation, rotation, tempSpeed * Time.deltaTime * 180);
        }
       
    }
    void LookForLineOfSight() //Shoots raycasts at the player and if it hits the player then it has line of sight
    {
        RaycastHit hit;
        //Debug.DrawRay(tip.transform.position, targetDiretion, Color.green);
        Physics.Raycast(tip.transform.position, targetDirection, out hit, range);
        if (hit.collider != null)
        {
            if (hit.collider.CompareTag(playerTag))
            { 
                Invoke("StateShootTarget",delayBeforeFirstShot);
            }
        }
        else { }
        // why is there an empty else
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
        //            // The player tag never changes, why get the tag from the player each time you check
        //            if (Time.time > ShootRate + lastShot)
        //            {
        //                var bt = Instantiate(BulletTrail, tip.transform.position, rotation);
        //                bt.GetComponent<MoveForward>().origin = this.gameObject.transform.rotation;
        //                bt.GetComponent<MoveForward>().target = hit.point;
        //                //bt.GetComponent<MoveForward>().damage = Damage;
        //                //Debug.Log("Player was shot, dealing damage.");
        //                //Use compare tag not equivalency
        //                if (hit.collider.CompareTag(playerTag))
        //                {
        //                    LevelManager.Instance.Player.TakeDamage(Damage);
        //                }
        //                lastShot = Time.time;
        //            }
        //        }
        //    }
        //}
    }
    void StateLookingForTarget() //swaps state to looking for target
    {
        state = TurretState.LookingForTarget;
    }
    void StateShootTarget() //swaps state to shooting target
    {
        state = TurretState.ShootTarget;
    }
}
*/