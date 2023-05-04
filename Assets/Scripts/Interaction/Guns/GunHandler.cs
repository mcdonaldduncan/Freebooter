using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public sealed class GunHandler : MonoBehaviour
{
    public IGun CurrentGun { get { return currentGun; } }
    public Dictionary<GunType, IGun> GunDict { get { return gunDict; } }
    public List<GunType> GunTypeList { get { return guns; } }
    public Camera FPSCam { get { return fpsCam; } }
    public AudioSource GunShotAudioSource { get { return gunShotAudioSource; } }
    public GameObject MuzzleFlash { get { return muzzleFlash; } }
    
    //public int HandGunCurrentAmmo { get { return handGunCurrentAmmo; } set { handGunCurrentAmmo = value; } }
    //public int HandGunMaxAmmo { get { return handGunMaxAmmo; } }
    public int ShotGunCurrentAmmo { get { return shotGunCurrentAmmo; } set { shotGunCurrentAmmo = value; } }
    public int ShotGunMaxAmmo { get { return shotGunMaxAmmo; } }
    public int ShotGunBulletAmount { get { return shotGunBulletAmount; } }
    public int AutoGunCurrentAmmo { get { return autoGunCurrentAmmo; } set { autoGunCurrentAmmo = value; } }
    public int AutoGunMaxAmmo { get { return autoGunMaxAmmo; } }
    public int GrenadeGunCurrentAmmo { get { return grenadeGunCurrentAmmo; } set { grenadeGunCurrentAmmo = value; } }
    public int GrenadeGunMaxAmmo { get { return grenadeGunMaxAmmo; } }

    //public bool Reloading { get { return reloading; } set { reloading = value; } }
    public bool InfiniteAmmo { get { return infiniteAmmo; } }

    public delegate void GunSwitchDelegate();
    public static event GunSwitchDelegate weaponSwitched;

    public enum GunType
    {
        //handGun,
        shotGun,
        grenadeGun,
        autoGun
    }

    [Header("Gun Handler Parameters")]
    [SerializeField] private GunType currentGunState;
    [SerializeField] private Camera fpsCam;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private TextMeshProUGUI ammoText;
    [Tooltip("Whatever object goes here must have a Trail Renderer component!")]
    [SerializeField] private GameObject bulletTrail;
    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private bool reloading;
    [SerializeField] private bool infiniteAmmo;
    [SerializeField] private LayerMask ignoreLayers;
    private int currentGunAmmo;
    private AudioSource gunShotAudioSource;


    //[Header("Handgun Parameters")]
    //[SerializeField] private GameObject handGunModel;
    //[SerializeField] private Transform handGunShootFrom;
    //[Tooltip("At which distance should damage start to fall?")]
    //[SerializeField] private float handGunDamageDropStart;
    //[Tooltip("At which point should damage always be minimum?")]
    //[SerializeField] private float handGunDamageDropEnd;
    //[Tooltip("The maxmimum amount of damage the gun can do. This will always be the total damage if the enemy is closer than Damage Drop Start")]
    //[SerializeField] private float handGunMaxDamage;
    //[Tooltip("The minimum amount of damage the gun can do when the enemy is far away. This will always be the total if enemy is further than Damage Drop End")]
    //[SerializeField] private float handGunMinDamage;
    //[SerializeField] private float handGunVerticalSpread;
    //[SerializeField] private float handGunHorizontalSpread;
    //[SerializeField] private int handGunCurrentAmmo;
    //[SerializeField] private int handGunMaxAmmo;
    //[SerializeField] private float handGunReloadTime;
    //[Tooltip("Number of seconds between shots")]
    //[SerializeField] private float handGunFireRate;
    //[Tooltip("This will offset how the shot is centered from the tip of the gun")]
    //[SerializeField] private float handGunAimOffset = 15f;
    //[SerializeField] private CanvasGroup handGunReticle;
    //[SerializeField] private AudioClip handGunShotAudio;
    //[SerializeField] private HandgunAnimationHandler handgunAnimationHandler;

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
    [Tooltip("Number of seconds between shots")]
    [SerializeField] private float shotGunFireRate;
    [Tooltip("This will offset how the shot is centered from the tip of the gun")]
    [SerializeField] private float shotGunAimOffset = 15f;
    [SerializeField] private CanvasGroup shotGunReticle;
    [SerializeField] private AudioClip shotGunShotAudio;
    [SerializeField] private GunAnimationHandler shotgunAnimationHandler;
    [SerializeField] private float shotShakeDuration;
    [SerializeField] private float shotShakeMagnitude;
    [SerializeField] private float shotShakeDampen;

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
    [Tooltip("Number of seconds between shots")]
    [SerializeField] private float autoFireRate;
    [Tooltip("This will offset how the shot is centered from the tip of the gun")]
    [SerializeField] private float autoGunAimOffset = 15f;
    [SerializeField] private CanvasGroup autoGunReticle;
    [SerializeField] private AudioClip[] autoGunShotAudioList;
    //[SerializeField] private AudioClip triggerReleasedAudio;
    [SerializeField] private GunAnimationHandler autoGunAnimationHandler;
    [SerializeField] private float autoShakeDuration;
    [SerializeField] private float autoShakeMagnitude;
    [SerializeField] private float autoShakeDampen;

    [Header("Grenade Launcher Parameters")]
    [SerializeField] private GameObject grenadeObject;
    [SerializeField] private GameObject grenadeGunModel;
    [SerializeField] private Transform grenadeGunShootFrom;
    [SerializeField] private float grenadeDamage = 10f;
    [SerializeField] private float grenadeGunHorizontalSpread;
    [SerializeField] private float grenadeGunVerticalSpread;
    [SerializeField] private int grenadeGunCurrentAmmo;
    [SerializeField] private int grenadeGunMaxAmmo;
    [SerializeField] private float grenadeFireRate;
    [Tooltip("This will offset how the shot is centered from the tip of the gun")]
    [SerializeField] private float grenadeGunAimOffset = 15f;
    [SerializeField] private CanvasGroup grenadeGunReticle;
    [SerializeField] private AudioClip grenadeGunShotAudio;
    [SerializeField] private float grenadeLaunchForce;
    [SerializeField] private float grenadeLaunchArc;
    [SerializeField] private float grenadeGunShakeDuration;
    [SerializeField] private float grenadeGunShakeMagnitude;
    [SerializeField] private float grenadeGunShakeDampen;

    [SerializeField] private List<GameObject> gunInventory;

    private AutoGun autoGun;
    private HandGun handGun;
    private ShotGun shotGun;
    private GrenadeGun grenadeGun;
    private IGun currentGun;

    private List<GunType> guns;

    private Dictionary<GunType, IGun> gunDict;
    private Dictionary<GunType, int> gunTypeDict;
    private Dictionary<GunType, WaitForSeconds> gunReloadWaitDict;

    private int autoGunAmmoCP;  
    private int shotGunAmmoCP;
    private int grenadeGunAmmoCP;

    //Particle Effects for the bullet collision.
    [SerializeField] private GameObject hitEnemy;
    [SerializeField] private GameObject hitNONEnemy;

    public delegate void AmmoNotificationDelegate();
    public event AmmoNotificationDelegate AmmoEmpty;


    public delegate void AmmoPickupDelegate(int amount, IGun gun);
    public event AmmoPickupDelegate AmmoPickup;

    private void Awake()
    {
        autoGun = gameObject.AddComponent<AutoGun>();
        //handGun = gameObject.AddComponent<HandGun>();
        shotGun = gameObject.AddComponent<ShotGun>();
        grenadeGun = gameObject.AddComponent<GrenadeGun>();

        PopulateGunProperties(autoGun);
        //PopulateGunProperties(handGun);
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
        gun.Bullet = this.bulletTrail;

        if (gun is AutoGun)
        {
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
            //autoGun.TriggerReleasedAudio = this.triggerReleasedAudio;
            //gun.ReloadWait = this.autoGunReloadWait;
            gun.GunAnimationHandler = this.autoGunAnimationHandler;
            gun.FireRate = this.autoFireRate;
            gun.ShakeDuration = this.autoShakeDuration;
            gun.ShakeMagnitude = this.autoShakeMagnitude;
            gun.ShakeDampen = this.autoShakeDampen;
        }
        //if (gun is HandGun)
        //{
        //    gun.GunModel = this.handGunModel;
        //    gun.ShootFrom = this.handGunShootFrom;
        //    gun.MinDamage = this.handGunMinDamage;
        //    gun.MaxDamage = this.handGunMaxDamage;
        //    gun.DropEnd = this.handGunDamageDropEnd;
        //    gun.DropStart = this.handGunDamageDropStart;
        //    gun.VerticalSpread = this.handGunVerticalSpread;
        //    gun.HorizontalSpread = this.handGunHorizontalSpread;
        //    gun.AimOffset = this.handGunAimOffset;
        //    gun.GunReticle = this.handGunReticle;
        //    gun.GunShotAudio = this.handGunShotAudio;
        //    //gun.ReloadWait = this.handGunReloadWait;
        //    handGun.GunAnimationHandler = this.handgunAnimationHandler;
        //    gun.FireRate = this.handGunFireRate;
        //}
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
            //gun.ReloadWait = this.shotGunReloadWait;
            gun.GunAnimationHandler = this.shotgunAnimationHandler;
            gun.FireRate = this.shotGunFireRate;
            gun.ShakeDuration = this.shotShakeDuration;
            gun.ShakeMagnitude = this.shotShakeMagnitude;
            gun.ShakeDampen = this.shotShakeDampen;
        }
        if (gun is GrenadeGun)
        {
            gun.FireRate = this.grenadeFireRate;
            gun.GunModel = this.grenadeGunModel;
            gun.ShootFrom = this.grenadeGunShootFrom;
            gun.VerticalSpread = this.grenadeGunVerticalSpread;
            gun.HorizontalSpread = this.grenadeGunHorizontalSpread;
            gun.AimOffset = this.grenadeGunAimOffset;
            gun.GunReticle = this.grenadeGunReticle;
            gun.GunShotAudio = this.grenadeGunShotAudio;
            gun.ShakeDuration = this.grenadeGunShakeDuration;
            gun.ShakeMagnitude = this.grenadeGunShakeMagnitude;
            gun.ShakeDampen = this.grenadeGunShakeDampen;
            grenadeGun.Grenade = this.grenadeObject;
            grenadeGun.GrenadeDamage = this.grenadeDamage;
            grenadeGun.GrenadeLaunchForce = this.grenadeLaunchForce;
            grenadeGun.GrenadeLaunchArcVector = new Vector3(0f, grenadeLaunchArc, 0f);
        }
    }
    private void Start()
    {
        guns = new List<GunType>() { GunType.shotGun };

        //handGunReticle.alpha = 0;
        shotGunReticle.alpha = 0;
        autoGunReticle.alpha = 0;
        grenadeGunReticle.alpha = 0;

        //handGunModel.SetActive(false);
        shotGunModel.SetActive(false);
        autoGunModel.SetActive(false);
        grenadeGunModel.SetActive(false);

        //handGunCurrentAmmo = handGunMaxAmmo;
        shotGunCurrentAmmo = shotGunMaxAmmo;
        autoGunCurrentAmmo = autoGunMaxAmmo;
        grenadeGunCurrentAmmo = grenadeGunMaxAmmo;

        //gunDict.Add(GunType.handGun, handGun);
        gunDict.Add(GunType.shotGun, shotGun);
        gunDict.Add(GunType.autoGun, autoGun);
        gunDict.Add(GunType.grenadeGun, grenadeGun);

        //gunTypeDict.Add(GunType.handGun, 0);
        gunTypeDict.Add(GunType.shotGun, 0);
        gunTypeDict.Add(GunType.autoGun, 1);
        gunTypeDict.Add(GunType.grenadeGun, 2);

        currentGun = gunDict[GunType.shotGun];
        currentGun.GunReticle.alpha = 1;
        currentGun.GunModel.SetActive(true);
        currentGun.GunAnimationHandler.RecoilAnim.SetFloat("RecoilSpeed", currentGun.GunAnimationHandler.RecoilAnimClip.length / currentGun.FireRate);

        LevelManager.Instance.CheckPointReached += OnCheckpointReached;
        LevelManager.Instance.PlayerRespawn += OnPlayerRespawn;

        UpdateAmmoDisplay();

        //this.handgunAnimationHandler.RecoilAnim.SetFloat("RecoilSpeed", this.handgunAnimationHandler.RecoilAnimClip.length / this.handGunFireRate);
        //this.shotgunAnimationHandler.RecoilAnim.SetFloat("RecoilSpeed", this.shotgunAnimationHandler.RecoilAnimClip.length / this.shotGunFireRate);
        //this.autoGunAnimationHandler.RecoilAnim.SetFloat("RecoilSpeed", this.autoGunAnimationHandler.RecoilAnimClip.length / this.autoFireRate);
    }

    public void UpdateAmmoDisplay()
    {
        bool isUnlimitedGun = currentGun is ShotGun;
        ammoText.text = $"{(isUnlimitedGun ? "\u221E" : currentGun.CurrentAmmo)}";
    }

    public void SwitchWeapon(InputAction.CallbackContext context)
    {
        float mouseScrollDirection = context.ReadValue<float>();
        //if (!IsOwner) return;
        currentGun.GunReticle.alpha = 0;
        currentGun.GunModel.SetActive(false);

        if (context.control.device is Gamepad)
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

        //scroll down
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

        //scroll up
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
        if (currentGun.GunAnimationHandler != null)
        {
            currentGun.GunAnimationHandler.RecoilAnim.SetFloat("RecoilSpeed", currentGun.GunAnimationHandler.RecoilAnimClip.length / currentGun.FireRate);
        }

        weaponSwitched?.Invoke();
        UpdateAmmoDisplay();
    }

    //Behavior for picking up the weapon and adding it to the inventory
    public bool OnWeaponPickup(GunType gunType)
    {
        bool pickedUp = false;

        //if the gun is not already in the inventory
        if (!guns.Contains(gunType))
        {
            //Check to make sure that the predetermined position is not greater than the size of the inventory list
            //If it is, then simply add to the end of the list
            if (gunTypeDict[gunType] > guns.Count - 1)
            {
                guns.Add(gunType);
            }
            else
            {
                //Add the gun to the inventory at the proper position in the inventory
                guns.Insert(gunTypeDict[gunType], gunType);
            }

            pickedUp = true;
        }
        else if (guns.Contains(gunType) && gunDict[gunType].CurrentAmmo < gunDict[gunType].MaxAmmo) //If they have the gun already, refill ammo
        {
            OnAmmoPickup(gunType, gunDict[gunType].MaxAmmo - gunDict[gunType].CurrentAmmo);

            pickedUp = true;
        }

        //Behavior for equipping newly picked up gun
        currentGun.GunReticle.alpha = 0;
        currentGun.GunModel.SetActive(false);

        currentGunState = guns[guns.IndexOf(gunType)];
        currentGun = gunDict[currentGunState];

        currentGun.CurrentAmmo = currentGun.MaxAmmo;
        currentGun.GunReticle.alpha = 1;
        currentGun.GunModel.SetActive(true);
        if (currentGun.GunAnimationHandler != null)
        {
            currentGun.GunAnimationHandler.RecoilAnim.SetFloat("RecoilSpeed", currentGun.GunAnimationHandler.RecoilAnimClip.length / currentGun.FireRate);
        }

        weaponSwitched?.Invoke();
        UpdateAmmoDisplay();

        return pickedUp;
    }

    public void OnAmmoPickup(GunType gunType, int ammoToAdd)
    {
        IGun gunToRecieveAmmo = gunDict[gunType];
        int temp = ammoToAdd;

        if (gunToRecieveAmmo.CurrentAmmo + ammoToAdd >= gunToRecieveAmmo.MaxAmmo)
        {
            temp = gunToRecieveAmmo.MaxAmmo - gunToRecieveAmmo.CurrentAmmo;
        }

        
        gunToRecieveAmmo.CurrentAmmo += ammoToAdd;



        if (gunToRecieveAmmo.CurrentAmmo >= gunToRecieveAmmo.MaxAmmo)
        {
            gunToRecieveAmmo.CurrentAmmo = gunToRecieveAmmo.MaxAmmo;
        }

        AmmoPickup?.Invoke(temp, gunToRecieveAmmo);
        UpdateAmmoDisplay();
    }

    public void Shoot(InputAction.CallbackContext context)
    {
        if (currentGun.CurrentAmmo <= 0)
        {
            AmmoEmpty?.Invoke();
        }
        currentGun.ShootTriggered(context);
        UpdateAmmoDisplay();
    }

    public void AlternateShoot(InputAction.CallbackContext context)
    {
        currentGun.AlternateTriggered(context);
        UpdateAmmoDisplay();
    }

    private void OnCheckpointReached()
    {
        autoGunAmmoCP = autoGun.CurrentAmmo;
        //shotGunAmmoCP = shotGun.CurrentAmmo;
        grenadeGunAmmoCP = grenadeGun.CurrentAmmo;
    }

    private void OnPlayerRespawn()
    {
        autoGun.CurrentAmmo = autoGunAmmoCP;
        //shotGun.CurrentAmmo = shotGunAmmoCP;
        grenadeGun.CurrentAmmo = grenadeGunAmmoCP;
        UpdateAmmoDisplay();
    }

    //public void Reload(InputAction.CallbackContext context)
    //{
    //    if (currentGun.CurrentAmmo < currentGun.MaxAmmo)
    //    {
    //        currentGun.StartReload();
    //    }
    //}
}