using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.ScrollRect;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] GameObject m_Activator;

    IActivator m_IActivator;

    [System.NonSerialized] public int m_Index;

    bool isActivated;

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
            //Debug.LogError("Valid IActivator Not Found");
        }

    }

    private void OnDisable()
    {
        if (m_IActivator == null) return;
        m_IActivator.Activate -= OnActivate;
    }


    void OnActivate()
    {
        //Debug.Log("CP activated");
        if (isActivated) return;

        LevelManager.Instance.UpdateCurrentCP(this);
        isActivated = true;
    }

    
}
