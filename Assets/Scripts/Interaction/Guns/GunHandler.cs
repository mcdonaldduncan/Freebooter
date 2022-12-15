using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public sealed class GunHandler : MonoBehaviour
{
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

    public delegate void GunSwitchDelegate();
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
    [Tooltip("At which distance should damage start to fall?")]
    [SerializeField] private float handGunDamageDropStart;
    [Tooltip("At which point should damage always be minimum?")]
    [SerializeField] private float handGunDamageDropEnd;
    [Tooltip("The maxmimum amount of damage the gun can do. This will always be the total damage if the enemy is closer than Damage Drop Start")]
    [SerializeField] private float handGunMaxDamage;
    [Tooltip("The minimum amount of damage the gun can do when the enemy is far away. This will always be the total if enemy is further than Damage Drop End")]
    [SerializeField] private float handGunMinDamage;
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
    [Tooltip("At which distance should damage start to fall?")]
    [SerializeField] private float shotGunDamageDropStart;
    [Tooltip("At which point should damage always be minimum?")]
    [SerializeField] private float shotGunDamageDropEnd;
    [Tooltip("The maxmimum amount of damage the gun can do. This will always be the total damage if the enemy is closer than Damage Drop Start")]
    [SerializeField] private float shotGunMaxDamage;
    [Tooltip("The minimum amount of damage the gun can do when the enemy is far away. This will always be the total if enemy is further than Damage Drop End")]
    [SerializeField] private float shotGunMinDamage;
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
    [Tooltip("At which distance should damage start to fall?")]
    [SerializeField] private float autoGunDamageDropStart;
    [Tooltip("At which point should damage always be minimum?")]
    [SerializeField] private float autoGunDamageDropEnd;
    [Tooltip("The maxmimum amount of damage the gun can do. This will always be the total damage if the enemy is closer than Damage Drop Start")]
    [SerializeField] private float autoGunMaxDamage;
    [Tooltip("The minimum amount of damage the gun can do when the enemy is far away. This will always be the total if enemy is further than Damage Drop End")]
    [SerializeField] private float autoGunMinDamage;
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

    private AutoGun autoGun;
    private HandGun handGun;
    private ShotGun shotGun;
    private GrenadeGun grenadeGun;
    private IGun currentGun;

    private List<GunType> guns;

    private WaitForSeconds handGunReloadWait;
    private WaitForSeconds shotGunReloadWait;
    private WaitForSeconds autoGunReloadWait;
    private WaitForSeconds grenadeGunReloadWait;

    private Dictionary<GunType, IGun> gunDict;
    private Dictionary<GunType, int> gunTypeDict;
    private Dictionary<GunType, WaitForSeconds> gunReloadWaitDict;

    //Particle Effects for the bullet collision.
    [SerializeField] private GameObject hitEnemy;
    [SerializeField] private GameObject hitNONEnemy;

    private void Awake()
    {
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

        gunShotAudioSource = gameObject.GetComponent<AudioSource>();
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
            gun.MinDamage = this.autoGunMinDamage;
            gun.MaxDamage = this.autoGunMaxDamage;
            gun.DropEnd = this.autoGunDamageDropEnd;
            gun.DropStart = this.autoGunDamageDropStart;
            gun.VerticalSpread = this.autoGunVerticalSpread;
            gun.HorizontalSpread = this.autoGunHorizontalSpread;
            gun.AimOffset = this.autoGunAimOffset;
            gun.GunReticle = this.autoGunReticle;
            autoGun.GunShotAudioList = this.autoGunShotAudioList;
            autoGun.TriggerReleasedAudio = this.triggerReleasedAudio;
            gun.ReloadWait = this.autoGunReloadWait;
        }
        if (gun is HandGun)
        {
            gun.FireRate = this.handGunFireRate;
            gun.GunModel = this.handGunModel;
            gun.ShootFrom = this.handGunShootFrom;
            gun.MinDamage = this.handGunMinDamage;
            gun.MaxDamage = this.handGunMaxDamage;
            gun.DropEnd = this.handGunDamageDropEnd;
            gun.DropStart = this.handGunDamageDropStart;
            gun.VerticalSpread = this.handGunVerticalSpread;
            gun.HorizontalSpread = this.handGunHorizontalSpread;
            gun.AimOffset = this.handGunAimOffset;
            gun.GunReticle = this.handGunReticle;
            gun.GunShotAudio = this.handGunShotAudio;
            gun.ReloadWait = this.handGunReloadWait;
        }
        if (gun is ShotGun)
        {
            gun.FireRate = this.shotGunFireRate; //TODO get rid of coroutine reloads
            gun.GunModel = this.shotGunModel;
            gun.ShootFrom = this.shotGunShootFrom;
            gun.MinDamage = this.shotGunMinDamage;
            gun.MaxDamage = this.shotGunMaxDamage;
            gun.DropEnd = this.shotGunDamageDropEnd;
            gun.DropStart = this.shotGunDamageDropStart;
            gun.VerticalSpread = this.shotGunVerticalSpread;
            gun.HorizontalSpread = this.shotGunHorizontalSpread;
            gun.AimOffset = this.shotGunAimOffset;
            gun.GunReticle = this.shotGunReticle;
            gun.GunShotAudio = this.shotGunShotAudio;
            gun.ReloadWait = this.shotGunReloadWait;
        }
        if (gun is GrenadeGun)
        {
            gun.FireRate = this.grenadeFireRate;
            gun.GunModel = this.grenadeGunModel;
            gun.ShootFrom = this.grenadeGunShootFrom;
            gun.VerticalSpread = this.grenadeGunVerticalSpread;
            gun.HorizontalSpread = this.grenadeGunHorizontalSpread;
            gun.AimOffset = this.grenadeGunAimOffset;
            gun.ReloadWait = this.grenadeGunReloadWait;
            gun.GunReticle = this.grenadeGunReticle;
            gun.GunShotAudio = this.grenadeGunShotAudio;
            grenadeGun.Grenade = this.grenadeObject;
            grenadeGun.GrenadeDamage = this.grenadeDamage;
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

        currentGun = gunDict[GunType.handGun];
        currentGun.GunReticle.alpha = 1;
        currentGun.GunModel.SetActive(true);
    }

    private void Update()
    {
        //currentGunAmmo = currentGun.CurrentAmmo;

        // change this to either an event or check that only updates when necessary, lots of garbage collection from this
        ammoText.text = $"Ammo: {currentGun.CurrentAmmo}/{currentGun.CurrentMaxAmmo}";
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

        weaponSwitched?.Invoke();
    }

    public void OnWeaponPickup(GunType gunType)
    {
        if (!guns.Contains(gunType))
        {
            if (gunTypeDict[gunType] > guns.Count - 1)
            {
                guns.Add(gunType);
            }
            else
            {
                guns.Insert(gunTypeDict[gunType], gunType);
            }
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
        if (currentGun.CurrentAmmo <= 0)
        {
            currentGun.StartReload();
        }
        currentGun.ShootTriggered(context);
    }

    public void Reload(InputAction.CallbackContext context)
    {
        if (currentGun.CurrentAmmo < currentGun.CurrentMaxAmmo)
        {
            currentGun.StartReload();
        }
    }
}