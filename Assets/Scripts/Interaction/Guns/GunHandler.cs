using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class GunHandler : NetworkBehaviour
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

    public bool Reloading { get { return reloading; } set { reloading = value; } }
    public bool InfiniteAmmo { get { return infiniteAmmo; } }

    public delegate void GunSwitchDelegate(WaitForSeconds reloadWait);
    public static GunSwitchDelegate weaponSwitched;

    public enum GunType
    {
        handGun,
        shotGun,
        longGun,
        autoGun
    }

    [Header("Gun Handler Parameters")]
    [SerializeField] private GunType currentGunState;
    [SerializeField] private Transform shootFrom;
    [SerializeField] private Camera fpsCam;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TrailRenderer bulletTrail;
    [SerializeField] private bool reloading;
    [SerializeField] private bool infiniteAmmo;
    private int currentGunAmmo;
    private AudioSource gunShotAudioSource;


    [Header("Handgun Parameters")]
    [SerializeField] private float handGunBulletDamage = 10f;
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
    [SerializeField] private float shotGunFireRate;
    [Tooltip("This will offset how the shot is centered from the tip of the gun")]
    [SerializeField] private float shotGunAimOffset = 15f;
    [SerializeField] private CanvasGroup shotGunReticle;
    [SerializeField] private AudioClip shotGunShotAudio;

    [Header("Autogun Parameters")]
    [SerializeField] private float autoGunBulletDamage = 10f;
    [SerializeField] private float autoGunHorizontalSpread;
    [SerializeField] private float autoGunVerticalSpread;
    [SerializeField] private int autoGunCurrentAmmo;
    [SerializeField] private int autoGunMaxAmmo;
    [SerializeField] private float autoGunReloadTime;
    [SerializeField] private float autoFireRate;
    [Tooltip("This will offset how the shot is centered from the tip of the gun")]
    [SerializeField] private float autoGunAimOffset = 15f;
    [SerializeField] private CanvasGroup autoGunReticle;
    [SerializeField] private AudioClip autoGunShotAudio;

    [Header("Longgun Parameters")]
    [SerializeField] private float longGunBulletDamage = 10f;

    [SerializeField] private List<GameObject> gunInventory;

    private bool holdingTrigger;

    private AutoGun autoGun;
    private HandGun handGun;
    private ShotGun shotGun;
    private IGun currentGun;

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
        fireRateWait = new WaitForSeconds(autoFireRate);

        handGunReloadWait = new WaitForSeconds(handGunReloadTime);
        shotGunReloadWait = new WaitForSeconds(shotGunReloadTime);
        autoGunReloadWait = new WaitForSeconds(autoGunReloadTime);

        autoGun = gameObject.AddComponent<AutoGun>();
        handGun = gameObject.AddComponent<HandGun>();
        shotGun = gameObject.AddComponent<ShotGun>();

        PopulateGunProperties(autoGun);
        PopulateGunProperties(handGun);
        PopulateGunProperties(shotGun);

        gunDict = new Dictionary<int, IGun>();
        gunReloadWaitDict = new Dictionary<GunType, WaitForSeconds>();

        gunRenderer = gameObject.GetComponent<Renderer>();

        gunShotAudioSource = gameObject.GetComponent<AudioSource>();

        lineDrawer = new GameObject();

    }


    private void PopulateGunProperties(IGun gun)
    {
        gun.GunManager = this;
        gun.ShootFrom = this.shootFrom;
        gun.LayerToIgnore = this.playerLayer;
        gun.HitEnemy = this.hitEnemy;
        gun.HitNonEnemy = this.hitNONEnemy;
        gun.BulletTrail = this.bulletTrail;

        if (gun is AutoGun)
        {
            gun.FireRate = this.autoFireRate;
            gun.BulletDamage = this.autoGunBulletDamage;
            gun.VerticalSpread = this.autoGunVerticalSpread;
            gun.HorizontalSpread = this.autoGunHorizontalSpread;
            gun.AimOffset = this.autoGunAimOffset;
            gun.ReloadTime = this.autoGunReloadTime;
            gun.GunReticle = this.autoGunReticle;
            gun.GunShotAudio = this.autoGunShotAudio;
        }
        if (gun is HandGun)
        {
            gun.FireRate = this.handGunFireRate;
            gun.BulletDamage = this.handGunBulletDamage;
            gun.VerticalSpread = this.handGunVerticalSpread;
            gun.HorizontalSpread = this.handGunHorizontalSpread;
            gun.AimOffset = this.handGunAimOffset;
            gun.ReloadTime = this.handGunReloadTime;
            gun.GunReticle = this.handGunReticle;
            gun.GunShotAudio = this.handGunShotAudio;
        }
        if (gun is ShotGun)
        {
            gun.FireRate = this.shotGunFireRate; //TODO get rid of coroutine reloads
            gun.BulletDamage = this.shotGunBulletDamage;
            gun.VerticalSpread = this.shotGunVerticalSpread;
            gun.HorizontalSpread = this.shotGunHorizontalSpread;
            gun.AimOffset = this.shotGunAimOffset;
            gun.ReloadTime = this.shotGunReloadTime;
            gun.GunReticle = this.shotGunReticle;
            gun.GunShotAudio = this.shotGunShotAudio;
        }
    }

    private void Start()
    {
        handGunReticle.alpha = 0;
        shotGunReticle.alpha = 0;
        autoGunReticle.alpha = 0;

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

        currentGun = gunDict[Array.IndexOf(guns, currentGunState)];
        currentGun.GunReticle.alpha = 1;
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (reloading)
        {
            gunRenderer.material.color = Color.red;
        }
        else
        {
            gunRenderer.material.color = default;
        }

        currentGunAmmo = currentGun.CurrentAmmo;

        ammoText.text = $"Ammo: {currentGunAmmo}/{currentGun.CurrentMaxAmmo}";
    }

    public void SwitchWeapon(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        currentGun.GunReticle.alpha = 0;

        if (currentGunState != guns.Last())
        {
            currentGunState = guns[Array.IndexOf(guns, currentGunState) + 1];
        }
        else
        {
            currentGunState = guns[0];
        }

        currentGun = gunDict[Array.IndexOf(guns, currentGunState)];
        currentGun.GunReticle.alpha = 1;

        WaitForSeconds reloadToInvoke = gunReloadWaitDict[currentGunState];
        weaponSwitched?.Invoke(reloadToInvoke);
    }

    public void Shoot(InputAction.CallbackContext context)
    {
        //if (currentGunAmmo > 0 && !reloading)
        //{
        //    currentGun.Shoot()
        //}
        if (!IsOwner) return;
        
        if (currentGunState == GunType.autoGun)
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
        }
    }

    [ServerRpc]
    private void RequestFireServerRpc()
    {
        FireClientRpc();
    }

    [ClientRpc]
    private void FireClientRpc()
    {
        if (!IsOwner) ExecuteShoot();
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
        if (!IsOwner) return;
        if (currentGunState == GunType.autoGun && autoGunCurrentAmmo < autoGunMaxAmmo && !reloading)
        {
            autoGun.StartReload(autoGunReloadWait);
        }

        if (currentGunState == GunType.handGun && handGunCurrentAmmo < handGunMaxAmmo && !reloading)
        {
            handGun.StartReload(handGunReloadWait);
        }

        if (currentGunState == GunType.shotGun && shotGunCurrentAmmo < shotGunMaxAmmo && !reloading)
        {
            shotGun.StartReload(shotGunReloadWait);
        }
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
