using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IActivator
{
    public delegate void ActivateDelegate();
    public event ActivateDelegate Activate;
    public event ActivateDelegate Deactivate;

    public void FireActivation();
    public void FireDeactivation();
}
