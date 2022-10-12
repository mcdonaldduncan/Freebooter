using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Crate_MainMenu : MonoBehaviour, IDamageable
{
    [SerializeField]
    private float health;

    public float Health { get { return health; } set { health = value; } }

    public UnityEvent TriggerEventOnDisable;

    //[SerializeField]
    //private GameObject myPrefab;

    //[SerializeField]
    //private Vector3 gameObjectPosition;

    private RigidbodyConstraints defaultConstraints;
    private Vector3 defaultPosition;

    private void Start()
    {
        defaultConstraints = this.gameObject.GetComponent<Rigidbody>().constraints;
        defaultPosition = this.gameObject.transform.position;
    }

    public void CheckForDeath()
    {
        if (Health <= 0)
        {
            DisableAllComponents();
            TriggerEventOnDisable.Invoke();
        }
        else
        {
            return;
        }
    }

    public void TakeDamage(float damageTaken)
    {
        Health -= damageTaken;
        CheckForDeath();
    }

    private void DisableAllComponents()
    {
        this.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        this.gameObject.GetComponent<MeshRenderer>().enabled = false;
        this.gameObject.GetComponent<MeshCollider>().enabled = false;
    }

    public void ReturnToDefaults()
    {
        Health = 1f;
        this.gameObject.GetComponent<Rigidbody>().constraints = defaultConstraints;
        this.gameObject.transform.position = defaultPosition;
        this.gameObject.GetComponent<MeshRenderer>().enabled = true;
        this.gameObject.GetComponent<MeshCollider>().enabled = true;
    }
}
