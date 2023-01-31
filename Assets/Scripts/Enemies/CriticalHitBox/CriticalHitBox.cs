using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CriticalHitBox : MonoBehaviour, IDamageable, IPoolable
{
    private IDamageable damageable;

    [Header("SetUp")] 
    public GameObject m_Prefab;
    public GameObject CritVFX;
    public Transform CritTFXPosition;

    [Header("Crit Damage Variable")]
    public float CriticalDamageMultiplier = 2;

    public float Health { get => damageable.Health; set => damageable.Health = value; }

    public GameObject Prefab { get => m_Prefab; set => m_Prefab = value; }

    public void CheckForDeath()
    {
    }

    public void TakeDamage(float damageTaken)
    {
        playCriticalVFX();
        damageable.TakeDamage(damageTaken*CriticalDamageMultiplier);
    }

    public void playCriticalVFX()
    {
        ProjectileManager.Instance.TakeFromPool(CritVFX, CritTFXPosition.transform.position);
    }

    void Start()
    {
        damageable = Prefab.GetComponent<IDamageable>();
    }
}
