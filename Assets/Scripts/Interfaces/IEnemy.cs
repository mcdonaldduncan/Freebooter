using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemy
{
    public Vector3 StartingPosition { get; set; }

    // public bool ShouldSleep { get; set; }

    // void ActivateAggression();

    // void DeactivateAggression();

    //

    void OnDeath();

    void OnPlayerRespawn();

    void OnCheckPointReached();

}
