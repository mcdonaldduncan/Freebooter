using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTrail : MonoBehaviour, IPoolable
{
    [SerializeField] GameObject m_Prefab;
    [SerializeField] float m_TravelSpeed;

    [NonSerialized] public Vector3 m_TargetPosition;

    public GameObject Prefab { get => m_Prefab; set => m_Prefab = value; }

    void Start()
    {
        
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, m_TargetPosition, m_TravelSpeed * Time.deltaTime);
    }

    public void SetUp(Vector3 startPosition)
    {
        transform.position = startPosition;
    }
}
