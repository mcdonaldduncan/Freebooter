using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UtilityFunctions;

public class PlatformBase : MonoBehaviour
{
    [SerializeField] public TranslationType m_TranslationType;

    [SerializeField] public AnimationCurve m_AnimationCurve;

    [SerializeField] float m_MaxForce;
    [SerializeField] float m_MaxSpeed;
    [SerializeField] float m_LerpScale;
    [SerializeField] float m_Damping;

    [System.NonSerialized] public Transform m_Target;
    [System.NonSerialized] public Transform m_PreviousTarget;
    
    Transform m_Transform;
    Transform m_Navigator;

    MovingPlatform m_Platform;

    Vector3 m_Velocity;
    Vector3 m_Acceleration;
    Vector3 m_LinearPos;

    float m_Speed;
    float m_LerpTime;

    bool m_ShouldMove;

    void Start()
    {
        m_Transform = transform;
        m_Platform = GetComponentInParent<MovingPlatform>();
        m_Navigator = m_Platform.gameObject.FindChildWithTag("Navigator").transform;
        
        if (m_TranslationType == TranslationType.DAMP)
        {
            m_Platform.SetTransform(m_Navigator);
        }
        else
        {
            m_Platform.SetTransform(m_Transform);
        }
    }

    void Update()
    {
        HandleMovementState();
    }

    #region Movement Logic
    void HandleMovementState()
    {
        if (!m_ShouldMove) return;
        if (Time.time < m_Platform.NextMoveTime) return;

        switch (m_TranslationType)
        {
            case TranslationType.LINEAR:

                LinearMotion();

                break;
            case TranslationType.CURVE:

                CurveMotion();

                break;
            case TranslationType.STEERING:

                SteeringMotion();

                break;
            case TranslationType.DAMP:

                DampMotion();

                break;
            default:
                break;
        }
    }

    private void LinearMotion()
    {
        m_Transform.position = Vector3.MoveTowards(m_Transform.position, m_Target.position, m_Speed * Time.deltaTime);
    }

    private void CurveMotion()
    {
        m_LerpTime += Time.deltaTime / m_LerpScale;
        m_Transform.position = Vector3.Lerp(m_PreviousTarget.position, m_Target.position, m_AnimationCurve.Evaluate(m_LerpTime));
    }

    private void DampMotion()
    {
        m_Navigator.position = Vector3.MoveTowards(m_Navigator.position, m_Target.position, m_Speed * Time.deltaTime);
        m_Velocity = Vector3.ClampMagnitude(m_Velocity, m_MaxSpeed);

        var n1 = m_Velocity - (m_Transform.position - m_Navigator.position) * Mathf.Pow(m_Damping, 2) * Time.deltaTime;
        var n2 = 1 + m_Damping * Time.deltaTime;
        m_Velocity = n1 / n2;

        m_Transform.position += m_Velocity * Time.deltaTime;
    }

    private void SteeringMotion()
    {
        m_Acceleration += CalculateSteering(m_Target.position);
        m_Velocity += m_Acceleration;
        m_Transform.position += m_Velocity * Time.deltaTime;
        m_Acceleration = Vector3.zero;

        if (Vector3.Distance(m_Transform.position, m_Target.position) < .5f)
        {
            m_Platform.UpdateFromBase();
        }
    }

    Vector3 CalculateSteering(Vector3 currentTarget)
    {
        Vector3 desired = currentTarget - m_Transform.position;
        desired = desired.normalized;
        desired *= m_Speed;
        Vector3 steer = desired - m_Velocity;
        steer = steer.normalized;
        steer *= m_MaxForce;
        return steer;
    }

    #endregion

    #region Public Set Methods
    public void Init(float speed, bool shouldMove, Transform target)
    {
        m_Speed = speed;
        m_ShouldMove = shouldMove;
        m_Target = target;
        m_PreviousTarget = target;
    }

    public void SetTargets(Transform prevTarget, Transform target)
    {
        m_PreviousTarget = prevTarget;
        m_Target = target;
        m_LerpTime = 0;
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
    CURVE,
    DAMP,
    STEERING
}
