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

    //void TakeDamage(float damageTaken);
    void TakeDamage(float damageTaken, HitBoxType? hitType = null); //used for damage numbers, use this when player is damaging an enemy

    void CheckForDeath();
    
}
