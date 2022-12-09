using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class GunHandler : MonoBehaviour
{
    //TODO: Consider making ammo properties in interface and implementing into different guntypes, as this would prevent the need for passing so many parameters
    public IGun CurrentGun { get { return currentGun; } }
    public Camera FPSCam { get { return fpsCam; } }
    public AudioSource GunShotAudioSource { get { return gunShotAudioSource; } }
    
    public int HandGunCurrentAmmo { get { return handGunCurrentAmmo; } set { handGunCurrentAmmo = value; } }
    public int HandGunMaxAmmo { get { return handGunMaxAmmo; } }
    public int ShotGunCurrentAmmo { get { return shotGunCurrentAmmo; } set { shotGunCurrentAmmo = value; } }
    public int ShotGunMaxAmmo { get { return shotGunMaxAmmo; } }
    public int ShotGunBulletAmount { get { return shotGunBulletAmount; } }
    public int AutoGunCurrentAmmo { get { return autoGunCurrentAmmo; } set { autoGunCurrentAmmo = value; } }
    public int AutoGunMaxAmmo { get { return autoGunMaxAmmo; } }
    public int GrenadeGunCurrentAmmo { get { return grenadeGunCurrentAmmo; } set { grenadeGunCurrentAmmo = value; } }
    public int GrenadeGunMaxAmmo { get { return grenadeGunMaxAmmo; } }

    public bool Reloading { get { return reloading; } set { reloading = value; } }
    public bool InfiniteAmmo { get { return infiniteAmmo; } }

    public delegate void GunSwitchDelegate(WaitForSeconds reloadWait);
    public static GunSwitchDelegate weaponSwitched;

    public enum GunType
    {
        handGun,
        shotGun,
        grenadeGun,
        autoGun
    }

    [Header("Gun Handler Parameters")]
    [SerializeField] private GunType currentGunState;
    [SerializeField] private Camera fpsCam;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TrailRenderer bulletTrail;
    [SerializeField] private bool reloading;
    [SerializeField] private bool infiniteAmmo;
    [SerializeField] private LayerMask ignoreLayers;
    private int currentGunAmmo;
    private AudioSource gunShotAudioSource;


    [Header("Handgun Parameters")]
    [SerializeField] private GameObject handGunModel;
    [SerializeField] private Transform handGunShootFrom;
    [SerializeField] private float handGunBulletDamage = 10f;
    [SerializeField] private float handGunDamageDrop = 1f;
    [SerializeField] private float handGunVerticalSpread;
    [SerializeField] private float handGunHorizontalSpread;
    [SerializeField] private int handGunCurrentAmmo;
    [SerializeField] private int handGunMaxAmmo;
    [SerializeField] private float handGunReloadTime;
    [SerializeField] private float handGunFireRate;
    [Tooltip("This will offset how the shot is centered from the tip of the gun")]
    [SerializeField] private float handGunAimOffset = 15f;
    [SerializeField] private CanvasGroup handGunReticle;
    [SerializeField] private AudioClip handGunShotAudio;

    [Header("Shotgun Parameters")]
    [SerializeField] private GameObject shotGunModel;
    [SerializeField] private Transform shotGunShootFrom;
    [Tooltip("This will apply to EACH 'bullet' the shotgun fires")]
    [SerializeField] private float shotGunBulletDamage = 10f;
    [SerializeField] private float shotGunDamageDrop = 1f;
    [SerializeField] private int shotGunBulletAmount;
    [Tooltip("Increase for wider vertical spread. This will be used to a find a random number between the negative of this and the positive.")]
    [SerializeField] private float shotGunVerticalSpread;
    [Tooltip("Increase for wider horizontal spread. This will be used to a find a random number between the negative of this and the positive.")]
    [SerializeField] private float shotGunHorizontalSpread;
    [SerializeField] private int shotGunCurrentAmmo;
    [SerializeField] private int shotGunMaxAmmo;
    [SerializeField] private float shotGunReloadTime;
    [SerializeField] private float shotGunFireRate;
    [Tooltip("This will offset how the shot is centered from the tip of the gun")]
    [SerializeField] private float shotGunAimOffset = 15f;
    [SerializeField] private CanvasGroup shotGunReticle;
    [SerializeField] private AudioClip shotGunShotAudio;

    [Header("Autogun Parameters")]
    [SerializeField] private GameObject autoGunModel;
    [SerializeField] private Transform autoGunShootFrom;
    [SerializeField] private float autoGunBulletDamage = 10f;
    [SerializeField] private float autoGunDamageDrop = 1f;
    [SerializeField] private float autoGunHorizontalSpread;
    [SerializeField] private float autoGunVerticalSpread;
    [SerializeField] private int autoGunCurrentAmmo;
    [SerializeField] private int autoGunMaxAmmo;
    [SerializeField] private float autoGunReloadTime;
    [SerializeField] private float autoFireRate;
    [Tooltip("This will offset how the shot is centered from the tip of the gun")]
    [SerializeField] private float autoGunAimOffset = 15f;
    [SerializeField] private CanvasGroup autoGunReticle;
    [SerializeField] private AudioClip[] autoGunShotAudioList;
    [SerializeField] private AudioClip triggerReleasedAudio;

    [Header("Grenade Launcher Parameters")]
    [SerializeField] private GameObject grenadeObject;
    [SerializeField] private GameObject grenadeGunModel;
    [SerializeField] private Transform grenadeGunShootFrom;
    [SerializeField] private float grenadeDamage = 10f;
    [SerializeField] private float grenadeGunHorizontalSpread;
    [SerializeField] private float grenadeGunVerticalSpread;
    [SerializeField] private int grenadeGunCurrentAmmo;
    [SerializeField] private int grenadeGunMaxAmmo;
    [SerializeField] private float grenadeGunReloadTime;
    [SerializeField] private float grenadeFireRate;
    [Tooltip("This will offset how the shot is centered from the tip of the gun")]
    [SerializeField] private float grenadeGunAimOffset = 15f;
    [SerializeField] private CanvasGroup grenadeGunReticle;
    [SerializeField] private AudioClip grenadeGunShotAudio;

    [SerializeField] private List<GameObject> gunInventory;

    private bool holdingTrigger;

    private AutoGun autoGun;
    private HandGun handGun;
    private ShotGun shotGun;
    private GrenadeGun grenadeGun;
    private IGun currentGun;

    private GameObject lineDrawer;
    private LineRenderer lineRenderer;

    private List<GunType> guns;

    private WaitForSeconds fireRateWait;
    private WaitForSeconds handGunReloadWait;
    private WaitForSeconds shotGunReloadWait;
    private WaitForSeconds autoGunReloadWait;
    private WaitForSeconds grenadeGunReloadWait;

    private Renderer gunRenderer;

    private Dictionary<GunType, IGun> gunDict;
    private Dictionary<GunType, int> gunTypeDict;
    private Dictionary<GunType, WaitForSeconds> gunReloadWaitDict;

    //Particle Effects for the bullet collision.
    [SerializeField] private GameObject hitEnemy;
    [SerializeField] private GameObject hitNONEnemy;

    private void Awake()
    {
        fireRateWait = new WaitForSeconds(autoFireRate);

        handGunReloadWait = new WaitForSeconds(handGunReloadTime);
        shotGunReloadWait = new WaitForSeconds(shotGunReloadTime);
        autoGunReloadWait = new WaitForSeconds(autoGunReloadTime);
        grenadeGunReloadWait = new WaitForSeconds(grenadeGunReloadTime);

        autoGun = gameObject.AddComponent<AutoGun>();
        handGun = gameObject.AddComponent<HandGun>();
        shotGun = gameObject.AddComponent<ShotGun>();
        grenadeGun = gameObject.AddComponent<GrenadeGun>();

        PopulateGunProperties(autoGun);
        PopulateGunProperties(handGun);
        PopulateGunProperties(shotGun);
        PopulateGunProperties(grenadeGun);

        gunDict = new Dictionary<GunType, IGun>();
        gunTypeDict = new Dictionary<GunType, int>();
        gunReloadWaitDict = new Dictionary<GunType, WaitForSeconds>();

        gunRenderer = gameObject.GetComponent<Renderer>();

        gunShotAudioSource = gameObject.GetComponent<AudioSource>();

        lineDrawer = new GameObject();

    }


    private void PopulateGunProperties(IGun gun)
    {
        gun.GunManager = this;
        gun.LayerToIgnore = ignoreLayers;
        gun.HitEnemy = this.hitEnemy;
        gun.HitNonEnemy = this.hitNONEnemy;
        gun.BulletTrail = this.bulletTrail;

        if (gun is AutoGun)
        {
            gun.FireRate = this.autoFireRate;
            gun.GunModel = this.autoGunModel;
            gun.ShootFrom = this.autoGunShootFrom;
            gun.BulletDamage = this.autoGunBulletDamage;
            gun.VerticalSpread = this.autoGunVerticalSpread;
            gun.HorizontalSpread = this.autoGunHorizontalSpread;
            gun.AimOffset = this.autoGunAimOffset;
            gun.GunReticle = this.autoGunReticle;
            gun.DamageDrop = this.autoGunDamageDrop != 0 ? this.autoGunDamageDrop : 1;
            autoGun.GunShotAudioList = this.autoGunShotAudioList;
            autoGun.TriggerReleasedAudio = this.triggerReleasedAudio;
        }
        if (gun is HandGun)
        {
            gun.FireRate = this.handGunFireRate;
            gun.GunModel = this.handGunModel;
            gun.ShootFrom = this.handGunShootFrom;
            gun.BulletDamage = this.handGunBulletDamage;
            gun.VerticalSpread = this.handGunVerticalSpread;
            gun.HorizontalSpread = this.handGunHorizontalSpread;
            gun.AimOffset = this.handGunAimOffset;
            gun.GunReticle = this.handGunReticle;
            gun.GunShotAudio = this.handGunShotAudio;
            gun.DamageDrop = this.handGunDamageDrop != 0 ? this.handGunDamageDrop : 1;
        }
        if (gun is ShotGun)
        {
            gun.FireRate = this.shotGunFireRate; //TODO get rid of coroutine reloads
            gun.GunModel = this.shotGunModel;
            gun.ShootFrom = this.shotGunShootFrom;
            gun.BulletDamage = this.shotGunBulletDamage;
            gun.VerticalSpread = this.shotGunVerticalSpread;
            gun.HorizontalSpread = this.shotGunHorizontalSpread;
            gun.AimOffset = this.shotGunAimOffset;
            gun.GunReticle = this.shotGunReticle;
            gun.GunShotAudio = this.shotGunShotAudio;
            gun.DamageDrop = this.shotGunDamageDrop != 0 ? this.shotGunDamageDrop : 1;
        }
        if (gun is GrenadeGun)
        {
            gun.FireRate = this.grenadeFireRate;
            gun.GunModel = this.grenadeGunModel;
            gun.ShootFrom = this.grenadeGunShootFrom;
            gun.BulletDamage = this.grenadeDamage;
            gun.VerticalSpread = this.grenadeGunVerticalSpread;
            gun.HorizontalSpread = this.grenadeGunHorizontalSpread;
            gun.AimOffset = this.grenadeGunAimOffset;
            gun.ReloadWait = this.grenadeGunReloadWait;
            gun.GunReticle = this.grenadeGunReticle;
            gun.GunShotAudio = this.grenadeGunShotAudio;
            grenadeGun.Grenade = this.grenadeObject;
        }
    }
    private void Start()
    {
        guns = new List<GunType>() { GunType.handGun };

        handGunReticle.alpha = 0;
        shotGunReticle.alpha = 0;
        autoGunReticle.alpha = 0;
        grenadeGunReticle.alpha = 0;

        handGunModel.SetActive(false);
        shotGunModel.SetActive(false);
        autoGunModel.SetActive(false);
        grenadeGunModel.SetActive(false);

        handGunCurrentAmmo = handGunMaxAmmo;
        shotGunCurrentAmmo = shotGunMaxAmmo;
        autoGunCurrentAmmo = autoGunMaxAmmo;
        grenadeGunCurrentAmmo = grenadeGunMaxAmmo;

        gunDict.Add(GunType.handGun, handGun);
        gunDict.Add(GunType.shotGun, shotGun);
        gunDict.Add(GunType.autoGun, autoGun);
        gunDict.Add(GunType.grenadeGun, grenadeGun);

        gunTypeDict.Add(GunType.handGun, 0);
        gunTypeDict.Add(GunType.shotGun, 1);
        gunTypeDict.Add(GunType.autoGun, 2);
        gunTypeDict.Add(GunType.grenadeGun, 3);

        gunReloadWaitDict.Add(GunType.handGun, handGunReloadWait);
        gunReloadWaitDict.Add(GunType.shotGun, shotGunReloadWait);
        gunReloadWaitDict.Add(GunType.autoGun, autoGunReloadWait);
        gunReloadWaitDict.Add(GunType.grenadeGun, grenadeGunReloadWait);

        lineRenderer = lineDrawer.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        currentGun = gunDict[GunType.handGun];
        currentGun.GunReticle.alpha = 1;
        currentGun.GunModel.SetActive(true);
    }

    private void Update()
    {
        currentGunAmmo = currentGun.CurrentAmmo;

        ammoText.text = $"Ammo: {currentGunAmmo}/{currentGun.CurrentMaxAmmo}";
    }

    public void SwitchWeapon(InputAction.CallbackContext context)
    {
        float mouseScrollDirection = context.ReadValue<float>();
        //if (!IsOwner) return;
        currentGun.GunReticle.alpha = 0;
        currentGun.GunModel.SetActive(false);

        if (mouseScrollDirection < 0)
        {
            if (currentGunState != guns.Last())
            {
                currentGunState = guns[guns.IndexOf(currentGunState) + 1];
            }
            else
            {
                currentGunState = guns[0];
            }
        }
        if (mouseScrollDirection > 0)
        {
            if (currentGunState != guns[0])
            {
                currentGunState = guns[guns.IndexOf(currentGunState) - 1];
            }
            else
            {
                currentGunState = guns.Last();
            }
        }
        
        currentGun = gunDict[currentGunState];
        currentGun.GunReticle.alpha = 1;
        currentGun.GunModel.SetActive(true);

        WaitForSeconds reloadToInvoke = gunReloadWaitDict[currentGunState];
        weaponSwitched?.Invoke(reloadToInvoke);
    }

    public void OnWeaponPickup(GunType gunType)
    {
        if (gunTypeDict[gunType] > guns.Count - 1)
        {
            guns.Add(gunType);
        }
        else
        {
            guns.Insert(gunTypeDict[gunType], gunType);
        }

        currentGun.GunReticle.alpha = 0;
        currentGun.GunModel.SetActive(false);

        currentGunState = guns[guns.IndexOf(gunType)];
        currentGun = gunDict[currentGunState];
        currentGun.GunReticle.alpha = 1;
        currentGun.GunModel.SetActive(true);
    }

    public void Shoot(InputAction.CallbackContext context)
    {
        //if (currentGunAmmo > 0 && !reloading)
        //{
        //    currentGun.Shoot()
        //}

        //if (!IsOwner) return;

        currentGun.ShootTriggered(context);
        
        /*if (currentGunState == GunType.autoGun)
        {
            autoGun.ShootTriggered(context);
            RequestFireServerRpc();
        }
        if ((currentGunState == GunType.handGun || currentGunState == GunType.longGun) && context.performed && handGunCurrentAmmo > 0 && !reloading)
        {
            handGun.Shoot();
            RequestFireServerRpc();
        }

        if (currentGunState == GunType.shotGun && context.performed && shotGunCurrentAmmo > 0 && !reloading)
        {
            shotGun.Shoot();
            RequestFireServerRpc();
        }*/
    }

    [ServerRpc]
    private void RequestFireServerRpc()
    {
        FireClientRpc();
    }

    [ClientRpc]
    private void FireClientRpc()
    {
        //if (!IsOwner) ExecuteShoot();
    }

    private void ExecuteShoot()
    {
        handGun.Shoot();
    }

    // network stuff do not delete
    //public void ShootOther(InputAction.CallbackContext context)
    //{
    //    //if (currentGunAmmo > 0 && !reloading)
    //    //{
    //    //    currentGun.Shoot()
    //    //}

    //    if (currentGunState == GunType.autoGun)
    //    {
    //        autoGun.ShootTriggered(context);
    //    }
    //    if ((currentGunState == GunType.handGun || currentGunState == GunType.longGun) && context.performed && handGunCurrentAmmo > 0 && !reloading)
    //    {
    //        handGun.Shoot();
    //    }

    //    if (currentGunState == GunType.shotGun && context.performed && shotGunCurrentAmmo > 0 && !reloading)
    //    {
    //        shotGun.Shoot();
    //    }
    //}


    //[ServerRpc(RequireOwnership = false)]
    //void SyncShootingServerRPC()
    //{
    //    SyncShootingClientRPC();
    //}

    //[ClientRpc]
    //void SyncShootingClientRPC()
    //{
    //    var gunHandlers = FindObjectsOfType<GunHandler>();
    //    foreach (GunHandler gh in gunHandlers)
    //    {
    //        //gh.ShootOther();
    //    }
    //}


    public void Reload(InputAction.CallbackContext context)
    {
        currentGun.StartReload(currentGun.ReloadWait);

        //if (!IsOwner) return;
        //if (currentGunState == GunType.autoGun && autoGunCurrentAmmo < autoGunMaxAmmo && !reloading)
        //{
        //    autoGun.StartReload(autoGunReloadWait);
        //}

        //if (currentGunState == GunType.handGun && handGunCurrentAmmo < handGunMaxAmmo && !reloading)
        //{
        //    handGun.StartReload(handGunReloadWait);
        //}

        //if (currentGunState == GunType.shotGun && shotGunCurrentAmmo < shotGunMaxAmmo && !reloading)
        //{
        //    shotGun.StartReload(shotGunReloadWait);
        //}
    }
    public void InfAmmoActive()
    {
        infiniteAmmo = true;
    }
    public void InfAmmoInactive()
    {
        infiniteAmmo = false;
    }
}
