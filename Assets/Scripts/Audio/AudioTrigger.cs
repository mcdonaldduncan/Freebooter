using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTrigger : MonoBehaviour
{
    //[SerializeField] BehaviorType m_BehaviorType;
    [SerializeField] GameObject m_Activator;
    [SerializeField] AudioSource m_AudioSource;
    [SerializeField] AudioClip m_clip;
    [SerializeField] bool m_ActivateMultiple;

    IActivator m_IActivator;

    bool m_IsActive;

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
        if (m_IsActive && !m_ActivateMultiple) return;
        m_AudioSource.clip = m_clip;
        m_AudioSource.Play();
        
        
        m_IsActive = true;
    }


}

enum BehaviorType
{
    QUEUE,
    OVERRIDE
}


//switch (m_BehaviorType)
//{
//    case BehaviorType.QUEUE:
//        AudioManager.Instance.QueueSound(m_clip);
//        break;
//    case BehaviorType.OVERRIDE:
//        m_AudioSource.clip = m_clip;
//        m_AudioSource.Play();
//        break;
//    default:
//        break;
//}