using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemy
{
    public Vector3 StartingPosition { get; set; }

    void OnDeath();

    void OnPlayerRespawn();

    void OnCheckPointReached();

}
