using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnDeathExplosion : MonoBehaviour
{
    [SerializeField] GameObject Body;
    [SerializeField] GameObject prefab;
    [SerializeField] GameObject explosionparticle;
    [SerializeField] float m_Damage;
    [SerializeField] float m_Radius;
    [SerializeField] float blinkrate = .3f;

    float m_LastBlinkTime;

    Rigidbody rb;
    Renderer m_Renderer;
    Collider m_Collider;

    bool dead = false;
    bool landed = false; 
    bool explosion = false;
    
    int deathFrames;

    Collider[] m_HitsSaved = new Collider[10];

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        m_Renderer = GetComponent<Renderer>();
        
        
    }

    private void Update()
    {
        if (!dead) return;
        Blink();
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

    void Blink()
    {
        if (Time.time > blinkrate + m_LastBlinkTime && m_Renderer.material.color == Color.white)
        {
            m_Renderer.material.color = Color.red;
            m_LastBlinkTime = Time.time;
        }
        if (Time.time > blinkrate + m_LastBlinkTime && m_Renderer.material.color == Color.red)
        {
            m_Renderer.material.color = Color.white;
            m_LastBlinkTime = Time.time;
        }
    }

    void FallOnDeath()
    {
        if (deathFrames == 0)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            m_Collider.enabled = true;
            rb.useGravity = true;
            Vector3 explosiveForce = new Vector3(Random.Range(-5f, 5f), Random.Range(3f, 7f), Random.Range(-5f, 5f));
            rb.AddForce(explosiveForce, ForceMode.Impulse);
        }
        deathFrames++;
    }

    public void ResetVariables()
    {
        dead = false;
        landed = false;
        explosion = false;
        deathFrames = 0;
        Destroy(rb);
        transform.position = transform.parent.position;
        transform.rotation = transform.parent.rotation;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!dead) return;

        if (collision.collider.gameObject.layer != 9 && collision.collider.gameObject.layer != 8) // Make sure the collision is with something other than enemy because it would collide with itself since the parent object has a collider
        {
            landed = true;
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
                damageable.TakeDamage(m_Damage);
                m_HitsSaved[i] = null;
            }
        }
    }

}
