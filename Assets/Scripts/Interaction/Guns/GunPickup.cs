using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunPickup : MonoBehaviour
{
    [SerializeField] private GunHandler.GunType gunTypeToPickup;
    private GunHandler gunHandler;

    private void Start()
    {
        gunHandler = GameObject.FindWithTag("Player").GetComponentInChildren<GunHandler>();
    }

    private void OnTriggerEnter(Collider other)
    {
        gunHandler.OnWeaponPickup(gunTypeToPickup);
        Destroy(gameObject);
    }
}
