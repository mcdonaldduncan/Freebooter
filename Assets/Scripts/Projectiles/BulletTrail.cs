using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTrail : MonoBehaviour, IPoolable
{
    [SerializeField] GameObject m_Prefab;
    [SerializeField] GameObject m_HitSplat;
    [SerializeField] GameObject m_MissSplat;
    [SerializeField] float m_TravelSpeed;
    [SerializeField] float m_LifeTime;

    [NonSerialized] public Vector3 m_TargetPosition;

    float startTime;

    bool hitDamageable;

    public GameObject Prefab { get => m_Prefab; set => m_Prefab = value; }

    private void OnEnable()
    {
        startTime = Time.time;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, m_TargetPosition, m_TravelSpeed * Time.deltaTime);
        if (Time.time > m_LifeTime + startTime || transform.position == m_TargetPosition) ResetTrail();
    }

    public void Launch(Vector3 targetPosition)
    {
        m_TargetPosition = targetPosition;
        hitDamageable = false; // will pass value in if we want to go that route
    }

    // Not using currently, might use if we want
    void SpawnHitFeedback()
    {
        if (hitDamageable)
        {
            ProjectileManager.Instance.TakeFromPool(m_HitSplat, transform.position);
        }
        else
        {
            ProjectileManager.Instance.TakeFromPool(m_MissSplat, transform.position);
        }


        ResetTrail();
    }

    void ResetTrail()
    {
        ProjectileManager.Instance.ReturnToPool(gameObject);
    }
}
