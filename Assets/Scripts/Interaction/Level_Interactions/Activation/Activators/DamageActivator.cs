using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DamageActivator : MonoBehaviour, IDamageable, IActivator
{
    [SerializeField] float m_Health;
    [SerializeField] GameObject m_DeathParticles;
    

    public float Health { get; set; }

    public event IActivator.ActivateDelegate Activate;
    public event IActivator.ActivateDelegate Deactivate;

    private void Start()
    {
        LevelManager.PlayerRespawn += SetUsable;
        SetUsable();
    }

    public void CheckForDeath()
    {
        if (m_Health <= 0)
        {
            
            FireActivation();
            
        }
    }

    public void FireActivation()
    {
        Debug.Log("Activation fired");
        Activate?.Invoke();
        HandleParticle();
        SetInactive();
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
    
    void SetInactive()
    {
        gameObject.SetActive(false);
        LevelManager.CheckPointReached += OnCheckPointReached;
    }

    public void OnCheckPointReached()
    {
        LevelManager.PlayerRespawn -= SetUsable;
    }

    void HandleParticle()
    {
        if (m_DeathParticles == null) return;

        Instantiate(m_DeathParticles, this.transform.position, Quaternion.identity);
    }

    void SetUsable()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        Health = m_Health;
    }
}
