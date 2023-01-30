using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankDrone : AgentBase
{
    [Header("Drone Body")]
    [SerializeField] GameObject m_Body;
    [SerializeField] Transform m_PrimaryShootFrom;
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
        
    }

    

}
