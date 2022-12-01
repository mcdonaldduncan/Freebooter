using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
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
    WaitForSeconds m_Wait = new WaitForSeconds(.25f);

    private void Awake()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        m_LineOfSightChecker = GetComponentInChildren<LineOfSightChecker>();
    }

    private void OnEnable()
    {
        m_LineOfSightChecker.OnGainSight += HandleSightGain;
        m_LineOfSightChecker.OnLoseSight += HandleSightLost;
    }

    private void OnDisable()
    {
        m_LineOfSightChecker.OnGainSight -= HandleSightGain;
        m_LineOfSightChecker.OnLoseSight -= HandleSightLost;
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

            int hits = Physics.OverlapSphereNonAlloc(m_Agent.transform.position, 
                m_LineOfSightChecker.sphereCollider.radius, 
                m_Colliders, 
                m_HidableLayers);

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
                if (!NavMesh.SamplePosition(m_Colliders[i].transform.position, 
                    out NavMeshHit hit, 
                    2f, 
                    m_Agent.areaMask)) 
                    break;
                
                if (!NavMesh.FindClosestEdge(hit.position, out hit, m_Agent.areaMask))
                {
                    Debug.LogError($"Unable to find edge close to {hit.position}");
                }

                ProcessHideAttempt(hit, (target.position - hit.position).normalized, 0);
            }
            yield return m_Wait;

        }
    }

    private void ProcessHideAttempt(NavMeshHit hit, Vector3 target, int iterations)
    {
        if (iterations >= 5) return;

        if (Vector3.Dot(hit.normal, target) < m_Sensitivity)
        {
            m_Agent.SetDestination(hit.position);
            //m_Agent.transform.position = hit.position;
            return;
        }
        else
        {
            ProcessHideAttempt(hit, target * 2, ++iterations);
        }
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

}
