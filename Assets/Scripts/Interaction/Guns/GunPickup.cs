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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (gunHandler.OnWeaponPickup(gunTypeToPickup))
            {
                player.PlayerAudioSource.PlayOneShot(player.GunPickupAudio);
                gameObject.SetActive(false);
            }
        }
    }
}
