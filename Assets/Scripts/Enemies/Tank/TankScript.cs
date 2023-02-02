using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankScript : AgentBase
{
    
    private void Start()
    {
        HandleSetup();
    }

    private void Update()
    {
        HandleAgentState();
    }
}
