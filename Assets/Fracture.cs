using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Fracture : MonoBehaviour
{
    public GameObject fractured;
    public float breakForce;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            Breakage();
        }
    }

    public void Breakage()
    {
       GameObject frac = Instantiate(fractured,transform.position,transform.rotation);

        foreach (Rigidbody rb in frac.GetComponentsInChildren<Rigidbody>())
        {
            Vector3 force = (rb.transform.position - transform.position).normalized * breakForce;
            rb.AddForce(force);
        }

        Destroy(gameObject);
    }
}
