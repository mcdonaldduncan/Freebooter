using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Fracture : MonoBehaviour, IDamageable
{
    [SerializeField] private float health;
    [SerializeField] private float breakForce;
    private Collider colliderToDisable;

    public float Health { get { return health; } set { health = value; } }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if(other.tag == "Player")
    //    {
    //        Breakage();
    //    }
    //}

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
            Vector3 force = (rb.transform.position - transform.position).normalized * breakForce;
            rb.AddForce(force);
        }
    }

    public void TakeDamage(float damageTaken)
    {
        Health -= damageTaken;
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
