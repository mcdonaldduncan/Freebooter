using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsObjectBehavior : MonoBehaviour, IDamageable
{
    #region UnusedIDamageableStuff
    public float Health { get; set; }

    public GameObject DamageTextPrefab { get { return null; } }

    public Transform TextSpawnLocation { get; }

    public float FontSize { get; }

    public void CheckForDeath()
    {
        throw new System.NotImplementedException();
    }
    #endregion

    [SerializeField] private float m_force;
    [SerializeField] private float m_forceScale;

    private Rigidbody m_objRB;

    private void Start()
    {
        m_objRB = GetComponent<Rigidbody>();
    }

    public void TakeDamage(float damageTaken, HitBoxType hitbox, Vector3 hitPoint = default)
    {
        Vector3 forceDirection = m_objRB.position - hitPoint;
        m_objRB.AddForce(forceDirection.normalized * m_force * m_forceScale, ForceMode.Impulse);
    }
}
