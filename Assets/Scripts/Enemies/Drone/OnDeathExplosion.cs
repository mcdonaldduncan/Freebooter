using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnDeathExplosion : MonoBehaviour
{
    [SerializeField] GameObject explosionparticle;
    [SerializeField] float m_Damage;
    [SerializeField] float m_Radius;

    Rigidbody rb;
    Collider m_Collider;

    bool dead = false;
    
    int deathFrames;

    Collider[] m_HitsSaved = new Collider[10];

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    private void Update()
    {
        if (!dead) return;
        FallOnDeath();
    }

    public void StartDeathSequence()
    {
        dead = true;
    }

    private void OnEnable()
    {
        m_Collider = GetComponent<Collider>();
        m_Collider.enabled = false;
    }

    void FallOnDeath()
    {
        if (deathFrames == 0)
        {
            rb.isKinematic = false;
            m_Collider.enabled = true;
            rb.useGravity = true;
            Vector3 explosiveForce = new Vector3(Random.Range(-5f, 5f), Random.Range(3f, 7f), Random.Range(-5f, 5f));
            rb.constraints = RigidbodyConstraints.None;
            rb.AddForce(explosiveForce, ForceMode.Impulse);
        }
        deathFrames++;
    }

    public void ResetVariables()
    {
        dead = false;
        deathFrames = 0;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        transform.position = transform.parent.position;
        transform.rotation = transform.parent.rotation;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!dead) return;

        if (collision.collider.gameObject.layer != 9 && collision.collider.gameObject.layer != 8) // Make sure the collision is with something other than enemy because it would collide with itself since the parent object has a collider
        {
            Instantiate(explosionparticle, transform.position, Quaternion.identity);
            ExplosionDamage();
            transform.parent.gameObject.SetActive(false);
        }
    }

    void ExplosionDamage()
    {
        var hits = Physics.OverlapSphereNonAlloc(transform.position, m_Radius, m_HitsSaved);
        for (int i = 0; i < hits; i++)
        {
            if (m_HitsSaved[i] == null || !m_HitsSaved[i].gameObject.activeSelf) continue;
            
            if (m_HitsSaved[i].TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(m_Damage, HitBoxType.normal);
                m_HitsSaved[i] = null;
            }
        }
    }

}
