using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRespawn
{
    IRespawn Respawn { get; set; }

    public void SubscribeToRespawn()
    {
        LevelManager.PlayerRespawn += OnPlayerRespawn;
    }

    public void SubscribeToCheckpointReached()
    {
        LevelManager.CheckPointReached += OnCheckPointReached;
    }

    public void OnCheckPointReached()
    {
        LevelManager.PlayerRespawn -= OnPlayerRespawn;
    }

    void OnPlayerRespawn();

}
