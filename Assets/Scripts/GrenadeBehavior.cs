using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GrenadeBehavior : MonoBehaviour, IPoolable, IDamageTracking
{
    [SerializeField] private float timeBeforeExplosion;
    [SerializeField] private float explosionRadius;
    [SerializeField] private float explosionDamage;
    [SerializeField] private GameObject grenadeVFX;
    [SerializeField] private GameObject m_Prefab;
    [SerializeField] private float explosionShakeDuration;
    [SerializeField] private float explosionShakeMagnitude;
    [SerializeField] private float explosionShakeDampen;

    private bool ShouldExplode => timerStarted && startTime + timeBeforeExplosion <= Time.time && !exploded;

    public GameObject Prefab { get => m_Prefab; set => m_Prefab = value; }
    public PlayerDamageDelegate DamageDealt { get; set; }

    private float startTime;
    private float hitStopDuration;
    private AudioSource grenadeAudioSource;
    private bool timerStarted = false;
    private Renderer grenadeRenderer;
    private Rigidbody grenadeRB;
    private GrenadeGun grenadeGun;
    private bool collided = false;
    private bool exploded = false;
    private Vector3 stickPosition;

    // Start is called before the first frame update
    //void Start()
    //{
    //    grenadeRB = GetComponent<Rigidbody>();
    //}

    private void OnEnable()
    {
        grenadeGun = LevelManager.Instance.Player.GetComponentInChildren<GrenadeGun>();
        //hitStopDuration = grenadeGun.HitStopDuration;
        transform.SetParent(null, true);
        startTime = Time.time;
        grenadeAudioSource = GetComponent<AudioSource>();
        grenadeRenderer = GetComponent<Renderer>();
        grenadeGun.remoteDetonationEvent += Explode;
        LevelManager.PlayerRespawn += OnPlayerRespawn;
        startTime = Time.time;
        timerStarted = true;
        collided = false;
    }

    private void Update()
    {
        if (ShouldExplode)
        {
            //Explode();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag != "Player" && !collided)
        {
            if ((collision.gameObject.TryGetComponent(out IDamageable damageable) || collision.gameObject.GetComponentInParent<IDamageable>() != null) && !collision.gameObject.name.Contains("barrel"))
            {
                Explode();
                return;
            }
            grenadeRB.constraints = RigidbodyConstraints.FreezeAll;
            stickPosition = gameObject.transform.position;
            transform.SetParent(collision.transform);
            collided = true;
        }
    }

    public void Launch(Vector3 direction)
    {
        //get the rigidbody of the grenade
        grenadeRB = GetComponent<Rigidbody>();

        //reset the constraints of the grenade, so that it doesn't freeze after being respawned from object pool
        grenadeRB.constraints = RigidbodyConstraints.None;

        //Add force to the launched grenade
        grenadeRB.AddForce(direction);

        //Add torque for a little stylish spinning :)
        grenadeRB.AddTorque(direction * 0.01f);
    }

    private void Explode()
    {
        exploded = true;
        var explosion = ProjectileManager.Instance.TakeFromPool(grenadeVFX, transform.position);

        //Get all colliders within the radius of the grenade explosion
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        if (colliders.Length > 0)
        {
            //little bit of camera shake for VFX
            CameraShake.ShakeCamera(explosionShakeDuration, explosionShakeMagnitude, explosionShakeDampen);
        }

        // We really do not need to be using try catch, it is innefficient
        //Damage each object that is an IDamageable
        foreach (var hit in colliders)
        {
            if (hit != null)
            {
                var damageableTarget = hit.transform.GetComponent<IDamageable>();
                if (damageableTarget != null && hit is not CharacterController)
                {
                    try
                    {
                        damageableTarget.TakeDamage(explosionDamage, HitBoxType.normal, stickPosition);
                        DamageDealt?.Invoke(explosionDamage);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
        }

        //Unsubscribe from the detonation event (subscribed in OnEnable)
        grenadeGun.remoteDetonationEvent -= Explode;
        transform.SetParent(null, true);

        //Return the grenade to the object pool
        ProjectileManager.Instance.ReturnToPool(gameObject);
    }

    private void OnPlayerRespawn()
    {
        //Unsubscribe from the detonation event (subscribed in OnEnable)
        grenadeGun.remoteDetonationEvent -= Explode;
        transform.SetParent(null, true);

        //Return the grenade to the object pool
        ProjectileManager.Instance.ReturnToPool(gameObject);
    }

    private void OnDestroy()
    {
        grenadeGun.remoteDetonationEvent -= Explode;
    }
}
