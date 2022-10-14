using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnDestroyCheckForDrops : MonoBehaviour
{
    PowerUpManager powerUpManager;
    GameObject powerUpManagerGO;
    private void Start()
    {
        powerUpManagerGO = GameObject.Find("PowerUP_Manager");
        powerUpManager = powerUpManagerGO.GetComponent<PowerUpManager>();
    }
    private void OnDestroy()
    {
        powerUpManager.CheckForDrops(this.transform);
        Debug.Log("called");
    }
}
