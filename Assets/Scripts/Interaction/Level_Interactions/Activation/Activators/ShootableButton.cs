using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
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

    public void TakeDamage(float damageTaken)
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
