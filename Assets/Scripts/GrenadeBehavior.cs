using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GrenadeBehavior : MonoBehaviour
{
    [SerializeField] private float timeBeforeExplosion;
    [SerializeField] private float explosionRadius;
    [SerializeField] private float explosionDamage;
    [SerializeField] private AudioClip explosionSound;

    private bool ShouldExplode => startTime + timeBeforeExplosion <= Time.time;

    private float startTime;
    private AudioSource grenadeAudioSource;
    private bool soundPlayed = false;
    private Renderer grenadeRenderer;

    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;
        grenadeAudioSource = GetComponent<AudioSource>();
        grenadeRenderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    //void Update()
    //{
    //    if (ShouldExplode)
    //    {
    //        Explode();
    //    }
    //}

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag != "Player")
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (!soundPlayed)
        {
            grenadeAudioSource.PlayOneShot(explosionSound);
            soundPlayed = true;
        }
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
}
