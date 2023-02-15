using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemy
{
    float MovementSampleRadius { get; }

    Vector3 StartingPosition { get; set; }

    bool ShouldSleep { get; set; }

    IActivator Activator { get; set; }

    void ActivateAggro();

    void DeactivateAggro();

    void OnActivate();

    void OnDeactivate();

    void MoveToLocation(Transform location);

    void OnDeath();

    void OnPlayerRespawn();

    void OnCheckPointReached();

}
