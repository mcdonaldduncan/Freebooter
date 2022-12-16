using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InvokeEvent : MonoBehaviour
{
    [SerializeField] GameObject m_Activator;
    [SerializeField] UnityEvent m_EventToCall;

}
