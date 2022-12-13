using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class LineOfSightChecker : MonoBehaviour
{
    [System.NonSerialized] public SphereCollider sphereCollider;
    Coroutine LineOfSightCoroutine;
    [SerializeField] LayerMask layerMask;
    [SerializeField] float FOV = 360f;

    public delegate void GainSightDelegate(Transform target);
    public delegate void LoseSightDelegate(Transform target);
    public GainSightDelegate OnGainSight;
    public LoseSightDelegate OnLoseSight;

    WaitForSeconds wait = new WaitForSeconds(.25f);

    private void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        

        if (!CheckLineOfSight(other.transform))
        {
            LineOfSightCoroutine = StartCoroutine(AssessLineOfSight(other.transform));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        OnLoseSight?.Invoke(other.transform);
        
        if (LineOfSightCoroutine != null)
        {
            StopCoroutine(LineOfSightCoroutine);
        }
    }

    private bool CheckLineOfSight(Transform target)
    {
        if (!target.CompareTag("Player")) return false;
        if (target == null) return false;

        Vector3 direction = (target.transform.position - transform.position).normalized;

        float dot = Vector3.Dot(transform.forward, direction);

        if (dot >= Mathf.Cos(FOV * Mathf.Deg2Rad))
        {
            if (!Physics.Raycast(transform.position, direction, out RaycastHit hit, sphereCollider.radius, layerMask)) return false;

            
            OnGainSight?.Invoke(target);
            return true;
        }

        return false;
    }

    private IEnumerator AssessLineOfSight(Transform target)
    {
        //if (!target.CompareTag("Player")) yield break;

        while (!CheckLineOfSight(target))
        {
            yield return wait;
        }
    }
}
