using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ProximityButton : MonoBehaviour, IActivator
{
    [SerializeField] float m_ResetDelay;
    [SerializeField] Material[] m_Materials;

    BoxCollider m_BoxCollider;
    MeshRenderer m_Renderer;

    bool m_isUsable;
    bool m_inTrigger;

    public event IActivator.ActivateDelegate Activate;
    public event IActivator.ActivateDelegate Deactivate;

    public void FireActivation()
    {
        Activate?.Invoke();
    }

    public void FireDeactivation()
    {
        Deactivate?.Invoke();
    }

    private void OnEnable()
    {
        m_BoxCollider = GetComponent<BoxCollider>();
        m_Renderer = GetComponent<MeshRenderer>();
        m_BoxCollider.isTrigger = true;
        m_isUsable = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!m_isUsable) return;
        if (m_inTrigger) return;
        if (other.gameObject.CompareTag("Player"))
        {
            m_inTrigger = true;
            m_isUsable = false;
            FireActivation();
            m_Renderer.material = m_Materials[1];
            Invoke(nameof(SetUsable), m_ResetDelay);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!m_inTrigger) return;
        if (!other.gameObject.CompareTag("Player")) return;
        m_inTrigger = false;
    }

    void SetUsable()
    {
        m_isUsable = true;
        m_Renderer.material = m_Materials[0];
    }
}
