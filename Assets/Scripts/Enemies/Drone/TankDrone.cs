using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald
public class TankDrone : AgentBase
{
    [Header("Drone Body")]
    [SerializeField] GameObject m_Body;
    [SerializeField] Transform m_SecondaryShootFrom;

    OnDeathExplosion m_DeathExplosion;

    int shotCount;

    private void Start()
    {
        HandleSetup();
        m_DeathExplosion = m_Body.GetComponent<OnDeathExplosion>();
    }

    public override void OnPlayerRespawn()
    {
        base.OnPlayerRespawn();
        m_DeathExplosion.ResetVariables();
    }

    private void Update()
    {
        if (isDead)
        {
            m_DeathExplosion.StartDeathSequence();


            return;
        }
        HandleAgentState();
    }

    public override void HandleAgentState()
    {
        distanceToPlayer = Vector3.Distance(transform.position, m_Target.position);

        switch (m_State)
        {
            case AgentState.GUARD:
                Aim();
                if (CheckLineOfSight()) m_State = AgentState.CHASE;
                break;
            case AgentState.WANDER:
                Wander();
                break;
            case AgentState.CHASE:
                Aim();
                if (altShoootFrom) Shoot(m_SecondaryShootFrom);
                else Shoot();
                ChasePlayer();
                break;
            case AgentState.RETURN:
                ReturnToOrigin();
                break;
            default:
                break;
        }
    }



}
