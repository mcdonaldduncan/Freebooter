using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GrenadeBehavior : MonoBehaviour
{
    [SerializeField] private float timeBeforeExplosion;
    [SerializeField] private float explosionRadius;
    [SerializeField] private float explosionDamage;
    [SerializeField] private GameObject grenadeVFX;

    private bool ShouldExplode => timerStarted && startTime + timeBeforeExplosion <= Time.time;

    private float startTime;
    private AudioSource grenadeAudioSource;
    private bool explosionPlayed = false;
    private bool timerStarted = false;
    private Renderer grenadeRenderer;
    private Rigidbody grenadeRB;
    private GrenadeGun grenadeGun;
    private bool collided = false;

    // Start is called before the first frame update
    void Start()
    {
        grenadeRB = GetComponent<Rigidbody>();
        grenadeGun = transform.parent.GetComponent<GrenadeGun>();
        transform.SetParent(null, true);
        startTime = Time.time;
        grenadeAudioSource = GetComponent<AudioSource>();
        grenadeRenderer = GetComponent<Renderer>();
    }

    private void Update()
    {
        if (ShouldExplode)
        {
            Explode();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag != "Player" && !collided)
        {
            grenadeGun.remoteDetonationEvent += Explode;
            grenadeRB.constraints = RigidbodyConstraints.FreezeAll;
            startTime = Time.time;
            timerStarted = true;
            collided = true;
        }
    }

    private void Explode()
    {
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

        grenadeRenderer.enabled = false;
        Destroy(gameObject, 1f);
    }

    private void OnDestroy()
    {
        grenadeGun.remoteDetonationEvent -= Explode;
    }
}
