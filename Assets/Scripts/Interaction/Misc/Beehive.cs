using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beehive : MonoBehaviour, IDamageable
{
    [SerializeField]
    private float health;

    public float Health { get { return health; } set { health = value; } }

    [SerializeField]
    List<GameObject> bees;

    private RigidbodyConstraints defaultConstraints;
    private Vector3 defaultPosition;

    private void Start()
    {
        if (this.gameObject.GetComponent<Rigidbody>() != null)
        {
            defaultConstraints = this.gameObject.GetComponent<Rigidbody>().constraints;
        }
        defaultPosition = this.gameObject.transform.position;
    }

    public void CheckForDeath()
    {
        if (Health <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            return;
        }
    }

    public void TakeDamage(float damageTaken)
    {
        Health -= damageTaken;
        ActivateBees();
        CheckForDeath();
    }

    private void ActivateBees()
    {
        if (Health <= 70 && Health >= 60)
        {
            SetBeeActive(0);
        }
        if (Health <= 60 && Health >= 50)
        {
            SetBeeActive(1);
            SetBeeActive(2);
        }
        if (Health <= 50 && Health >= 40)
        {
            SetBeeActive(3);
            SetBeeActive(4);
            SetBeeActive(5);
        }
        if (Health <= 40 && Health >= 30)
        {
            SetBeeActive(6);
            SetBeeActive(7);
            SetBeeActive(8);
            SetBeeActive(9);
        }
        if (Health <= 30 && Health >= 10)
        {
            SetBeeActive(10);
            SetBeeActive(11);
            SetBeeActive(12);
            SetBeeActive(13);
        }
        if (Health <= 10)
        {
            SetBeeActive(14);
            SetBeeActive(15);
            SetBeeActive(16);
        }
    }

    private void SetBeeActive(int beeIndex)
    {
        if (bees[beeIndex] != null)
        {
            bees[beeIndex].SetActive(true);
        }
    }
}
