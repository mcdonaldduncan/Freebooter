using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class NewTurretTest : AgentBase
{
    TurretState m_TurretState;

    void Start()
    {
        HandleSetup();
    }

    void Update()
    {
        switch (m_TurretState)
        {
            case TurretState.GUARD:
                Aim();
                if (CheckLineOfSight()) m_TurretState = TurretState.ATTACK;
                break;
            case TurretState.ATTACK:
                Aim();
                Shoot();
                if (!CheckRange()) m_TurretState = TurretState.GUARD;
                break;
            default:
                break;
        }
    }
}
