using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioTrigger : MonoBehaviour
{
    [SerializeField] GameObject m_Activator;
    [SerializeField] AudioClip m_Clip;
    [SerializeField] Sprite m_Sprite;
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

        AudioManager.Instance.PlayClip(m_Clip);
        AudioManager.Instance.EnableNavImage(m_Sprite);

        m_IsActive = true;
    }

    private void Update()
    {
        if (!m_IsActive) return;
        if (AudioManager.Instance.ClearCheck(m_Clip))
        {
            AudioManager.Instance.DisableNavImage();
        }
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