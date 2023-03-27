using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatDrone : MonoBehaviour
{
    [SerializeField] float m_RangeFromOrigin;
    [SerializeField] float m_HorizontalAvoidance;
    [SerializeField] float m_VerticalAvoidance;

    Transform m_Transform;


    void Start()
    {
        m_Transform = transform;
    }

    void Update()
    {
        
    }
}
