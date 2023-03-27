using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class DroneAgent : Agent
{
    public GameObject player;
    public float moveSpeed;
    public float rotateSpeed;

    private Rigidbody rb;
    private Vector3 startingPosition;
    private Quaternion startingRotation;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        startingPosition = transform.position;
        startingRotation = transform.rotation;
    }

    public override void OnEpisodeBegin()
    {
        transform.position = startingPosition;
        transform.rotation = startingRotation;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Observe player's position and rotation
        sensor.AddObservation(player.transform.position);
        sensor.AddObservation(player.transform.rotation);

        // Observe drone's position and rotation
        sensor.AddObservation(transform.position);
        sensor.AddObservation(transform.rotation);

        // Observe drone's velocity and angular velocity
        sensor.AddObservation(rb.velocity);
        sensor.AddObservation(rb.angularVelocity);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        base.OnActionReceived(actions);
    }

    //public override void OnActionReceived(float[] vectorAction)
    //{
    //    // Move the drone based on the action vector
    //    float moveX = vectorAction[0];
    //    float moveZ = vectorAction[1];
    //    float rotateY = vectorAction[2];

    //    rb.AddRelativeForce(new Vector3(moveX, 0, moveZ) * moveSpeed);
    //    rb.AddRelativeTorque(new Vector3(0, rotateY, 0) * rotateSpeed);

    //    // Reward the agent for getting closer to the player
    //    float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
    //    if (distanceToPlayer < 5f)
    //    {
    //        SetReward(0.1f);
    //    }

    //    // Punish the agent for moving too far away from the player
    //    if (distanceToPlayer > 20f)
    //    {
    //        SetReward(-0.1f);
    //    }
    //}
}
