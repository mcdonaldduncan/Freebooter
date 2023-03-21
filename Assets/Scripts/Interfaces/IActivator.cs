using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald
public interface IActivator
{
    public delegate void ActivateDelegate();
    public event ActivateDelegate Activate;
    public event ActivateDelegate Deactivate;

    public void FireActivation();
    public void FireDeactivation();
}
