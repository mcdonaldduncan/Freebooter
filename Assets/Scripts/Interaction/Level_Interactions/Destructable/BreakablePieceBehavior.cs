using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakablePieceBehavior : MonoBehaviour
{
    [SerializeField] private float m_dissolveTime = 0.025f;
    [SerializeField] private float m_dissolveRate = 0.0125f;
    [SerializeField] private float m_dissolveWidth = 0.02f;

    private MeshRenderer m_mesh;
    private Material m_material;

    private float m_dissolveCounter;
    private float m_dissolveRefreshStartTime;
    private float m_dissolveAmountStart;

    private float m_timeToDisable;
    private Rigidbody m_pieceRB;
    private float m_timeBroken;
    private bool m_isBroken;
    private Vector3 m_startingLocalPos;

    private bool ShouldDissolve => m_material != null && m_material.GetFloat("_DissolveAmount") < 1 && m_isBroken 
        && m_timeBroken + m_timeToDisable <= Time.time;
    private bool CompletelyDissolved => m_material.GetFloat("_DissolveAmount") >= 1;

    private void OnEnable()
    {
        Physics.IgnoreLayerCollision(8, 14);
    }

    private void Start()
    {
        m_pieceRB = GetComponent<Rigidbody>();
        m_mesh = GetComponent<MeshRenderer>();
        m_startingLocalPos = transform.localPosition;
        if (m_mesh != null) m_material = m_mesh.materials[0];
        m_dissolveAmountStart = m_material.GetFloat("_DissolveAmount");
    }

    private void Update()
    {
        if (m_timeToDisable > 0)
        {
            if (ShouldDissolve)
            {
                Dissolve();
            }
        }

    }

    public void Break(float breakForceMultiplier, float timeToDespawn)
    {
        m_dissolveCounter = 0;
        m_pieceRB.isKinematic = false;
        Vector3 force = (gameObject.transform.forward * breakForceMultiplier);
        m_pieceRB.AddForce(force);

        m_timeBroken = Time.time;
        m_dissolveRefreshStartTime = Time.time;
        m_isBroken = true;
        m_timeToDisable = timeToDespawn;
    }

    private void Dissolve()
    {
        m_material.SetFloat("_DissolveWidth", m_dissolveWidth);

        if (m_dissolveRefreshStartTime + m_dissolveTime < Time.time)
        {
            m_dissolveCounter += m_dissolveRate;
            m_material.SetFloat("_DissolveAmount", m_dissolveCounter);

            m_dissolveRefreshStartTime = Time.time;
        }

        if (CompletelyDissolved && gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    public void ResetLocalPosition()
    {
        gameObject.transform.localPosition = m_startingLocalPos;
        m_isBroken = false;
        m_pieceRB.isKinematic = true;
        m_material.SetFloat("_DissolveAmount", m_dissolveAmountStart);
        gameObject.SetActive(true);
    }
}
