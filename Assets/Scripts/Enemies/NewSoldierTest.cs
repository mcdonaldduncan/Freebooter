using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewSoldierTest : AgentBase
{
    [SerializeField] Animator m_Animator;
    [SerializeField] GameObject m_Weapon;
    [SerializeField] Transform m_Hand;

    private void Awake()
    {
        m_Weapon.transform.SetParent(m_Hand);
    }

    private void Update()
    {
        m_Animator.SetFloat("Blend", m_Agent.velocity.magnitude);
        HandleAgentState();
    }
}
