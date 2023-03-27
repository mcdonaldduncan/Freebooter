using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
//using Unity.MLAgentsExamples;

public class DroneAcademy : MonoBehaviour
{
    private DroneSettings droneSettings;

    private void Awake()
    {
        droneSettings = FindObjectOfType<DroneSettings>();
    }

    public void ResetScene()
    {
        var agentObjects = GameObject.FindObjectsOfType<DroneAgent>();
        foreach (var agentObject in agentObjects)
        {
            agentObject.OnEpisodeBegin();
        }
    }

    public void SetEnvironmentSettings()
    {
        // Set the environment settings based on the drone settings
        //droneSettings.UpdateEnvironment();
    }
}

