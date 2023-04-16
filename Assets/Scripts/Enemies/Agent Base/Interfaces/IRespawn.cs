using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRespawn
{
    //IRespawn Respawn { get; set; }

    public void SubscribeToRespawn()
    {
        LevelManager.Instance.PlayerRespawn += OnPlayerRespawn;
    }

    public void SubscribeToCheckpointReached()
    {
        LevelManager.Instance.CheckPointReached += OnCheckPointReached;
    }

    public void OnCheckPointReached()
    {
        LevelManager.Instance.PlayerRespawn -= OnPlayerRespawn;
    }

    void OnPlayerRespawn();

}
