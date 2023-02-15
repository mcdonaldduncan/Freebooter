using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [SerializeField] private GunHandler.GunType ammoTypeToGive;
    [SerializeField] private int ammoAmount;
    private GunHandler gunHandler;
    private FirstPersonController player; //keeping this for when we add audio
    private IGun gunToRecieveAmmo;

    // Start is called before the first frame update
    void Start()
    {
        gunHandler = LevelManager.Instance.Player.GetComponentInChildren<GunHandler>();
        player = LevelManager.Instance.Player.GetComponent<FirstPersonController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (gunToRecieveAmmo == null)
        {
            if (gunHandler.GunTypeList.Contains(ammoTypeToGive))
            {
                gunToRecieveAmmo = gunHandler.GunDict[ammoTypeToGive];
            }
            else return; //if it's null and not present in list, then player does not yet have gun, so ammo should not be picked up
        }

        if (other.CompareTag("Player") && gunToRecieveAmmo.CurrentAmmo < gunToRecieveAmmo.MaxAmmo)
        {
            gunHandler.OnAmmoPickup(ammoTypeToGive, ammoAmount);
            gameObject.SetActive(false);
        }
    }
}
