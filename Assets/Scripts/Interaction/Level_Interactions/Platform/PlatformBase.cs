using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UtilityFunctions;

/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald
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
    MeshRenderer m_Renderer;

    MovingPlatform m_Platform;
    //FirstPersonController m_Player;

    Vector3 m_Velocity;
    Vector3 m_Acceleration;
    Vector3 m_CurrentPos;
    Vector3 m_LastPos;
    Vector3 m_LinearPos;

    float m_Speed;
    float m_LerpTime;

    bool m_ShouldMove;
    //bool m_IsAttached;

    void Start()
    {
        //m_Player = GameObject.FindWithTag("Player").GetComponent<FirstPersonController>();
        m_Renderer = GetComponent<MeshRenderer>();
        m_Platform = GetComponentInParent<MovingPlatform>();
        m_Navigator = m_Platform.gameObject.FindChildWithTag("Navigator").transform;
        m_Transform = transform;
        
        if (m_TranslationType == TranslationType.DAMP)
        {
            m_Platform.SetTransform(m_Navigator);
        }
        else
        {
            m_Platform.SetTransform(m_Transform);
        }
    }

    void FixedUpdate()
    {
        //m_CurrentPos = transform.position;
        //if (m_IsAttached && m_CurrentPos != m_LastPos)
        //{
        //    Vector3 temp1 = new Vector3(m_CurrentPos.x, 0, m_CurrentPos.z);
        //    Vector3 temp2 = new Vector3(m_LastPos.x, 0, m_LastPos.z);
        //    var surfaceMotion = m_CurrentPos - m_LastPos;
        //    LevelManager.Instance.Player.surfaceMotion += surfaceMotion;
        //}
        HandleMovementState();
        m_LastPos = m_CurrentPos;
    }

    public void SetMaterial(Material mat)
    {
        m_Renderer.material = mat;
    }

    #region Movement Logic
    void HandleMovementState()
    {
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
        if (!m_ShouldMove) return;
        if (Time.time < m_Platform.NextMoveTime) return;
        m_Transform.position = Vector3.MoveTowards(m_Transform.position, m_Target.position, m_Speed * Time.deltaTime);
    }

    private void CurveMotion()
    {
        if (!m_ShouldMove) return;
        if (Time.time < m_Platform.NextMoveTime) return;
        m_LerpTime += Time.deltaTime / m_LerpScale;
        m_Transform.position = Vector3.Lerp(m_PreviousTarget.position, m_Target.position, m_AnimationCurve.Evaluate(m_LerpTime));
    }

    private void DampMotion()
    {
        m_ShouldMove = Vector3.Distance(m_Transform.position, m_Target.position) > .1f;
        
        if (!m_ShouldMove) return;

        m_Velocity = Vector3.ClampMagnitude(m_Velocity, m_MaxSpeed);

        var n1 = m_Velocity - (m_Transform.position - m_Navigator.position) * Mathf.Pow(m_Damping, 2) * Time.deltaTime;
        var n2 = 1 + m_Damping * Time.deltaTime;
        m_Velocity = n1 / n2;

        m_Transform.position += m_Velocity * Time.deltaTime;

        
        if (Time.time < m_Platform.NextMoveTime) return;

        m_Navigator.position = Vector3.MoveTowards(m_Navigator.position, m_Target.position, m_Speed * Time.deltaTime);
    }

    private void SteeringMotion()
    {
        if (!m_ShouldMove) return;

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
            collision.transform.SetParent(transform);
            //m_IsAttached = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            m_Platform.OnPlayerExit();
            collision.transform.SetParent(null, true);
            //m_IsAttached = false;
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
