using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
   [SerializeField] private List<GameObject> powerups = new List<GameObject>();
   [SerializeField] private float dropRate;
    public bool PowerUPS_On;
    public void CheckForDrops(Transform t)
   {
        var randomNum = Random.RandomRange(0f, 100f);
        if (randomNum < dropRate && PowerUPS_On == true)
        {
            Instantiate(powerups[Random.Range(0, powerups.Count)], t.position, t.rotation);
        }
   }
}
