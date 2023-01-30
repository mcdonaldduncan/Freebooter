using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CriticalHitBox : MonoBehaviour, IDamageable
{
    
    private IDamageable damageable;

    [Header("SetUp")]
    public GameObject ObjectWithIdamageble;
    public ParticleSystem critParticle;

    [Header("Crit Damage Variable")]
    public float CriticalDamageMultiplier = 2;

    public float Health { get => damageable.Health; set => damageable.Health = value; }

    public void CheckForDeath()
    {
    }

    public void TakeDamage(float damageTaken)
    {
        playCriticalVFX();
        Debug.Log("Crit");
        Debug.Log("damage enhanced to " +damageTaken*CriticalDamageMultiplier);
        damageable.TakeDamage(damageTaken*CriticalDamageMultiplier);
    }

    public void playCriticalVFX()
    {
        Debug.Log("Critical VFX");
        critParticle.Play();
    }

    void Start()
    {
        damageable = ObjectWithIdamageble.GetComponent<IDamageable>();    
    }
}
