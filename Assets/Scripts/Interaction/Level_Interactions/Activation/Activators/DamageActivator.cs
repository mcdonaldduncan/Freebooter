using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald
public class DamageActivator : MonoBehaviour, IDamageable, IActivator
{
    [SerializeField] float m_Health;
    [SerializeField] GameObject m_DeathParticles;
    

    public float Health { get; set; }

    public GameObject DamageTextPrefab => throw new System.NotImplementedException();

    public Transform TextSpawnLocation => throw new System.NotImplementedException();

    public float FontSize => throw new System.NotImplementedException();

    public bool ShowDamageNumbers => throw new System.NotImplementedException();

    public TextMeshPro Text { get; set; }

    public event IActivator.ActivateDelegate Activate;
    public event IActivator.ActivateDelegate Deactivate;

    private void Start()
    {
        LevelManager.Instance.PlayerRespawn += SetUsable;
        SetUsable();
    }

    public void CheckForDeath()
    {
        if (Health <= 0)
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

    public void TakeDamage(float damageTaken, HitBoxType hitbox, Vector3 hitPoint = default(Vector3))
    {
        if (Health < 0) return;
        Health -= damageTaken;
        CheckForDeath();
    }


    void SetInactive()
    {
        gameObject.SetActive(false);
        LevelManager.Instance.CheckPointReached += OnCheckPointReached;
    }

    public void OnCheckPointReached()
    {
        LevelManager.Instance.PlayerRespawn -= SetUsable;
    }

    void HandleParticle()
    {
        if (m_DeathParticles == null) return;

        Instantiate(m_DeathParticles, transform.position, Quaternion.identity);
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
