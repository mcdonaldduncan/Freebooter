using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.UIElements;

public class BarrierSegment : MonoBehaviour
{
    [Header("Node Prefab")]
    [SerializeField] GameObject NodePrefab;

    [Header("Nodes")]
    [SerializeField] public List<Transform> m_Nodes;

    Transform m_Transform;

    Barrier m_ParentController;

    float m_Speed;

    int m_CurrentIndex;
    int m_LastIndex;

    bool m_ShouldMove;


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
    }

#endif

    private void OnEnable()
    {
        m_ParentController = transform.parent.GetComponentInParent<Barrier>();
        m_ParentController.Activation += OnActivation;
        m_ParentController.SetState += OnSetState;
    }

    private void OnDisable()
    {
        m_ParentController.Activation -= OnActivation;
        m_ParentController.SetState -= OnSetState;
    }

    void Start()
    {
        m_Transform = transform;
        m_Speed = m_ParentController.MoveSpeed;
    }

    void Update()
    {
        HandleState();
        HandleMovement();
    }

    #region Movement Logic

    void HandleMovement()
    {
        if (!m_ShouldMove) return;
        m_Transform.position = Vector3.MoveTowards(m_Transform.position, m_Nodes[m_CurrentIndex].position, m_Speed * Time.deltaTime);
    }

    void HandleOpen()
    {
        if (!m_ShouldMove) return;

        if (m_CurrentIndex < m_LastIndex) m_CurrentIndex = m_LastIndex;

        if (m_Transform.position == m_Nodes[m_Nodes.Count - 1].position)
        {
            m_ShouldMove = false;
            return;
        }
        if (m_Transform.position != m_Nodes[m_CurrentIndex].position) return;

        m_LastIndex = m_CurrentIndex++;
        
    }

    void HandleClose()
    {
        if (!m_ShouldMove) return;

        if (m_CurrentIndex > m_LastIndex) m_CurrentIndex = m_LastIndex;

        if (m_Transform.position == m_Nodes[0].position)
        {
            m_ShouldMove = false;
            return;
        }
        if (m_Transform.position != m_Nodes[m_CurrentIndex].position) return;

        m_LastIndex = m_CurrentIndex--;

    }

    void HandleState()
    {
        if (!m_ShouldMove) return;
        switch (m_ParentController.m_State)
        {
            case BarrierState.OPEN:
                HandleOpen();
                break;
            case BarrierState.CLOSED:
                HandleClose();
                break;
            default:
                break;
        }
    }

    #endregion

    #region EventSubscribers

    void OnActivation()
    {
        m_ShouldMove = true;
    }

    void OnSetState()
    {
        m_CurrentIndex = m_Nodes.Count - 1;
        m_Transform.position = m_Nodes[m_CurrentIndex].position;
    }

    #endregion

    public GameObject AddNode()
    {
        GameObject node = Instantiate(NodePrefab, transform.parent, false);
        m_Nodes.Add(node.transform);
        node.name = $"Node_{m_Nodes.Count}";
        return node;
    }
}
