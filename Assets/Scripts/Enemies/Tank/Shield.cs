using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour, IDamageable
{
    public float _maxhealth;
    public float _health;
    public float Health { get => _health; set => _health = value; }

    private void Start()
    {
        _health = _maxhealth;
    }

    public void CheckForDeath()
    {
        if (_health < 0)
        {
            this.gameObject.SetActive(false);
        }
    }

    public void TakeDamage(float damageTaken)
    {
        _health -= damageTaken;
        CheckForDeath();
    }
}
