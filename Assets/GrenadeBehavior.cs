using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GrenadeBehavior : MonoBehaviour
{
    [SerializeField] private float timeBeforeExplosion;
    [SerializeField] private float explosionRadius;
    [SerializeField] private float explosionDamage;

    private bool ShouldExplode => startTime + timeBeforeExplosion <= Time.time;

    private float startTime;
    
    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (ShouldExplode)
        {
            Explode();
        }
    }

    private void Explode()
    {
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

        Destroy(gameObject);
    }
}
