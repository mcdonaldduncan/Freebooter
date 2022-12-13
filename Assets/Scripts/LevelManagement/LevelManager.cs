using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{
    [SerializeField] public FirstPersonController Player;
    [SerializeField] public CheckPoint[] CheckPoints;


}
