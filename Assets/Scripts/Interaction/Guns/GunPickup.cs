using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunPickup : MonoBehaviour
{
    [SerializeField] private GunHandler.GunType gunTypeToPickup;
    private GunHandler gunHandler;
    private FirstPersonController player;

    private void Start()
    {
        gunHandler = LevelManager.Instance.Player.GetComponentInChildren<GunHandler>();
        player = LevelManager.Instance.Player.GetComponent<FirstPersonController>();
    }

    //Once the player walks over the weapon
    private void OnTriggerEnter(Collider other)
    {
        //make sure the player is the one walking over the trigger
        if (other.CompareTag("Player"))
        {
            //the method returns a bool, making sure that the audio is only played, and the object is set in active if the player doesn't already have the weapon.
            if (gunHandler.OnWeaponPickup(gunTypeToPickup))
            {
                player.PlayerAudioSource.PlayOneShot(player.GunPickupAudio);
                gameObject.SetActive(false);
            }
        }
    }
}
