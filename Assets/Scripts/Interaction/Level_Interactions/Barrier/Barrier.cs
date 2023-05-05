using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald
public class Barrier : MonoBehaviour, IRespawn
{
    // State options
    [Header("State Options")]
    [SerializeField] public BarrierState m_State; // The current state of the barrier
    [SerializeField] AccessType m_AccessType; // The type of access for the barrier (proximity, activate, locked)
    [SerializeField] bool m_ShouldClose; // Determines if the barrier should close automatically
    [SerializeField] float m_CloseDelay; // The delay before the barrier closes

    // Segment Parameters
    [Header("Segment Parameters")]
    [SerializeField] public float MoveSpeed; // The speed at which the barrier moves

    // Prefabs
    [Header("Prefabs")]
    [SerializeField] GameObject m_KeyPrefab; // The prefab for the key
    [SerializeField] GameObject m_SegmentPrefab; // The prefab for the barrier segment

    // Activation Object
    [Header("Activation Object")]
    [SerializeField] GameObject m_Activator; // The object that activates the barrier

    [Header("")]
    [SerializeField] public List<Key> m_RequiredKeys; // The keys required to open the barrier

    // Events
    public delegate void ActionDelegate();
    public event ActionDelegate Activation; // Event for when the barrier is activated
    public event ActionDelegate SetState; // Event for when the barrier's state is set

    IActivator m_IActivator; // Interface for the activator object
    IRespawn m_IRespawn;

    BarrierState m_StartingState;

    // Determines if the player has all the required keys
    bool HasKeys => KeyManager.Instance.KeyInventory.Intersect(m_RequiredKeys).Count() == m_RequiredKeys.Count;

    // Determines if the barrier should auto close
    bool AutoClose => m_AccessType == AccessType.ACTIVATE && m_ShouldClose && m_State == BarrierState.OPEN;

    bool m_InTrigger; // Determines if the player is in the trigger zone

#if UNITY_EDITOR
    // Draws a gizmo in the Unity editor for the barrier's position
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one);
    }
#endif

    public delegate void LockedBarrierDelegate();
    public event LockedBarrierDelegate LockedBarrierAccessed;
    public event LockedBarrierDelegate LockedBarrierOpened;

    // Register events when the script is enabled
    private void OnEnable()
    {
        Activation += OnActivation;
        m_StartingState = m_State;
        m_IRespawn = this;


        // If no activator is set or the access type is not activate, return
        if (m_Activator == null || m_AccessType != AccessType.ACTIVATE) return;

        try
        {
            // Get the IActivator component from the activator object
            m_IActivator = (IActivator)m_Activator.GetComponent(typeof(IActivator));
            m_IActivator.Activate += OnActivate;
            m_IActivator.Deactivate += OnDeactivate; 
        }
        catch (System.Exception)
        {
            Debug.LogError("Valid IActivator Not Found");
        }
    }

    private void OnDisable()
    {
        // unsubscribe from the Activation event
        Activation -= OnActivation;

        // if the IActivator is null or the access type is not ACTIVATE, return
        if (m_IActivator == null || m_AccessType != AccessType.ACTIVATE) return;
        // unsubscribe from the Activate and Deactivate events
        m_IActivator.Activate -= OnActivate;
        m_IActivator.Deactivate -= OnDeactivate;
    }

    void Start()
    {
        m_IRespawn.SubscribeToRespawn();

        // if the initial state of the barrier is open, invoke the SetState event
        if (m_State == BarrierState.OPEN)
        {
            SetState?.Invoke();
        }
    }

    private void OnActivate()
    {
        // if the barrier shouldn't close and it is currently open, return
        if (!m_ShouldClose && m_State == BarrierState.OPEN) return;

        // invoke the Activation event
        Activation?.Invoke();
        m_IRespawn.SubscribeToCheckpointReached();

        // if AutoClose is enabled, invoke the OnActivate method after the specified delay
        if (AutoClose)
        {
            Invoke(nameof(OnActivate), m_CloseDelay);
        }
    }

    private void OnDeactivate()
    {
        // if the barrier is open, invoke the Activation event
        if (m_State == BarrierState.OPEN)
        {
            Activation?.Invoke();
        }
    }

    void OnActivation()
    {
        if (!m_ShouldClose && m_State == BarrierState.OPEN) return;
        // switch the state of the barrier between open and closed
        m_State = m_State == BarrierState.OPEN ? BarrierState.CLOSED : BarrierState.OPEN;
    }

    public GameObject AddSegment()
    {
        // instantiate a new barrier segment and return it
        GameObject segment = Instantiate(m_SegmentPrefab, transform, false);
        return segment;
    }

    public GameObject AddKey()
    {
        // instantiate a new key and add it to the RequiredKeys list
        GameObject key = Instantiate(m_KeyPrefab, transform, false);
        m_RequiredKeys.Add(key.GetComponent<Key>());
        return key;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (m_InTrigger) return;
        if (!other.gameObject.CompareTag("Player")) return;
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
                    LockedBarrierAccessed?.Invoke();
                    break;
                }
                else
                {
                    OnActivate();
                    LockedBarrierOpened?.Invoke();
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
        m_InTrigger = false;
        if (m_State == BarrierState.CLOSED) return;
        OnActivate();
    }

    public void OnPlayerRespawn()
    {
        m_State = m_StartingState;
        if (m_State == BarrierState.OPEN)
        {
            SetState?.Invoke();
        }
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


