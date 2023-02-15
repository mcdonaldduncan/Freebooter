using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald
public interface IDamageable
{
    public float Health { get; set; }

    void TakeDamage(float damageTaken);

    void CheckForDeath();
    
}
