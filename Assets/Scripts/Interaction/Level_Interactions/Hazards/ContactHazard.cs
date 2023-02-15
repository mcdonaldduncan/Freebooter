using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald
public class ContactHazard : MonoBehaviour
{
    [SerializeField] bool m_IsLive = true;
    [SerializeField] bool m_InstantKill;
    [SerializeField] bool m_IsTrigger;
    [SerializeField] float m_DamageAmount;

    private void OnEnable()
    {
        GetComponent<Collider>().isTrigger = m_IsTrigger;
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleDamage(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleDamage(other);
    }

    private void HandleDamage(Collider collider)
    {
        if (!m_IsLive) return;

        IDamageable damageable = collider.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            if (m_InstantKill) damageable.TakeDamage(damageable.Health + 1);
            else damageable.TakeDamage(m_DamageAmount);
        }
        else
        {
            Destroy(collider.gameObject);
        }
    }
}
