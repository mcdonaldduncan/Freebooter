using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [SerializeField] private GunHandler.GunType ammoTypeToGive;
    [SerializeField] private int ammoAmount;
    [SerializeField] private AudioClip ammoPickupSound;
    private GunHandler gunHandler;
    private FirstPersonController player; //keeping this for when we add audio
    private IGun gunToRecieveAmmo;

    public delegate void AmmoPickupDelegate(bool hasWeapon);
    public event AmmoPickupDelegate AmmoPickupFailed;

    // Start is called before the first frame update
    void Start()
    {
        gunHandler = LevelManager.Instance.Player.GetComponentInChildren<GunHandler>();
        player = LevelManager.Instance.Player.GetComponent<FirstPersonController>();
        LevelManager.Instance.PlayerRespawn += OnPlayerRespawn;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (gunToRecieveAmmo == null)
        {
            if (gunHandler.GunTypeList.Contains(ammoTypeToGive))
            {
                gunToRecieveAmmo = gunHandler.GunDict[ammoTypeToGive];
            }
            else
            {
                AmmoPickupFailed?.Invoke(false);
                return; //if it's null and not present in list, then player does not yet have gun, so ammo should not be picked up
            }
        }

        if (gunToRecieveAmmo.CurrentAmmo < gunToRecieveAmmo.MaxAmmo)
        {
            gunHandler.OnAmmoPickup(ammoTypeToGive, ammoAmount);
            player.PlayerAudioSource.PlayOneShot(ammoPickupSound);
            gameObject.SetActive(false);
            LevelManager.Instance.CheckPointReached += OnCheckPointReached;
        }
        else
        {
            AmmoPickupFailed?.Invoke(true);
        }
    }

    private void OnCheckPointReached()
    {
        LevelManager.Instance.PlayerRespawn -= OnPlayerRespawn;
    }

    private void OnPlayerRespawn()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
    }
}
