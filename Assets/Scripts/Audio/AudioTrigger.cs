using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioTrigger : MonoBehaviour
{
    [SerializeField] GameObject m_Activator; // reference to the gameobject that activates this audio trigger
    [SerializeField] AudioClip m_Clip; // reference to the audio clip that will be played
    [SerializeField] Sprite m_Sprite; // reference to the sprite that will be displayed when the audio clip is played
    [SerializeField] bool m_ActivateMultiple; // a flag that determines if this audio trigger can be activated multiple times or not

    IActivator m_IActivator; // reference to the IActivator component on the activator gameobject

    bool m_IsActive; // flag that determines if this audio trigger is currently active

    private void OnEnable()
    {
        if (m_Activator == null) return; // if there is no activator gameobject, exit the function

        try
        {
            // try to get the IActivator component from the activator gameobject
            m_IActivator = (IActivator)m_Activator.GetComponent(typeof(IActivator));
            // subscribe to the Activate event
            m_IActivator.Activate += OnActivate;
        }
        catch (System.Exception)
        {
            Debug.LogError("Valid IActivator Not Found"); // if the activator gameobject does not have a valid IActivator component, log an error
        }

    }

    private void OnDisable()
    {
        if (m_IActivator == null) return; // if there is no IActivator component, exit the function
        // unsubscribe to the Activate event
        m_IActivator.Activate -= OnActivate;
    }

    void OnActivate()
    {
        if (m_IsActive && !m_ActivateMultiple) return; // if this audio trigger is currently active and it's not set to activate multiple times, exit the function

        AudioManager.Instance.PlayClip(m_Clip); // play the audio clip
        AudioManager.Instance.EnableNavImage(m_Sprite); // enable the navigation image

        m_IsActive = true; // set the flag to indicate that this audio trigger is currently active
    }

    private void Update()
    {
        if (!m_IsActive) return; // if this audio trigger is not currently active, exit the function
        if (AudioManager.Instance.ClearCheck(m_Clip))
        {
            AudioManager.Instance.DisableNavImage(); // if the audio clip has finished playing, disable the navigation image
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