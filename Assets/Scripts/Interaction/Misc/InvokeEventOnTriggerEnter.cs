using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InvokeEventOnTriggerEnter : MonoBehaviour
{
    [SerializeField]
    private GameObject objectToCheck;

    public UnityEvent eventToInvoke;

    private void OnTriggerEnter(Collider other)
    {
        if (!objectToCheck.activeInHierarchy)
        {
            eventToInvoke.Invoke();
        }
    }
}
