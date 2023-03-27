using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public interface INavigation
{
    NavMeshAgent Agent { get; set; }

    LayerMask WalkableLayers { get; }

    float StoppingDistance { get; }
    float RotationSpeed { get; }
    float MovementSampleRadius { get; }

    float WanderDelay { get; }
    float WanderDistance { get; }
    float LastWanderTime { get; set; }

    /// <summary>
    /// Returns true if wander time is valid and path status is complete
    /// </summary>
    private bool shouldWander => Time.time > WanderDelay + LastWanderTime && Agent.pathStatus == NavMeshPathStatus.PathComplete;

    /// <summary>
    /// Chase the player, attempting to follow at the distance provided by stopping distance
    /// </summary>
    void ChaseTarget()
    {
        float randomX = Random.Range(-1f, 1f);
        float randomZ = Random.Range(-1f, 1f);

        Vector3 playerToAgent = Agent.transform.position - LevelManager.Instance.Player.transform.position;
        Vector3 randomOffset = new Vector3(randomX, 0, randomZ);

        if (StoppingDistance <= playerToAgent.magnitude)
        {
            MoveToLocation(LevelManager.Instance.Player.transform.position + randomOffset.normalized * StoppingDistance);
        }

    }

    /// <summary>
    /// Attempt to sample a navmesh position at location.position and move to that position
    /// </summary>
    /// <param name="location">transform at desired location</param>
    void MoveToLocation(Transform location)
    {
        if (NavMesh.SamplePosition(location.position, out NavMeshHit hit, MovementSampleRadius, NavMesh.AllAreas))
        {
            Agent.SetDestination(hit.position);
        }
        else
        {
            Debug.LogWarning($"The position {location.position} is a location with no navmesh");
        }

    }

    /// <summary>
    /// Attempt to sample a navmesh position and location and move to that position
    /// </summary>
    /// <param name="location">Vector3 describing desired location</param>
    void MoveToLocation(Vector3 location)
    {
        if (NavMesh.SamplePosition(location, out NavMeshHit hit, MovementSampleRadius, NavMesh.AllAreas))
        {
            Agent.SetDestination(hit.position);
        }
        else
        {
            Debug.LogWarning($"The position {location} is a location with no navmesh");
        }

    }

    /// <summary>
    /// Attempt to move to a location without previously sampling
    /// </summary>
    /// <param name="position"></param>
    void MoveToLocationDirect(Vector3 position)
    {
        Agent.SetDestination(position);
    }

    /// <summary>
    /// Returns true if agent is within one unit of starting position
    /// </summary>
    /// <param name="startingPosition"></param>
    /// <returns></returns>
    bool CheckReturned(Vector3 startingPosition)
    {
        if (Vector3.Distance(Agent.transform.position, startingPosition) < 1f) return true;

        return false;
    }

    /// <summary>
    /// Cycle agent for teleportation and resetting to ensure navmesh connection
    /// </summary>
    /// <param name="startingPosition"></param>
    void CycleAgent(Vector3 startingPosition)
    {
        if (Agent == null) return;

        if (!Agent.isOnNavMesh)
        {
            Agent.enabled = false;
            Agent.enabled = true;
        }
        else
        {
            Agent.isStopped = true;
            Agent.isStopped = false;
        }

        Agent.Warp(startingPosition);
    }

    /// <summary>
    /// Return random position on the navmesh within radius distance
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="distance"></param>
    /// <param name="layerMask"></param>
    /// <returns></returns>
    Vector3 RandomPosInSphere(Vector3 origin, float distance, LayerMask layerMask)
    {
        Vector3 randomPosition = Random.insideUnitSphere * distance;
        NavMesh.SamplePosition(randomPosition + origin, out NavMeshHit navHit, distance, layerMask);
        return navHit.position;
    }

    /// <summary>
    /// Update callable method to wander when next available
    /// </summary>
    void Wander()
    {
        if (!shouldWander) return;

        Agent.SetDestination(RandomPosInSphere(Agent.transform.position, WanderDistance, NavMesh.AllAreas));

        LastWanderTime = Time.time;
    }

    /// <summary>
    /// Set agent to stopped and
    /// </summary>
    void Sleep()
    {
        if (Agent.isStopped) return;

        Agent.ResetPath();
        Agent.isStopped = true;
    }

    /// <summary>
    /// Wake agent
    /// </summary>
    void Wake()
    {
        if (!Agent.isStopped || !Agent.isActiveAndEnabled) return;
        Agent.isStopped = false;
    }
}
