using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CritVFX : MonoBehaviour, IPoolable
{
    [SerializeField] GameObject m_Prefab;

    ParticleSystem m_ParticleSystem;

    public GameObject Prefab { get => m_Prefab; set => m_Prefab = value; }

    private void Start()
    {
        Prefab.SetActive(true);
    }

    private void OnEnable()
    {
        if (m_ParticleSystem == null)
        {
            m_ParticleSystem = GetComponent<ParticleSystem>();
        }

        var mainModule = m_ParticleSystem.main;
        mainModule.stopAction = ParticleSystemStopAction.Disable;
    }

    private void OnDisable()
    {
        if (ProjectileManager.Instance == null) return;
        ProjectileManager.Instance.ReturnToPool(gameObject);
    }
}
