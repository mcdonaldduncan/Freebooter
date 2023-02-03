using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialHitBoxScript : MonoBehaviour, IDamageable, IPoolable
{
    private IDamageable damageable;
   

    [Header("SetUp")] 
    public GameObject m_Prefab;
    public Transform VFXTransform;

    [Header("HitBox Type")]
    public HitBoxType hitboxtype;

    [Header("CriticalVFX")]
    public GameObject critVFX;

    [Header("ArmoredVFX")]
    public GameObject armorVFX;

    [Header("ShieldVFX")]
    public GameObject shieldVFXBlue;
    public GameObject shieldVFXYellow;
    public GameObject shieldVFXRed;

    [Header("Crit Damage Variable")]
    public float CriticalDamageMultiplier = 2;

    [Header("Armored Variable")]
    public float ArmorDamageReductionMultiplier = .5f;

    [Header("Shield Variable")]
    public GameObject ShieldGameObject;
    private IDamageable shieldDamageable;
    private float shieldMaxHp;

    public float Health { get => damageable.Health; set => damageable.Health = value; }

    public GameObject Prefab { get => m_Prefab; set => m_Prefab = value; }

    public void CheckForDeath()
    {
    }

    public void TakeDamage(float damageTaken)
    {
        if (hitboxtype == HitBoxType.critical)
        {
            playVFX(critVFX, VFXTransform.position);
            damageable.TakeDamage(damageTaken * CriticalDamageMultiplier);
        }
        if (hitboxtype == HitBoxType.armored)
        {
            playVFX(armorVFX, VFXTransform.position);
            damageable.TakeDamage(damageTaken * ArmorDamageReductionMultiplier);
        }
        if (hitboxtype == HitBoxType.shield)
        {
            if (ShieldGameObject != null)
            {
            if (shieldDamageable.Health > shieldMaxHp / 2) // if it is greater then 1/2
            {
                shieldDamageable.TakeDamage(damageTaken);
                playVFX(shieldVFXBlue, VFXTransform.position);
            }
            if (shieldDamageable.Health < shieldMaxHp / 2 && shieldDamageable.Health > shieldMaxHp / 4) //if between 1/2 and 1/4
            {
                shieldDamageable.TakeDamage(damageTaken);
                playVFX(shieldVFXYellow, VFXTransform.position);
            }
            if (shieldDamageable.Health < shieldMaxHp / 4) //if lower then 1/4
            {
                shieldDamageable.TakeDamage(damageTaken);
                playVFX(shieldVFXRed, VFXTransform.position);
            }
            }
        } 
    }

    public void playVFX(GameObject vfx, Vector3 position)
    {
        ProjectileManager.Instance.TakeFromPool(vfx, position);
    }

    void Start()
    {
        damageable = Prefab.GetComponent<IDamageable>();
        if (ShieldGameObject != null)
        {
            shieldDamageable = ShieldGameObject.GetComponent<IDamageable>();
            var shieldScript = ShieldGameObject.GetComponent<Shield>();
            shieldMaxHp = shieldScript._maxhealth;
        }
    }
}
public enum HitBoxType
{
    critical,
    armored,
    shield
}
