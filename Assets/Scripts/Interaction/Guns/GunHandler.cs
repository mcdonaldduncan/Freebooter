using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class GunHandler : MonoBehaviour
{
    [SerializeField]
    private enum GunType
    {
        handGun,
        shotGun,
        longGun,
        autoGun
    }

    [Header("Gun Handler Parameters")]
    [SerializeField] private GunType gunType;
    [SerializeField] private Transform shootFrom;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private TextMeshProUGUI ammoText;
    private int ammoUI;
    public bool reloading;
    public bool infiniteAmmo;


    [Header("Handgun Parameters")]
    [SerializeField] private float handGunBulletDamage = 10f;
    [SerializeField] private float handGunVerticalSpread;
    [SerializeField] private float handGunHorizontalSpread;
    public int handGunCurrentAmmo;
    public int handGunMaxAmmo;
    [SerializeField] private float handGunReloadTime;

    [Header("Shotgun Parameters")]
    [Tooltip("This will apply to EACH 'bullet' the shotgun fires")]
    [SerializeField] private float shotGunBulletDamage = 10f;
    [SerializeField] private int shotGunBulletAmount;
    [Tooltip("Increase for wider vertical spread. This will be used to a find a random number between the negative of this and the positive.")]
    [SerializeField] private float shotGunVerticalSpread;
    [Tooltip("Increase for wider horizontal spread. This will be used to a find a random number between the negative of this and the positive.")]
    [SerializeField] private float shotGunHorizontalSpread;
    public int shotGunCurrentAmmo;
    public int shotGunMaxAmmo;
    [SerializeField] private float shotGunReloadTime;

    [Header("Autogun Parameters")]
    [SerializeField] private float autoGunBulletDamage = 10f;
    [SerializeField] private float autoGunHorizontalSpread;
    [SerializeField] private float autoGunVerticalSpread;
    public int autoGunCurrentAmmo;
    public int autoGunMaxAmmo;
    [SerializeField] private float autoGunReloadTime;
    [SerializeField] private float fireRate;

    [Header("Longgun Parameters")]
    [SerializeField] private float longGunBulletDamage = 10f;

    [SerializeField] private List<GameObject> gunInventory;

    private bool holdingTrigger;

    private AutoGun autoGun;
    private HandGun handGun;
    private ShotGun shotGun;

    private GameObject lineDrawer;
    private LineRenderer lineRenderer;

    private GunType[] guns = {GunType.handGun, GunType.shotGun, GunType.autoGun};

    private WaitForSeconds fireRateWait;
    private WaitForSeconds handGunReloadWait;
    private WaitForSeconds shotGunReloadWait;
    private WaitForSeconds autoGunReloadWait;

    private void Start()
    {
        handGunCurrentAmmo = handGunMaxAmmo;
        shotGunCurrentAmmo = shotGunMaxAmmo;
        autoGunCurrentAmmo = autoGunMaxAmmo;

        fireRateWait = new WaitForSeconds(fireRate);
        handGunReloadWait = new WaitForSeconds(handGunReloadTime);
        shotGunReloadWait = new WaitForSeconds(shotGunReloadTime);
        autoGunReloadWait = new WaitForSeconds(autoGunReloadTime);

        autoGun = new AutoGun();
        handGun = new HandGun();
        shotGun = new ShotGun();

        lineDrawer = new GameObject();
        lineRenderer = lineDrawer.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
    }

    private void Update()
    {
        if (gunType == GunType.autoGun)
        {
            ammoUI = autoGunCurrentAmmo;
        }
        else if (gunType == GunType.handGun)
        {
            ammoUI = handGunCurrentAmmo;
        }
        else if (gunType == GunType.shotGun)
        {
            ammoUI = shotGunCurrentAmmo;
        }

        ammoText.text = $"Ammo: {ammoUI}";
    }

    public void SwitchWeapon(InputAction.CallbackContext context)
    {
        if (gunType != guns.Last())
        {
            gunType = guns[Array.IndexOf(guns, gunType) + 1];
        }
        else
        {
            gunType = guns[0];
        }
        Debug.Log($"Equipped gun: {gunType.ToString()}");
    }

    public void Shoot(InputAction.CallbackContext context)
    {
        if (gunType == GunType.autoGun && autoGunCurrentAmmo > 0 && !reloading)
        {
            AutoGun.Shoot(this, autoGun, shootFrom, gameObject, playerLayer, context, fireRateWait, autoGunBulletDamage, autoGunVerticalSpread, autoGunHorizontalSpread);
        }
        if ((gunType == GunType.handGun || gunType == GunType.longGun) && context.performed && handGunCurrentAmmo > 0 && !reloading)
        {
            HandGun.Shoot(shootFrom, gameObject, playerLayer, handGunBulletDamage, handGunVerticalSpread, handGunHorizontalSpread);
            if (!infiniteAmmo)
            {
                handGunCurrentAmmo--;
            }
        }
        
        if (gunType == GunType.shotGun && context.performed && shotGunCurrentAmmo > 0 && !reloading)
        {
            ShotGun.Shoot(shootFrom, gameObject, playerLayer, shotGunBulletDamage, shotGunBulletAmount, shotGunVerticalSpread, shotGunHorizontalSpread);
            if (!infiniteAmmo)
            {
                shotGunCurrentAmmo--;
            }
        }
    }

    public void Reload(InputAction.CallbackContext context)
    {
        if (gunType == GunType.autoGun && autoGunCurrentAmmo < autoGunMaxAmmo && !reloading)
        {
            AutoGun.StartReload(this, autoGun, autoGunReloadWait);
        }

        if (gunType == GunType.handGun && handGunCurrentAmmo < handGunMaxAmmo && !reloading)
        {
            HandGun.StartReload(this, handGun, handGunReloadWait);
        }

        if (gunType == GunType.shotGun && shotGunCurrentAmmo < shotGunMaxAmmo && !reloading)
        {
            ShotGun.StartReload(this, shotGun, shotGunReloadWait);
        }
    }
}
