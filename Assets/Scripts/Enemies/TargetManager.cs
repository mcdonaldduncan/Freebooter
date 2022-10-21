using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

public class TargetManager : Singleton<TargetManager>
{
    public NetworkPlayerController[] targets;

    public void FindTargets()
    {
        targets = FindObjectsOfType<NetworkPlayerController>();
        if (targets.Length == 2)
        {
            targets[0].playerGun2 = targets[1].playerGun;
            targets[1].playerGun2 = targets[0].playerGun;
        }
    }

}
