using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySwarmerBehavior : MonoBehaviour, IDamageable
{
    [SerializeField]
    private float health;

    public float Health { get { return health; } set { health = value; } }

    public void Damage(float damageTaken)
    {
        Health -= damageTaken;
        CheckForDeath();
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
}
