using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald

public delegate void CombatStateEventHandler(bool InCombat);
public interface IEnemy
{
    public bool IsInCombat { get; set; }


    //public delegate void CombatStateEventHandler(bool InCombat);
    /*public event */CombatStateEventHandler CombatStateChanged { get; set; }

    public void HandleCombatStateChange();



    //float MovementSampleRadius { get; }

    //Vector3 StartingPosition { get; set; }

    //bool ShouldSleep { get; set; }

    //IActivator Activator { get; set; }

    //void ActivateAggro();

    //void DeactivateAggro();

    //void OnActivate();

    //void OnDeactivate();

}
