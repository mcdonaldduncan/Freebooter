using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrenadeGun : MonoBehaviour, IGun
{
    public GunHandler GunManager { get; set; }
    public Transform ShootFrom { get; set; }
    public LayerMask LayerToIgnore { get; set; }
    public float FireRate { get; set; }
    public float GrenadeDamage { get; set; }
    public float DamageDrop { get; set; }
    public float VerticalSpread { get; set; }
    public float HorizontalSpread { get; set; }
    public float AimOffset { get; set; }
    public GameObject HitEnemy { get; set; }
    public GameObject HitNonEnemy { get; set; }
    public WaitForSeconds ReloadWait { get; set; }
    //public bool Reloading { get { return GunManager.Reloading; } set { GunManager.Reloading = value; } }
    public int CurrentAmmo { get { return GunManager.GrenadeGunCurrentAmmo; } set { GunManager.GrenadeGunCurrentAmmo = value; } }
    public int MaxAmmo { get { return GunManager.GrenadeGunMaxAmmo; } }
    public CanvasGroup GunReticle { get; set; }
    public GameObject Bullet { get; set; }
    public TrailRenderer BulletTrailRenderer { get; set; }
    public AudioClip GunShotAudio { get; set; }
    public GameObject GunModel { get; set; }
    public GameObject Grenade { get; set; }
    public float GrenadeLaunchForce { get; set; }
    public Vector3 GrenadeLaunchArcVector { get; set; }

    public bool CanShoot => lastShotTime + FireRate < Time.time && CurrentAmmo > 0;

    //THIS CAN BE IGNORED IT IS NEEDED SO THAT GRENADE GUN CAN STILL USE IGUN INTERFACE
    public float MaxDamage { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public float MinDamage { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public float DropStart { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public float DropEnd { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    private float lastShotTime;
    private float reloadStartTime;
    private Coroutine reloadCo;

    public delegate void GrenadeGunDelegate();
    public event GrenadeGunDelegate remoteDetonationEvent;

    private void OnEnable()
    {
        GunHandler.weaponSwitched += OnWeaponSwitch;
    }
    private void OnDisable()
    {
        GunHandler.weaponSwitched -= OnWeaponSwitch;
    }

    public void ShootTriggered(InputAction.CallbackContext context)
    {
        if (CanShoot && context.performed) Shoot();
    }

    public void AlternateTriggered(InputAction.CallbackContext context)
    {
        if (context.performed) DetonateGrenades();
    }

    public void Shoot()
    {
        if (!GunManager.InfiniteAmmo)
        {
            CurrentAmmo--;
        }

        GunManager.GunShotAudioSource.PlayOneShot(GunShotAudio);
        Ray ray = GunManager.FPSCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        Vector3 grenadeLaunchForce = (ray.direction + GrenadeLaunchArcVector) * GrenadeLaunchForce;

        GameObject newGrenade = ProjectileManager.Instance.TakeFromPool(Grenade, ShootFrom.position, out GrenadeBehavior grenade); //Instantiate(Grenade, ShootFrom.position, Quaternion.identity);
        //newGrenade.transform.parent = transform;
        grenade.Launch(grenadeLaunchForce);
        //Rigidbody gRB = newGrenade.GetComponent<Rigidbody>();
        //gRB.AddForce(grenadeLaunchForce);

        lastShotTime = Time.time;
    }

    public void DetonateGrenades()
    {
        remoteDetonationEvent?.Invoke();
    }

    //public void StartReload()
    //{
    //    reloadCo = GunManager.StartCoroutine(this.Reload(ReloadWait));
    //}

    //public IEnumerator Reload(WaitForSeconds reloadWait)
    //{
    //    GunManager.Reloading = true;
    //    yield return reloadWait;
    //    GunManager.Reloading = false;
    //    GunManager.GrenadeGunCurrentAmmo = GunManager.GrenadeGunMaxAmmo;
    //}

    private void OnWeaponSwitch()
    {
        //if (GunManager.Reloading)
        //{
        //    GunManager.Reloading = false;
        //}

        //if (reloadCo != null)
        //{
        //    GunManager.StopCoroutine(reloadCo);
        //    GunManager.Reloading = false;
        //}
    }
}
