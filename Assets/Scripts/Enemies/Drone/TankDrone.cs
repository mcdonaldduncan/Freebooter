using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald
public class TankDrone : NewAgentBase
{
    [Header("Drone Body")]
    [SerializeField] GameObject m_Body;
    [SerializeField] Transform m_SecondaryShootFrom;

    OnDeathExplosion m_DeathExplosion;

    public override Transform TrackingTransform => m_Body.transform;

    private void Awake()
    {
        AwakeSetup();
    }

    private void OnEnable()
    {
        EnableSetup();
    }

    private void Start()
    {
        StartSetup();
        m_DeathExplosion = m_Body.GetComponent<OnDeathExplosion>();
    }

    public override void OnPlayerRespawn()
    {
        base.OnPlayerRespawn();
        m_DeathExplosion.ResetVariables();
    }

    private void FixedUpdate()
    {
        if (IsDead) return;
        
        HandleAgentState();
    }

    public override void HandleAgentState()
    {
        switch (State)
        {
            case AgentState.GUARD:
                Tracking.TrackTarget();
                if (Tracking.CheckFieldOfView()) State = AgentState.CHASE;
                if (IsInCombat) HandleCombatStateChange();
                break;
            case AgentState.WANDER:
                Navigation.Wander();
                if (Tracking.CheckFieldOfView()) State = AgentState.CHASE;
                if (IsInCombat) HandleCombatStateChange();
                break;
            case AgentState.CHASE:
                Navigation.ChaseTarget();
                Tracking.TrackTarget();
                if (Tracking.CheckFieldOfView())
                {

                    if (AltShootFrom) Shooting.Shoot(m_SecondaryShootFrom);
                    else Shooting.Shoot();

                }
                else if (Tracking.InRange) State = AgentState.GUARD;
                else State = AgentState.RETURN;
                if (!IsInCombat) HandleCombatStateChange();
                break;
            case AgentState.RETURN:
                Navigation.MoveToLocationDirect(StartingPosition);
                if (Navigation.CheckReturned(StartingPosition)) State = StartingState;
                if (Tracking.CheckFieldOfView()) State = AgentState.CHASE;
                if (IsInCombat) HandleCombatStateChange();
                break;
            case AgentState.SLEEP:
                Navigation.Sleep();
                if (IsInCombat) HandleCombatStateChange();
                break;
            default:
                break;
        }
    }

    //if (altShoootFrom) Shoot(m_SecondaryShootFrom);
    
    public override void OnDeath()
    {
        base.OnDeath();
        gameObject.SetActive(true);
        m_DeathExplosion.StartDeathSequence();
    }

}
