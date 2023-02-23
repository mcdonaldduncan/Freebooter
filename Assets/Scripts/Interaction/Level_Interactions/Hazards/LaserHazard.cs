using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LaserHazard : MonoBehaviour, IRecipient
{
    [SerializeField] GameObject m_Activator;
    [SerializeField] GameObject m_LaserBeam;

    [SerializeField] Transform m_LaserTarget;

    [SerializeField] float m_DamagePerTick;
    [SerializeField] float m_TimeActive;

    Transform m_LaserTransform;

    bool isActive;

    public GameObject ActivatorObject => m_Activator;

    public IActivator Activator { get; set; }
    public IRecipient Recipient { get; set; }

    private void OnEnable()
    {
        Recipient = this;
        m_LaserTransform = m_LaserBeam.transform;
    }

    void Start()
    {
        Recipient.ActivatorSetUp();
    }

    public void OnActivate()
    {
        m_LaserBeam.SetActive(true);
        isActive = true;
        Invoke(nameof(Deactivate), m_TimeActive);
    }

    private void OnDisable()
    {
        Recipient.Unsubscribe();
    }

    void Deactivate()
    {
        m_LaserBeam.SetActive(false);
        isActive = false;
    }

    void Update()
    {
        if (!isActive) return;

        Vector3 direction = m_LaserTarget.position - m_LaserTransform.position;
        m_LaserTransform.LookAt(m_LaserTarget);
        if (Physics.Raycast(m_LaserTransform.position, direction, out RaycastHit hit))
        {
            if (hit.collider.gameObject.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(m_DamagePerTick * Time.deltaTime);
            }
        }
    }
}
