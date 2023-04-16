using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void PlayerDamageDelegate(float damage);

public interface IDamageTracking
{
    PlayerDamageDelegate DamageDealt { get; set; }
}
