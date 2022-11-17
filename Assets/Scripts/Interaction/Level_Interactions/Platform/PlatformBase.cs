using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformBase : MonoBehaviour
{
    [SerializeField] public TranslationType m_TranslationType;

    [SerializeField] float m_MaxForce;
    [SerializeField] float m_MaxSpeed;

    [SerializeField] bool m_IsDamp;

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

    void Update()
    {
        HandleMovement();
    }

    #region Movement Logic
    void HandleMovement()
    {
        if (!m_ShouldMove) return;
        if (Time.time < m_Platform.NextMoveTime) return;

        m_Transform.position = Vector3.MoveTowards(m_Transform.position, m_Target.position, m_Speed * Time.deltaTime);
    }

    #endregion

    #region Public Set Methods
    public void Init(float speed, bool shouldMove, Transform target)
    {
        m_Speed = speed;
        m_ShouldMove = shouldMove;
        m_Target = target;
    }

    public void SetTarget(Transform target)
    {
        m_Target = target;
    }

    public void SetState(bool state)
    {
        m_ShouldMove = state;
    }

    #endregion

    #region Collision Logic

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

    #endregion
}

public enum TranslationType
{
    LINEAR,
    DAMP,
    STEERING
}
