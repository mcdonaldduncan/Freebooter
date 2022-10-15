using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckForDrops : MonoBehaviour
{
    PowerUpManager powerUpManager;
    GameObject powerUpManagerGO;
    IDamageable d;
    bool checkOnce = true;
    
    private void Start()
    {
        powerUpManagerGO = GameObject.Find("PowerUP_Manager");
        powerUpManager = powerUpManagerGO.GetComponent<PowerUpManager>();
    }
    public void DropOrNot()
    {
        if (checkOnce == true)
        {
            powerUpManager.CheckForDrops(this.transform);
            checkOnce = false;
        }  
    }
}
