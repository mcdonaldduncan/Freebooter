using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewDroneTest : AgentBase
{
    [Header("Drone Body")]
    [SerializeField] GameObject m_Body;

    OnDeathExplosion m_DeathExplosion;

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
}
