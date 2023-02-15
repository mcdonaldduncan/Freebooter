using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.ScrollRect;

/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald
public class SetGroupActive : MonoBehaviour
{
    [SerializeField] GameObject m_Activator;
    [SerializeField] public bool m_UseChildren;
    [SerializeField] GameObject[] m_GroupToActivate;

    IActivator m_IActivator;

    private void OnEnable()
    {
        if (m_Activator == null) return;

        try
        {
            m_IActivator = (IActivator)m_Activator.GetComponent(typeof(IActivator));
            m_IActivator.Activate += Onactivate;
        }
        catch (System.Exception)
        {
            Debug.LogError("Valid IActivator Not Found");
        }

    }

    private void OnDisable()
    {
        if (m_IActivator == null) return;
        m_IActivator.Activate -= Onactivate;
    }

    void Start()
    {
        LevelManager.PlayerRespawn += OnPlayerRespawn;
    }

    void Onactivate()
    {
        if (m_UseChildren)
        {
            foreach (GameObject child in transform)
            {
                if (child.activeSelf) continue;
                child.SetActive(true);
            }
        }
        else
        {
            foreach (GameObject set in m_GroupToActivate)
            {
                if (set.activeSelf) continue;
                set.SetActive(true);
            }
        }

        
        LevelManager.CheckPointReached += OnCheckPointReached;
    }

    void OnPlayerRespawn()
    {
        if (m_UseChildren)
        {
            foreach (GameObject child in transform)
            {
                if (!child.activeSelf) continue;
                child.SetActive(false);
            }
        }
        else
        {
            foreach (GameObject set in m_GroupToActivate)
            {
                if (!set.activeSelf) continue;
                set.SetActive(false);
            }
        }
        
    }

    void OnCheckPointReached()
    {
        LevelManager.PlayerRespawn -= OnPlayerRespawn;
    }
}
