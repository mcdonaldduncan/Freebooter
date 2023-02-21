using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GrenadeBehavior : MonoBehaviour, IPoolable
{
    [SerializeField] private float timeBeforeExplosion;
    [SerializeField] private float explosionRadius;
    [SerializeField] private float explosionDamage;
    [SerializeField] private GameObject grenadeVFX;
    [SerializeField] private GameObject m_Prefab;

    private bool ShouldExplode => timerStarted && startTime + timeBeforeExplosion <= Time.time && !exploded;

    public GameObject Prefab { get => m_Prefab; set => m_Prefab = value; }

    private float startTime;
    private AudioSource grenadeAudioSource;
    private bool explosionPlayed = false;
    private bool timerStarted = false;
    private Renderer grenadeRenderer;
    private Rigidbody grenadeRB;
    private GrenadeGun grenadeGun;
    private bool collided = false;
    private bool exploded = false;

    // Start is called before the first frame update
    //void Start()
    //{
    //    grenadeRB = GetComponent<Rigidbody>();
    //}

    private void OnEnable()
    {
        grenadeGun = LevelManager.Instance.Player.GetComponentInChildren<GrenadeGun>();
        transform.SetParent(null, true);
        startTime = Time.time;
        grenadeAudioSource = GetComponent<AudioSource>();
        grenadeRenderer = GetComponent<Renderer>();
        grenadeGun.remoteDetonationEvent += Explode;
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
            grenadeRB.constraints = RigidbodyConstraints.FreezeAll;
            collided = true;
        }
    }

    public void Launch(Vector3 direction)
    {
        grenadeRB = GetComponent<Rigidbody>();
        grenadeRB.constraints = RigidbodyConstraints.None;
        grenadeRB.AddForce(direction);
    }

    private void Explode()
    {
        exploded = true;
        var explosion = ProjectileManager.Instance.TakeFromPool(grenadeVFX, transform.position);

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hit in colliders)
        {
            if (hit != null)
            {
                var damageableTarget = hit.transform.GetComponent<IDamageable>();
                if (damageableTarget != null && hit is not CharacterController)
                {
                    try
                    {
                        damageableTarget.TakeDamage(explosionDamage);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
        }
        grenadeGun.remoteDetonationEvent -= Explode;
        ProjectileManager.Instance.ReturnToPool(gameObject);
    }

    private void OnDestroy()
    {
        //grenadeGun.remoteDetonationEvent -= Explode;
    }
}
