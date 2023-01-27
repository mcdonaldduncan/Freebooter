using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Fracture : MonoBehaviour, IDamageable
{
    [SerializeField] private float health;
    [SerializeField] private float breakForceMulitplier;
    private Collider colliderToDisable;

    public float Health { get { return health; } set { health = value; } }

    private void Start()
    {
        colliderToDisable = GetComponent<Collider>();
    }

    public void Breakage()
    {
        colliderToDisable.enabled = false;
        foreach (Rigidbody rb in gameObject.GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = false;
            Vector3 force = (rb.transform.forward * breakForceMulitplier);
            rb.AddForce(force);
        }
    }

    public void TakeDamage(float damageTaken)
    {
        Health -= damageTaken;
        breakForceMulitplier *= damageTaken;
        CheckForDeath();
    }

    public void CheckForDeath()
    {
        if (Health <= 0)
        {
            Breakage();
        }
    }
}
