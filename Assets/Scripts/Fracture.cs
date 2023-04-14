using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;

public class Fracture : MonoBehaviour, IDamageable
{
    [SerializeField] private float m_health;
    [SerializeField] private float m_timeToDespawn;
    [SerializeField] private float m_breakForceMultiplier;
    [SerializeField] private AudioClip m_breakSound;
    private AudioSource m_breakSoundSource;
    private Collider m_colliderToDisable;
    private Transform m_groupParent;
    private BarrelGroupBehavior m_barrelGroupBehavior;
    //private bool isInGroup = false; Unused
    private bool m_initialDamageTaken = false;
    private bool m_shouldPlayAudio;
    private bool m_isInGroup;

    public float Health { get { return m_health; } set { m_health = value; } }

    public GameObject DamageTextPrefab => throw new System.NotImplementedException();

    public Transform TextSpawnLocation => throw new System.NotImplementedException();

    public float FontSize => throw new System.NotImplementedException();

    public bool ShowDamageNumbers => throw new System.NotImplementedException();

    public TextMeshPro Text { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    private void Start()
    {
        m_breakSoundSource = GetComponent<AudioSource>();
        m_colliderToDisable = GetComponent<Collider>();
        if (transform.parent != null && transform.parent.TryGetComponent<BarrelGroupBehavior>(out m_barrelGroupBehavior))
        {
            //m_barrelGroupBehavior = transform.parent.GetComponent<BarrelGroupBehavior>();
            //isInGroup = true;
            m_barrelGroupBehavior.fractureChildren += Breakage;
        }
    }

    public void Breakage()
    {
        if(m_colliderToDisable != null) m_colliderToDisable.enabled = false;

        foreach (BreakablePieceBehavior breakable in gameObject.GetComponentsInChildren<BreakablePieceBehavior>())
        {
            breakable.Break(m_breakForceMultiplier, m_timeToDespawn);
            breakable.transform.SetParent(null);
        }

        if (m_breakSoundSource == null) return;

        if (m_barrelGroupBehavior != null)
        {
            if(m_barrelGroupBehavior.ShouldPlayBarrelAudio) m_breakSoundSource.PlayOneShot(m_breakSound);
            m_barrelGroupBehavior.ShouldPlayBarrelAudio = false;
        }
        else
        {
            m_breakSoundSource.PlayOneShot(m_breakSound);
        }
    }

    public void ResetBreakables()
    {
        foreach (BreakablePieceBehavior breakable in gameObject.GetComponentsInChildren<BreakablePieceBehavior>())
        {
            breakable.ResetLocalPosition();
        }
    }

    public void TakeDamage(float damageTaken, HitBoxType hitbox, Vector3 hitPoint = default(Vector3))
    {
        ////TODO: We should create a tag for barrels/breakable props since this kind of if statement is repeated
        ////and I don't like checking object names for checks like this
        //if (!gameObject.name.Contains("barrel"))
        //{
        //    return;
        //}

        Health -= damageTaken;
        if (!m_initialDamageTaken)
        {
            m_breakForceMultiplier *= damageTaken;
            m_initialDamageTaken = true;
        }
        CheckForDeath();
    }


    public void CheckForDeath()
    {
        if (Health <= 0)
        {
            if (m_barrelGroupBehavior != null && !m_barrelGroupBehavior.activated)
            {
                m_barrelGroupBehavior.FractureChildren();
            }
            Breakage();
        }
    }
}
