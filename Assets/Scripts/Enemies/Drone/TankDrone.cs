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

    int shotCount;

    bool altShootFrom;

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

    private void Update()
    {
        if (IsDead) return;
        
        HandleAgentState();
    }

    public override void HandleAgentState()
    {
        switch (m_State)
        {
            case AgentState.GUARD:
                m_Tracking.TrackTarget();
                if (m_Tracking.CheckFieldOfView()) m_State = AgentState.CHASE;
                if (IsInCombat) HandleCombatStateChange();
                break;
            case AgentState.WANDER:
                m_Navigation.Wander();
                if (m_Tracking.CheckFieldOfView()) m_State = AgentState.CHASE;
                if (IsInCombat) HandleCombatStateChange();
                break;
            case AgentState.CHASE:
                m_Navigation.ChaseTarget();
                m_Tracking.TrackTarget();
                if (m_Tracking.CheckFieldOfView())
                {

                    if (AltShootFrom) m_Shooting.Shoot(m_SecondaryShootFrom, true);
                    else m_Shooting.Shoot(true);
                    
                }
                if (!m_Tracking.InRange) m_State = AgentState.RETURN;
                if (!IsInCombat) HandleCombatStateChange();
                break;
            case AgentState.RETURN:
                m_Navigation.MoveToLocationDirect(m_StartingPosition);
                if (m_Navigation.CheckReturned(m_StartingPosition)) m_State = m_StartingState;
                if (m_Tracking.CheckFieldOfView()) m_State = AgentState.CHASE;
                if (IsInCombat) HandleCombatStateChange();
                break;
            case AgentState.SLEEP:
                m_Navigation.Sleep();
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
