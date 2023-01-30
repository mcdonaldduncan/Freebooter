using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtilleryBot : AgentBase
{
    [SerializeField] Transform m_Turret;

    void Start()
    {
        HandleSetup();
    }

    void Update()
    {
        
    }

    public override void HandleAgentState()
    {
        base.HandleAgentState();
    }
}
