using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewSoldierTest : AgentBase
{
    [Header("Soldier Weapon/Hand Transforms")]
    [SerializeField] Transform m_Weapon;
    [SerializeField] Transform m_Hand;

    Animator m_Animator;

    private void Start()
    {
        HandleSetup();
        m_Animator = GetComponent<Animator>();
        m_Weapon.SetParent(m_Hand);
    }

    private void Update()
    {
        m_Animator.SetFloat("Blend", m_Agent.velocity.magnitude);
        HandleAgentState();
    }
}
