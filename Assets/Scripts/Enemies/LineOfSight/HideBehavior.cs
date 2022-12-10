using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(NavMeshAgent))]
public class HideBehavior : MonoBehaviour
{
    [SerializeField] LayerMask m_HidableLayers;

    [Range(-1, 1)]
    [SerializeField] float m_Sensitivity;

    [Range(1, 10)]
    [SerializeField] float m_MinDistance;

    LineOfSightChecker m_LineOfSightChecker;
    NavMeshAgent m_Agent;
    Coroutine m_MovementCoroutine;
    Collider[] m_Colliders = new Collider[10];
    WaitForSeconds m_Wait = new WaitForSeconds(.15f);

    private void Awake()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        m_LineOfSightChecker = GetComponentInChildren<LineOfSightChecker>();
    }

    private void OnEnable()
    {
        m_LineOfSightChecker.enabled = true;
        m_LineOfSightChecker.OnGainSight += HandleSightGain;
        m_LineOfSightChecker.OnLoseSight += HandleSightLost;
    }

    private void OnDisable()
    {
        m_LineOfSightChecker.OnGainSight -= HandleSightGain;
        m_LineOfSightChecker.OnLoseSight -= HandleSightLost;
        m_LineOfSightChecker.enabled = false;
    }

    void HandleSightGain(Transform target)
    {
        if (m_MovementCoroutine != null)
        {
            StopCoroutine(m_MovementCoroutine);
        }

        m_MovementCoroutine = StartCoroutine(Hide(target));
    }

    void HandleSightLost(Transform target)
    {
        if (m_MovementCoroutine != null)
        {
            StopCoroutine(m_MovementCoroutine);
        }
    }

    private IEnumerator Hide(Transform target)
    {
        while (true)
        {
            for (int i = 0; i < m_Colliders.Length; i++)
            {
                m_Colliders[i] = null;
            }

            int hits = Physics.OverlapSphereNonAlloc(m_Agent.transform.position, m_LineOfSightChecker.sphereCollider.radius, m_Colliders, m_HidableLayers);

            int hitModifier = 0;

            for (int i = 0; i < hits; i++)
            {
                if (Vector3.Distance(m_Colliders[i].transform.position, target.position) < m_MinDistance)
                {
                    m_Colliders[i] = null;
                    hitModifier++;
                }
            }

            hits -= hitModifier;

            Array.Sort(m_Colliders, ColliderSortCompare);

            for (int i = 0; i < hits; i++)
            {
                if (HideAttempt(target, Vector3.zero, i, 1, 0))
                {
                    break;
                }
                else
                {
                    Debug.LogError($"Unable to find NavMesh near object {m_Colliders[i].name} at {m_Colliders[i].transform.position}");
                }
            }
            yield return m_Wait;
        }
    }

    private bool HideAttempt(Transform target, Vector3 attempt, int i, int mod, int iterations)
    {
        if (NavMesh.SamplePosition(m_Colliders[i].transform.position - attempt * mod, out NavMeshHit hit, 4f, m_Agent.areaMask))
        {
            if (!NavMesh.FindClosestEdge(hit.position, out hit, m_Agent.areaMask))
            {
                Debug.LogError($"Unable to find edge close to {hit.position}");
            }

            if (!ProcessHideAttempt(hit, (target.position - hit.position).normalized, iterations))
            {
                return HideAttempt(target, (target.position - hit.position).normalized, i, mod * 2, ++iterations);
            }
            else
            {
                return true;
            }
        }
        return false;
    }

    private bool ProcessHideAttempt(NavMeshHit hit, Vector3 target, int iterations)
    {
        if (iterations >= 3) return false;

        if (Vector3.Dot(hit.normal, target) < m_Sensitivity)
        {
            m_Agent.SetDestination(hit.position);
            return true;
        }

        return false;
    }

    private int ColliderSortCompare(Collider A, Collider B)
    {
        if (A == null & B == null)
        {
            return 0;
        }
        else if (A == null)
        {
            return 1;
        }
        else if (B == null)
        {
            return -1;
        }
        else
        {
            return VectorCompare(m_Agent.transform.position,
                A.transform.position,
                B.transform.position);
        }
    }

    private int VectorCompare(Vector3 pos, Vector3 posA, Vector3 posB)
    {
        return Vector3.Distance(pos, posA).CompareTo(Vector3.Distance(pos, posB));
    }

    public void StartHideProcessRemote(Transform target)
    {
        if (m_MovementCoroutine != null)
        {
            StopCoroutine(m_MovementCoroutine);
        }

        m_MovementCoroutine = StartCoroutine(Hide(target));
    }
}
