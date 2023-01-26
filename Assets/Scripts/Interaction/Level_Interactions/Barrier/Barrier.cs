using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Barrier : MonoBehaviour
{
    [Header("State Options")]
    [SerializeField] public BarrierState m_State;
    [SerializeField] AccessType m_AccessType;
    [SerializeField] bool m_ShouldClose;
    [SerializeField] float m_CloseDelay;
    
    [Header("Segment Parameters")]
    [SerializeField] public float MoveSpeed;

    [Header("Prefabs")]
    [SerializeField] GameObject m_KeyPrefab;
    [SerializeField] GameObject m_SegmentPrefab;

    [Header("Activation Object")]
    [SerializeField] GameObject m_Activator;

    [Header("")]
    [SerializeField] public List<Key> m_RequiredKeys;

    public delegate void ActionDelegate();
    public event ActionDelegate Activation;
    public event ActionDelegate SetState;

    IActivator m_IActivator;

    bool HasKeys => KeyManager.Instance.KeyInventory.Intersect(m_RequiredKeys).Count() == m_RequiredKeys.Count;

    bool AutoClose => m_AccessType == AccessType.ACTIVATE && m_ShouldClose && m_State == BarrierState.OPEN;

    bool m_InTrigger;

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        

        Gizmos.DrawWireCube(transform.position, Vector3.one);
    }

#endif

    private void OnEnable()
    {
        Activation += OnActivation;

        if (m_Activator == null || m_AccessType != AccessType.ACTIVATE) return;

        try
        {
            m_IActivator = (IActivator)m_Activator.GetComponent(typeof(IActivator));
            m_IActivator.Activate += OnActivate;
            m_IActivator.Deactivate += OnDeactivate; 
        }
        catch (System.Exception)
        {
            //Debug.LogError("Valid IActivator Not Found");
        }
    }

    private void OnDisable()
    {
        Activation -= OnActivation;

        if (m_IActivator == null || m_AccessType != AccessType.ACTIVATE) return;
        m_IActivator.Activate -= OnActivate;
        m_IActivator.Deactivate -= OnDeactivate;
    }

    void Start()
    {
        if (m_State == BarrierState.OPEN)
        {
            SetState?.Invoke();
        }
    }

    private void OnActivate()
    {
        if (!m_ShouldClose && m_State == BarrierState.OPEN) return;

        Activation?.Invoke();

        if (AutoClose)
        {
            Invoke(nameof(OnActivate), m_CloseDelay);
        }
    }

    private void OnDeactivate()
    {
        if (m_State == BarrierState.OPEN)
        {
            Activation?.Invoke();
        }
    }

    void OnActivation()
    {
        m_State = m_State == BarrierState.OPEN ? BarrierState.CLOSED : BarrierState.OPEN;
    }

    public GameObject AddSegment()
    {
        GameObject segment = Instantiate(m_SegmentPrefab, transform, false);
        return segment;
    }

    public GameObject AddKey()
    {
        GameObject key = Instantiate(m_KeyPrefab, transform, false);
        m_RequiredKeys.Add(key.GetComponent<Key>());
        return key;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (m_InTrigger) return;
        if (!other.gameObject.CompareTag("Player")) return;
        //Debug.Log("Trigger Enter");
        //Debug.Log(Time.time);
        m_InTrigger = true;
        if (!m_ShouldClose && m_State == BarrierState.OPEN) return;

        switch (m_AccessType)
        {
            case AccessType.PROXIMITY:
                OnActivate();
                break;

            case AccessType.ACTIVATE:
                break;

            case AccessType.LOCKED:

                if (!HasKeys)
                {
                    // send message to UI
                    break;
                }
                else
                {
                    OnActivate();
                }

                break;

            default:
                break;
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (!m_InTrigger) return;
        if (!other.gameObject.CompareTag("Player")) return;
        //Debug.Log("Trigger Exit");
        m_InTrigger = false;
        if (m_State == BarrierState.CLOSED) return;
        OnActivate();
    }
}

enum AccessType
{
    PROXIMITY,
    ACTIVATE,
    LOCKED,
}

public enum BarrierState
{
    CLOSED,
    OPEN,
}


