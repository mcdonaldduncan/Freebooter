using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakablePieceBehavior : MonoBehaviour
{
    private float m_timeToDisable;
    private Rigidbody m_pieceRB;
    private float m_timeBroken;
    private bool m_isBroken;
    private Vector3 m_startingLocalPos;

    private void OnEnable()
    {
        Physics.IgnoreLayerCollision(8, 14);
    }

    private void Start()
    {
        m_pieceRB = GetComponent<Rigidbody>();
        m_startingLocalPos = transform.localPosition;
    }

    private void Update()
    {
        if (m_isBroken && m_timeToDisable > 0)
        {
            if (m_timeBroken + m_timeToDisable <= Time.time)
            {
                gameObject.SetActive(false);
            }
        }
    }

    public void Break(float breakForceMultiplier, float timeToDespawn)
    {
        m_pieceRB.isKinematic = false;
        Vector3 force = (gameObject.transform.forward * breakForceMultiplier);
        m_pieceRB.AddForce(force);

        m_isBroken = true;
        m_timeBroken = Time.time;
        m_timeToDisable = timeToDespawn; 
    }

    public void ResetLocalPosition()
    {
        gameObject.transform.localPosition = m_startingLocalPos;
        m_isBroken = false;
        m_pieceRB.isKinematic = true;
        gameObject.SetActive(true);
    }
}
