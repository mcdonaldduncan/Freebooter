using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void DissolvableDelegate();
public interface IDissolvable
{
    DissolvableDelegate EnemyDied { get; set; }
}
