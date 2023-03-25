using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Why did you just make an exact copy of my explosion script instead of just asking if we should rename it?
[RequireComponent(typeof(ParticleSystem))]
public class PoolVFX : MonoBehaviour, IPoolable
{
    [SerializeField] GameObject m_Prefab;

    ParticleSystem m_ParticleSystem;
    //TrailRenderer m_TrailRenderer;

    public GameObject Prefab { get => m_Prefab; set => m_Prefab = value; }

    private void OnEnable()
    {
        if (m_ParticleSystem == null)
        {
            m_ParticleSystem = GetComponent<ParticleSystem>();
        }
        //if (m_TrailRenderer == null)
        //{
        //    m_TrailRenderer = GetComponent<TrailRenderer>();
        //}

        var mainModule = m_ParticleSystem.main;
        mainModule.stopAction = ParticleSystemStopAction.Disable;
    }

    private void OnDisable()
    {
        if (ProjectileManager.Instance == null || gameObject == null) return;
        ProjectileManager.Instance.ReturnToPool(gameObject);
    }
}
