using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class GunHandler : MonoBehaviour
{
    //TODO: Consider making ammo properties in interface and implementing into different guntypes, as this would prevent the need for passing so many parameters
    public GunType CurrentGun { get { return currentGun; } }
    public Camera FPSCam { get { return fpsCam; } }

    public int HandGunCurrentAmmo { get { return handGunCurrentAmmo; } set { handGunCurrentAmmo = value; } }
    public int HandGunMaxAmmo { get { return handGunMaxAmmo; } }
    public int ShotGunCurrentAmmo { get { return shotGunCurrentAmmo; } set { shotGunCurrentAmmo = value; } }
    public int ShotGunMaxAmmo { get { return shotGunMaxAmmo; } }
    public int AutoGunCurrentAmmo { get { return autoGunCurrentAmmo; } set { autoGunCurrentAmmo = value; } }
    public int AutoGunMaxAmmo { get { return autoGunMaxAmmo; } }

    public bool Reloading { get { return reloading; } set { reloading = value; } }
    public bool InfiniteAmmo { get { return infiniteAmmo; } }

    public delegate void GunSwitchDelegate(GunHandler instance, IGun gun, WaitForSeconds reloadWait);
    public static GunSwitchDelegate weaponSwitched;

    public enum GunType
    {
        handGun,
        shotGun,
        longGun,
        autoGun
    }

    [Header("Gun Handler Parameters")]
    [SerializeField] private GunType currentGun;
    [SerializeField] private Transform shootFrom;
    [SerializeField] private Camera fpsCam;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private bool reloading;
    [SerializeField] private bool infiniteAmmo;
    private int ammoUI;


    [Header("Handgun Parameters")]
    [SerializeField] private float handGunBulletDamage = 10f;
    [SerializeField] private float handGunVerticalSpread;
    [SerializeField] private float handGunHorizontalSpread;
    [SerializeField] private int handGunCurrentAmmo;
    [SerializeField] private int handGunMaxAmmo;
    [SerializeField] private float handGunReloadTime;
    [Tooltip("This will offset how the shot is centered from the tip of the gun")]
    [SerializeField] private float handGunAimOffset = 15f;

    [Header("Shotgun Parameters")]
    [Tooltip("This will apply to EACH 'bullet' the shotgun fires")]
    [SerializeField] private float shotGunBulletDamage = 10f;
    [SerializeField] private int shotGunBulletAmount;
    [Tooltip("Increase for wider vertical spread. This will be used to a find a random number between the negative of this and the positive.")]
    [SerializeField] private float shotGunVerticalSpread;
    [Tooltip("Increase for wider horizontal spread. This will be used to a find a random number between the negative of this and the positive.")]
    [SerializeField] private float shotGunHorizontalSpread;
    [SerializeField] private int shotGunCurrentAmmo;
    [SerializeField] private int shotGunMaxAmmo;
    [SerializeField] private float shotGunReloadTime;
    [Tooltip("This will offset how the shot is centered from the tip of the gun")]
    [SerializeField] private float shotGunAimOffset = 15f;

    [Header("Autogun Parameters")]
    [SerializeField] private float autoGunBulletDamage = 10f;
    [SerializeField] private float autoGunHorizontalSpread;
    [SerializeField] private float autoGunVerticalSpread;
    [SerializeField] private int autoGunCurrentAmmo;
    [SerializeField] private int autoGunMaxAmmo;
    [SerializeField] private float autoGunReloadTime;
    [SerializeField] private float fireRate;
    [Tooltip("This will offset how the shot is centered from the tip of the gun")]
    [SerializeField] private float autoGunAimOffset = 15f;

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

    private Renderer gunRenderer;

    private Dictionary<int, IGun> gunDict;
    private Dictionary<GunType, WaitForSeconds> gunReloadWaitDict;

    //Particle Effects for the bullet collision.
    [SerializeField] private GameObject hitEnemy;
    [SerializeField] private GameObject hitNONEnemy;

    private void Awake()
    {
        autoGun = gameObject.AddComponent<AutoGun>();
        handGun = gameObject.AddComponent<HandGun>();
        shotGun = gameObject.AddComponent<ShotGun>();

        gunDict = new Dictionary<int, IGun>();
        gunReloadWaitDict = new Dictionary<GunType, WaitForSeconds>();

        fireRateWait = new WaitForSeconds(fireRate);
        handGunReloadWait = new WaitForSeconds(handGunReloadTime);
        shotGunReloadWait = new WaitForSeconds(shotGunReloadTime);
        autoGunReloadWait = new WaitForSeconds(autoGunReloadTime);

        gunRenderer = gameObject.GetComponent<Renderer>();

        lineDrawer = new GameObject();
    }

    private void Start()
    {
        handGunCurrentAmmo = handGunMaxAmmo;
        shotGunCurrentAmmo = shotGunMaxAmmo;
        autoGunCurrentAmmo = autoGunMaxAmmo;

        gunDict.Add(Array.IndexOf(guns, GunType.handGun), handGun);
        gunDict.Add(Array.IndexOf(guns, GunType.shotGun), shotGun);
        gunDict.Add(Array.IndexOf(guns, GunType.autoGun), autoGun);

        gunReloadWaitDict.Add(GunType.handGun, handGunReloadWait);
        gunReloadWaitDict.Add(GunType.shotGun, shotGunReloadWait);
        gunReloadWaitDict.Add(GunType.autoGun, autoGunReloadWait);

        lineRenderer = lineDrawer.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
    }

    private void Update()
    {
        if (reloading)
        {
            gunRenderer.material.color = Color.red;
        }
        else
        {
            gunRenderer.material.color = default;
        }

        if (currentGun == GunType.autoGun)
        {
            ammoUI = autoGunCurrentAmmo;
        }
        else if (currentGun == GunType.handGun)
        {
            ammoUI = handGunCurrentAmmo;
        }
        else if (currentGun == GunType.shotGun)
        {
            ammoUI = shotGunCurrentAmmo;
        }

        ammoText.text = $"Ammo: {ammoUI}";
    }

    public void SwitchWeapon(InputAction.CallbackContext context)
    {
        if (currentGun != guns.Last())
        {
            currentGun = guns[Array.IndexOf(guns, currentGun) + 1];
        }
        else
        {
            currentGun = guns[0];
        }

        if (reloading)
        {
            WaitForSeconds reloadToInvoke = gunReloadWaitDict[currentGun];

            weaponSwitched?.Invoke(this, gunDict[Array.IndexOf(guns, currentGun)], reloadToInvoke);
        }

        Debug.Log($"Equipped gun: {currentGun.ToString()}");
    }

    public void Shoot(InputAction.CallbackContext context)
    {
        if (currentGun == GunType.autoGun && autoGunCurrentAmmo > 0 && !reloading)
        {
            AutoGun.Shoot(this, autoGun, shootFrom, gameObject, playerLayer, context, fireRateWait, autoGunBulletDamage, autoGunVerticalSpread, autoGunHorizontalSpread, autoGunAimOffset, hitEnemy, hitNONEnemy);
        }
        if ((currentGun == GunType.handGun || currentGun == GunType.longGun) && context.performed && handGunCurrentAmmo > 0 && !reloading)
        {
            HandGun.Shoot(fpsCam, shootFrom, gameObject, playerLayer, handGunBulletDamage, handGunVerticalSpread, handGunHorizontalSpread, handGunAimOffset,hitEnemy,hitNONEnemy);
            if (!infiniteAmmo)
            {
                handGunCurrentAmmo--;
            }
        }
        
        if (currentGun == GunType.shotGun && context.performed && shotGunCurrentAmmo > 0 && !reloading)
        {
            ShotGun.Shoot(fpsCam, shootFrom, gameObject, playerLayer, shotGunBulletDamage, shotGunBulletAmount, shotGunVerticalSpread, shotGunHorizontalSpread, shotGunAimOffset,hitEnemy,hitNONEnemy);
            if (!infiniteAmmo)
            {
                shotGunCurrentAmmo--;
            }
        }
    }
    public void Reload(InputAction.CallbackContext context)
    {
        if (currentGun == GunType.autoGun && autoGunCurrentAmmo < autoGunMaxAmmo && !reloading)
        {
            AutoGun.StartReload(this, autoGun, autoGunReloadWait);
        }

        if (currentGun == GunType.handGun && handGunCurrentAmmo < handGunMaxAmmo && !reloading)
        {
            HandGun.StartReload(this, handGun, handGunReloadWait);
        }

        if (currentGun == GunType.shotGun && shotGunCurrentAmmo < shotGunMaxAmmo && !reloading)
        {
            ShotGun.StartReload(this, shotGun, shotGunReloadWait);
        }
    }
}
