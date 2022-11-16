using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformBase : MonoBehaviour
{
    [System.NonSerialized] public Transform m_Target;
    Transform m_Transform;

    MovingPlatform m_Platform;

    float m_Speed;
    bool m_ShouldMove;

    void Start()
    {
        m_Transform = transform;
        m_Platform = GetComponentInParent<MovingPlatform>();
    }

    public void Init(float speed, bool shouldMove, Transform target)
    {
        m_Speed = speed;
        m_ShouldMove = shouldMove;
        m_Target = target;
    }

    void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (!m_ShouldMove) return;

        m_Transform.position = Vector3.MoveTowards(m_Transform.position, m_Target.position, m_Speed * Time.deltaTime);
    }

    public void SetNewTarget(Transform target)
    {
        m_Target = target;
    }

    public void SetState(bool state)
    {
        m_ShouldMove = state;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            m_Platform.OnPlayerContact();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            m_Platform.OnPlayerExit();
        }
    }
}
