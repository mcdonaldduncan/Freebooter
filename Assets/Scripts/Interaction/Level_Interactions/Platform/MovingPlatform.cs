using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;
using static UtilityFunctions;

/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald
public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Properties")]
    [SerializeField] MovementType m_MovementType;
    [SerializeField] GameObject[] m_Activators;
    
    [SerializeField] float m_MoveSpeed;
    [SerializeField] float m_nodeDelay;
    [SerializeField] bool m_ActivatePerNode;
    [SerializeField] bool m_ShouldLoop;

    [Header("Node Prefab")]
    [SerializeField] GameObject Node;
    GameObject Platform;
    PlatformBase Base;

    [Header("Nodes")]
    [SerializeField] public List<Transform> m_Nodes;

    Transform m_Transform;

    Vector3 lastPosition;

    List<IActivator> m_IActivators;

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

    private void OnEnable()
    {
        LevelManager.PlayerRespawn += Reset;
        m_IActivators = new List<IActivator>();


        foreach (var activator in m_Activators)
        {
            if (activator == null || m_MovementType != MovementType.ACTIVATE) return;

            try
            {
                IActivator temp = (IActivator)activator.GetComponent(typeof(IActivator));

                m_IActivators.Add(temp);
                temp.Activate += AgnosticActivate;
                //temp.Deactivate += OnDeactivate;
            }
            catch (System.Exception)
            {
                Debug.LogError("Valid IActivator Not Found");
            }
        }
    }

    private void OnDisable()
    {
        LevelManager.PlayerRespawn -= Reset;


        foreach (var Iactivator in m_IActivators)
        {
            if (Iactivator == null || m_MovementType != MovementType.ACTIVATE) return;
            Iactivator.Activate -= AgnosticActivate;
            //Iactivator.Deactivate -= OnDeactivate;
        }
    }


    void Start()
    {
        //Player = GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonController>();
        Platform = gameObject.FindChildWithTag("Platform");
        Base = Platform.GetComponent<PlatformBase>();
        isActivated = m_MovementType == MovementType.CONSTANT;
        Base.Init(m_MoveSpeed, isActivated, m_Nodes[0] ?? transform);
        lastPosition = Platform.transform.position;
    }

    void Update()
    {
        MonitorBase();
    }

    #region Core Logic

    void ApplyMotionToPlayer()
    {
        if (isAttached)
        {
            Vector3 platformTranslation = Platform.transform.position - lastPosition;
            LevelManager.Instance.Player.surfaceMotion += platformTranslation;
        }
        lastPosition = Platform.transform.position;
    }

    void AgnosticActivate()
    {
        isActivated = !isActivated;
        Base.SetState(isActivated);
    }

    void OnActivate()
    {
        isActivated = true;
        Base.SetState(true);
    }

    void OnDeactivate()
    {
        isActivated = false;
        Base.SetState(false);
    }

    private void Reset()
    {
        currentIndex = 0;
        Base.SetTargets(m_Nodes[currentIndex], m_Nodes[++currentIndex]);
        isActivated = m_MovementType == MovementType.CONSTANT;
        Base.SetState(isActivated);
        Platform.transform.position = transform.position;
    }

    public void UpdateFromBase()
    {
        TransitionTargets();
    }

    void MonitorBase()
    {
        if (!isActivated) return;
        
        if (Vector3.Distance(m_Transform.position, m_Nodes[currentIndex].position) > .5f) return;

        TransitionTargets();
    }

    void TransitionTargets()
    {
        if (isLooping)
        {
            if (currentIndex == 0)
            {
                isLooping = false;
                Base.SetTargets(m_Nodes[currentIndex], m_Nodes[++currentIndex]);
            }
            else
            {
                Base.SetTargets(m_Nodes[currentIndex], m_Nodes[--currentIndex]);

                
            }
            lastNodeTime = Time.time;
            if (m_ActivatePerNode) OnDeactivate();
        }
        else
        {
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
                    Base.SetTargets(m_Nodes[currentIndex], m_Nodes[--currentIndex]);
                }
            }
            else
            {
                Base.SetTargets(m_Nodes[currentIndex], m_Nodes[++currentIndex]);
            }
            lastNodeTime = Time.time;
            if (m_ActivatePerNode) OnDeactivate();
        }
    }

    #endregion

    #region Player Contact

    public void OnPlayerContact()
    {
        
        isAttached = true;
        if (m_MovementType == MovementType.CONTACT)
        {
            OnActivate();
        }
    }

    public void OnPlayerExit()
    {
        isAttached = false;
        if (m_MovementType == MovementType.CONTACT)
        {
            OnDeactivate();
        }
    }

    #endregion

    public void SetTransform(Transform t)
    {
        m_Transform = t;
    }

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
