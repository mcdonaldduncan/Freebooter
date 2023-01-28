using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;

public class Fracture : MonoBehaviour, IDamageable
{
    [SerializeField] private float health;
    [SerializeField] private float breakForceMulitplier;
    private Collider colliderToDisable;
    private Transform groupParent;
    private BarrelGroupBehavior barrelGroupBehavior;
    private bool isInGroup = false;

    public float Health { get { return health; } set { health = value; } }

    private void Start()
    {
        colliderToDisable = GetComponent<Collider>();
        if (transform.parent != null)
        {
            barrelGroupBehavior = transform.parent.GetComponent<BarrelGroupBehavior>();
            isInGroup = true;
            barrelGroupBehavior.fractureChildren += Breakage;
        }
    }

    public void Breakage()
    {
        colliderToDisable.enabled = false;
        foreach (Rigidbody rb in gameObject.GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = false;
            Vector3 force = (rb.transform.forward * breakForceMulitplier);
            rb.AddForce(force);
            //gameObject.GetComponent<Collider>().isTrigger = true;
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
            if (barrelGroupBehavior != null && !barrelGroupBehavior.activated)
            {
                barrelGroupBehavior.FractureChildren();
            }
            Breakage();
        }
    }
}
