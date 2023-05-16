using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald
[RequireComponent(typeof(ParticleSystem))]
public class SelfPoolingVFX : MonoBehaviour, IPoolable
{
    [SerializeField] GameObject m_Prefab;

    ParticleSystem m_ParticleSystem;

    public GameObject Prefab { get => m_Prefab; set => m_Prefab = value; }

    private void OnEnable()
    {
        if (m_ParticleSystem == null)
        {
            m_ParticleSystem = GetComponent<ParticleSystem>();
        }

        var mainModule = m_ParticleSystem.main;
        mainModule.stopAction = mainModule.stopAction != ParticleSystemStopAction.Disable ? ParticleSystemStopAction.Disable : mainModule.stopAction;
    }

    private void OnDisable()
    {
        if (ProjectileManager.Instance == null || gameObject == null) return;
        ProjectileManager.Instance.ReturnToPool(gameObject);
    }
}
