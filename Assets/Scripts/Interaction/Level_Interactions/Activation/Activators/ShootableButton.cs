using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Reusable, damageable activator
/// </summary>
/// Author: Duncan McDonald
public class ShootableButton : MonoBehaviour, IDamageable, IActivator
{
    [SerializeField] float m_Health;
    [SerializeField] float m_ResetDelay;
    [SerializeField] Material[] m_Materials;

    MeshRenderer m_Renderer;

    bool isUsable;

    public float Health {get; set; }
    public GameObject DamageTextPrefab { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public Transform TextSpawnLocation { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public float FontSize { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public bool ShowDamageNumbers { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public TextMeshPro Text { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public event IActivator.ActivateDelegate Activate;
    public event IActivator.ActivateDelegate Deactivate;

    private void OnEnable()
    {
        m_Renderer = GetComponent<MeshRenderer>();
    }

    public void CheckForDeath()
    {
        if (Health <= 0)
        {
            FireActivation();
            m_Renderer.material = m_Materials[1];
            Invoke(nameof(SetUsable), m_ResetDelay);
        }
    }

    public void FireActivation()
    {
        Activate?.Invoke();
    }

    public void FireDeactivation()
    {
        Deactivate?.Invoke();
    }

    public void TakeDamage(float damageTaken, HitBoxType hitbox)
    {
        if (Health < 0) return;
        Health -= damageTaken;
        CheckForDeath();
    }

    void SetUsable()
    {
        Health = m_Health;
        m_Renderer.material = m_Materials[0];
    }

}
