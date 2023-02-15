using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.UI.ScrollRect;

/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald
public class InvokeEvent : MonoBehaviour
{
    [SerializeField] GameObject m_Activator;
    [SerializeField] UnityEvent m_EventToCall;

    IActivator m_IActivator;

    private void OnEnable()
    {
        if (m_Activator == null) return;

        try
        {
            m_IActivator = (IActivator)m_Activator.GetComponent(typeof(IActivator));
            m_IActivator.Activate += OnActivate;
        }
        catch (System.Exception)
        {
            Debug.LogError("Valid IActivator Not Found");
        }

    }

    private void OnDisable()
    {
        if (m_IActivator == null) return;
        m_IActivator.Activate -= OnActivate;
    }

    void OnActivate()
    {
        m_EventToCall.Invoke();
    }

}
