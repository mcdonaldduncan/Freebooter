using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class DroneSettings : MonoBehaviour
{
    public float moveSpeed;
    public float rotateSpeed;

    //public void UpdateEnvironment()
    //{
    //    // Update the observation and action spaces for the drone agent
    //    var agents = FindObjectsOfType<DroneAgent>();
    //    foreach (var agent in agents)
    //    {
    //        var agentBehaviorParams = agent.agentParameters;
    //        var agentActionSpec = new ActionSpec(3, new float[] { -1f, -1f, -1f }, new float[] { 1f, 1f, 1f });
    //        var agentSensorSpec = new SensorSpec(
    //            new List<int>() { 12 },
    //            SensorCompressionType.None,
    //            "LidarSensor",
    //            1,
    //            0,
    //            "drone"
    //        );

    //        agentBehaviorParams.ActionSpec = agentActionSpec;
    //        agentBehaviorParams.Sensors = new List<SensorSpec> { agentSensorSpec };
    //    }
    //}
}

