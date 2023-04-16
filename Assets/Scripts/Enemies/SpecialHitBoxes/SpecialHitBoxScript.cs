using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpecialHitBoxScript : MonoBehaviour, IDamageable
{
    private IDamageable damageable;
    private ParticleSystem m_ParticleSystem;

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

    public float _health;
    public float maxHealth;
    public float Health { get => _health; set => _health = value; }

    public GameObject DamageTextPrefab { get => damageable.DamageTextPrefab; }
    public Transform TextSpawnLocation { get => damageable.TextSpawnLocation; }
    public float FontSize { get => damageable.FontSize; } 

    public GameObject Prefab { get => m_Prefab; set => m_Prefab = value; }
    public TextMeshPro Text { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public void CheckForDeath()
    {
        if (_health < 0)
        {
            this.gameObject.SetActive(false);
        }
    }

    /// is this even working? Health only gets changed if the hitbox is a shield
    public void TakeDamage(float damageTaken, HitBoxType hitbox, Vector3 hitPoint = default(Vector3))
    {
        if (hitboxtype == HitBoxType.normal)
        {
            damageable.TakeDamage(damageTaken, HitBoxType.normal, hitPoint);
        }
        if (hitboxtype == HitBoxType.critical)
        {
            PlayVFX(critVFX, VFXTransform.position);
            damageable.TakeDamage(damageTaken * CriticalDamageMultiplier, HitBoxType.critical, hitPoint);
        }
        if (hitboxtype == HitBoxType.armored)
        {
            PlayVFX(armorVFX, VFXTransform.position);
            damageable.TakeDamage(damageTaken * ArmorDamageReductionMultiplier, HitBoxType.armored, hitPoint);
        }
        if (hitboxtype == HitBoxType.shield)
        {
            if (ShieldGameObject == null) return;

            if (_health > maxHealth / 2) // if it is greater then 1/2
            {
                PlayVFX(shieldVFXBlue, VFXTransform.position);
            }
            if (_health < maxHealth / 2 && _health > maxHealth / 4) //if between 1/2 and 1/4
            {
                PlayVFX(shieldVFXYellow, VFXTransform.position);
            }
            if (_health < maxHealth / 4) //if lower then 1/4
            {
                PlayVFX(shieldVFXRed, VFXTransform.position);
            }
            _health -= damageTaken;
            CheckForDeath();
        } 
    }

    public void PlayVFX(GameObject vfx, Vector3 position)
    {
        ProjectileManager.Instance.TakeFromPool(vfx, position);
    }

    void Start()
    {
        VFXTransform = this.transform;
        damageable = Prefab.GetComponent<IDamageable>();
        _health = maxHealth;
        m_ParticleSystem = GetComponentInChildren<ParticleSystem>();
    }

    // what do you even think you are doing here?
    //private void Update()
    //{
    //    shieldVFXBlue.transform.position = VFXTransform.position;
    //    shieldVFXYellow.transform.position = VFXTransform.position;
    //    shieldVFXRed.transform.position = VFXTransform.position;
    //}
}

public enum HitBoxType
{
    normal,
    critical,
    armored,
    shield
}
