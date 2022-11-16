using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UtilityFunctions;

public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Properties")]
    [SerializeField] MovementType m_MovementType;
    [SerializeField] float m_MoveSpeed;
    [SerializeField] float m_nodeDelay;
    [SerializeField] bool m_ShouldLoop;
    

    [Header("Node Prefab")]
    [SerializeField] GameObject Node;
    GameObject Platform;
    PlatformBase Base;

    [Header("Nodes")]
    [SerializeField] public List<Transform> m_Nodes;

    Transform m_Transform;

    FirstPersonController Player;

    Vector3 lastPosition;

    bool isActivated;
    bool isLooping;
    bool isAttached;

    int currentIndex = 0;

    float lastNodeTime;

    public float NextMoveTime => lastNodeTime + m_nodeDelay;

    #region Gizmo Drawing
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 lastPosition = transform.position;
        foreach (var _transform in m_Nodes)
        {
            Gizmos.DrawSphere(_transform.position, .5f);
            Gizmos.DrawLine(lastPosition, _transform.position);
            lastPosition = _transform.position;
        }

        Gizmos.DrawWireCube(transform.position, Vector3.one);
    }
#endif
    #endregion

    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonController>();
        Platform = gameObject.FindChildWithTag("Platform");
        Base = Platform.GetComponent<PlatformBase>();
        isActivated = m_MovementType == MovementType.CONSTANT;
        Base.Init(m_MoveSpeed, isActivated, m_Nodes[0] ?? transform);
        m_Transform = Platform.transform;
        lastPosition = m_Transform.position;
    }

    void Update()
    {
        MonitorBase();
        ApplyMotionToPlayer();
    }

    #region Core Logic

    void ApplyMotionToPlayer()
    {
        if (!isAttached) return;
        if (lastPosition == m_Transform.position) return;
        Vector3 platformTranslation = m_Transform.position - lastPosition;
        Player.surfaceMotion += platformTranslation;
        lastPosition = m_Transform.position;
    }

    void MonitorBase()
    {
        if (!isActivated) return;
        if (isLooping)
        {
            if (!(m_Transform.position == m_Nodes[currentIndex].position)) return;

            if (currentIndex == 0)
            {
                isLooping = false;
                Base.SetTarget(m_Nodes[++currentIndex]);
            }
            else
            {
                Base.SetTarget(m_Nodes[--currentIndex]);
            }
            lastNodeTime = Time.time;
        }
        else
        {
            if (!(m_Transform.position == m_Nodes[currentIndex].position)) return;
            
            if (currentIndex == m_Nodes.Count - 1)
            {
                if (!m_ShouldLoop)
                {
                    isActivated = false;
                    Base.SetState(false);
                }
                else
                {
                    isLooping = true;
                    Base.SetTarget(m_Nodes[--currentIndex]);
                }
                
            }
            else
            {
                Base.SetTarget(m_Nodes[++currentIndex]);
            }
            lastNodeTime = Time.time;
        }
    }

    #endregion

    #region Player Contact

    public void OnPlayerContact()
    {
        if (m_MovementType == MovementType.CONTACT)
        {
            isAttached = true;
            isActivated = true;
            Base.SetState(true);
        }
    }

    public void OnPlayerExit()
    {
        if (m_MovementType == MovementType.CONTACT)
        {
            isAttached = false;
            isActivated = false;
            Base.SetState(false);
        }
    }

    #endregion

    #region Editor Functions
    public GameObject AddNode()
    {
        GameObject node = Instantiate(Node, transform, false);
        m_Nodes.Add(node.transform);
        node.name = $"Node_{m_Nodes.Count}";
        return node;
    }
    #endregion
}

public enum MovementType
{
    CONSTANT,
    CONTACT,
    ACTIVATE
}
