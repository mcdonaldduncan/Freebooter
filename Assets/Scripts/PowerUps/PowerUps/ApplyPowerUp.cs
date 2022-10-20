using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ApplyPowerUp : MonoBehaviour
{
    public enum PowerType {Ammo, Health, Speed };
    public PowerType pt;
    [SerializeField] private PowerUpManager powerUpManager;
    [SerializeField] private GameObject PowerUpManagerGO;
    public float duration;
    bool oneTouch = false;

    

    
    // Start is called before the first frame update
    void Start()
    {
        //powerUpManager = PowerUpManagerGO.GetComponent<PowerUpManager>();
        duration = 30;
    }
   
    private void OnTriggerEnter(Collider other)
    {
        if (oneTouch == false && other.tag == "Player")
        {
            // the switch here is reduntant, you check the power type in intake data
            /*
            switch (pt)
            {
                case PowerType.Ammo:
                    powerUpManager.IntakeData(duration, pt);
                    break;
                case PowerType.Health:
                    powerUpManager.IntakeData(duration, pt);
                    break;
                case PowerType.Speed:
                    powerUpManager.IntakeData(duration, pt);
                    break;
                default:
                    break;
            }
            */
            PowerUpManager.Instance.IntakeData(duration, pt);
            oneTouch = true;
            Destroy(this.gameObject);
        }   
    }
}
