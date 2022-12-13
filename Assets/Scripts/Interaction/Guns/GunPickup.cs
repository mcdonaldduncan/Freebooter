using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunPickup : MonoBehaviour
{
    [SerializeField] private GunHandler.GunType gunTypeToPickup;
    private GunHandler gunHandler;

    private void Start()
    {
        gunHandler = LevelManager.Instance.Player.GetComponentInChildren<GunHandler>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gunHandler.OnWeaponPickup(gunTypeToPickup);
            Destroy(gameObject);
        }
    }
}
