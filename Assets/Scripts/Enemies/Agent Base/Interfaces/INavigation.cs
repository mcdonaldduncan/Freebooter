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

    private bool shouldWander => Time.time > WanderDelay + LastWanderTime && Agent.pathStatus == NavMeshPathStatus.PathComplete;

    void ChaseTarget()
    {
        Vector3 FromPlayerToAgent = Agent.transform.position - LevelManager.Instance.Player.transform.position;
        if (StoppingDistance <= FromPlayerToAgent.magnitude)
        {
            MoveToLocation(LevelManager.Instance.Player.transform.position + FromPlayerToAgent.normalized * StoppingDistance);
        }
        
    }

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

    void MoveToLocationDirect(Vector3 position)
    {
        Agent.SetDestination(position);
    }

    bool CheckReturned(Vector3 startingPosition)
    {
        if (Vector3.Distance(Agent.transform.position, startingPosition) < 1f) return true;

        return false;
    }

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

    Vector3 RandomPosInSphere(Vector3 origin, float distance, LayerMask layerMask)
    {
        Vector3 randomPosition = Random.insideUnitSphere * distance;
        NavMesh.SamplePosition(randomPosition + origin, out NavMeshHit navHit, distance, layerMask);
        return navHit.position;
    }

    void Wander()
    {
        if (!shouldWander) return;

        MoveToLocation(RandomPosInSphere(Agent.transform.position, WanderDistance, WalkableLayers));

        LastWanderTime = Time.time;
    }

    void Sleep()
    {
        if (Agent.isStopped) return;

        Agent.ResetPath();
        Agent.isStopped = true;
    }

    void Wake()
    {
        if (!Agent.isStopped || !Agent.isActiveAndEnabled) return;
        Agent.isStopped = false;
    }
}
