using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockDissolve : MonoBehaviour
{
    [SerializeField] private float m_dissolveTime = 0.025f;
    [SerializeField] private float m_dissolveRate = 0.0125f;
    [SerializeField] private float m_dissolveWidth = 0.02f;

    [SerializeField]
    private Barrier barrier;
    [SerializeField]
    private GameObject particles;

    bool canDissolve = false;

    private MeshRenderer m_mesh;
    private Material m_material;

    private float m_dissolveCounter;
    private float m_dissolveRefreshStartTime;

    private bool CompletelyDissolved => m_material.GetFloat("_DissolveAmount") >= 1;
    private bool shouldDisableParticles => m_material.GetFloat("_DissolveAmount") >= 0.5;

    private void OnEnable()
    {
        barrier.LockedBarrierOpened += StartDissolve;
        barrier.Activation += StartDissolve;
    }

    private void OnDisable()
    {
        barrier.LockedBarrierOpened -= StartDissolve;
        barrier.Activation -= StartDissolve;
    }

    private void Start()
    {
        m_mesh = GetComponent<MeshRenderer>();
        if (m_mesh != null) m_material = m_mesh.materials[0];
    }

    private void Update()
    {
        if (canDissolve)
        {
            Dissolve();
        }
    }

    void StartDissolve()
    {
        canDissolve = true;
    }

    private void Dissolve()
    {
        m_material.SetFloat("_DissolveWidth", m_dissolveWidth);

        if (m_dissolveRefreshStartTime + m_dissolveTime < Time.time)
        {
            m_dissolveCounter += m_dissolveRate;
            m_material.SetFloat("_DissolveAmount", m_dissolveCounter);

            m_dissolveRefreshStartTime = Time.time;
        }
        if (shouldDisableParticles && gameObject.activeSelf && particles.gameObject.activeSelf)
        {
            particles.gameObject.SetActive(false);
        }

        if (CompletelyDissolved && gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }
}
