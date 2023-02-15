using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRecipient
{
    GameObject ActivatorObject { get; }

    IActivator Activator { get; set; }
    IRecipient Recipient { get; set; }

    public void ActivatorSetUp()
    {
        if (ActivatorObject == null) return;

        try
        {
            Activator = (IActivator)ActivatorObject.GetComponent(typeof(IActivator));
            Activator.Activate += OnActivate;
        }
        catch (System.Exception)
        {
            Debug.LogError("Valid IActivator Not Found");
        }
    }

    public void Unsubscribe()
    {
        if (Activator == null) return;
        Activator.Activate -= OnActivate;
    }

    void OnActivate();
}
