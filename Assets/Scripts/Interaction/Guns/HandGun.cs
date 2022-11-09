using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandGun : MonoBehaviour, IGun
{
    public GunHandler GunManager { get; set; }
    public Transform ShootFrom { get; set; }
    public LayerMask LayerToIgnore { get; set; }
    public float FireRate { get; set; }
    public float BulletDamage { get; set; }
    public float VerticalSpread { get; set; }
    public float HorizontalSpread { get; set; }
    public float AimOffset { get; set; }
    public GameObject HitEnemy { get; set; }
    public GameObject HitNonEnemy { get; set; }
    public float ReloadTime { get; set; }
    public int CurrentAmmo { get { return GunManager.HandGunCurrentAmmo; } set { GunManager.HandGunCurrentAmmo = value; } }
    public int CurrentMaxAmmo { get { return GunManager.HandGunMaxAmmo; } }
    public CanvasGroup GunReticle { get; set; }
    public TrailRenderer BulletTrail { get; set; }
    public AudioClip GunShotAudio { get; set; }
    //public bool Reloading { get { return GunManager.Reloading; } set { GunManager.Reloading = value; } }

    private bool CanShoot => lastShotTime + FireRate < Time.time && !GunManager.Reloading;
    private bool ReloadNow => reloadStartTime + ReloadTime < Time.time && GunManager.Reloading;
    private float lastShotTime;
    private float reloadStartTime;
    private Coroutine reloadCo;

    //private void Update()
    //{
    //    if (ReloadNow)
    //    {
    //        Reload();
    //    }
    //}

    private void OnEnable()
    {
        GunHandler.weaponSwitched += OnWeaponSwitch;
    }
    private void OnDisable()
    {
        GunHandler.weaponSwitched -= OnWeaponSwitch;
    }

    //Doesn't need to be static anymore since this script is added as a component now
    public void Shoot()
    {
        //if the player has ammo and they are not reloading
        if (CanShoot)
        {
            RaycastHit hitInfo;

            //Make sure the gun shoots towards the crosshair
            //Vector3 aimSpot = GunManager.FPSCam.transform.position;
            //aimSpot += GunManager.FPSCam.transform.forward * this.AimOffset;
            //this.ShootFrom.LookAt(aimSpot);

            //Add the customized spread of the specific gun
            Vector3 direction = GunManager.FPSCam.transform.forward; // your initial aim.
            Vector3 spread = Vector3.zero;
            spread += GunManager.FPSCam.transform.up;// * Random.Range(-VerticalSpread, VerticalSpread);
            spread += GunManager.FPSCam.transform.right;// * Random.Range(-HorizontalSpread, HorizontalSpread);
            direction += spread.normalized;

            //Play the shooting sound of this gun
            GunManager.GunShotAudioSource.PlayOneShot(GunShotAudio);

            Ray ray = GunManager.FPSCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            //Shoot out a raycast
            if (Physics.Raycast(ray, out hitInfo, float.MaxValue, ~LayerToIgnore))
            {

                //Instantiate a bullet trail
                TrailRenderer trail = Instantiate(BulletTrail, ShootFrom.transform.position, ShootFrom.transform.localRotation);
                trail.transform.parent = ShootFrom.transform;

                if (hitInfo.transform.name != "Player")
                {
                    StartCoroutine(SpawnTrail(trail, hitInfo, HitEnemy));
                }
            }
            //if the player hit nothing
            else
            {
                //Spawn the bullet trail
                TrailRenderer trail = Instantiate(BulletTrail, ShootFrom.transform.position, Quaternion.identity);
                StartCoroutine(SpawnTrail(trail, ShootFrom.transform.position + direction * 10));
            }

            //if the player does not have infinite ammo, decrement the gun's ammo by one
            if (!GunManager.InfiniteAmmo)
            {
                CurrentAmmo--;
            }

            //Get the time of the last shot, as this is needed for fire rate timer
            lastShotTime = Time.time;
        }
    }

    /// <summary>
    /// For when the player doesn't hit anything
    /// </summary>
    /// <param name="trail"></param>
    /// <param name="hitPoint"></param>
    /// <returns></returns>
    private IEnumerator SpawnTrail(TrailRenderer trail, Vector3 hitPoint)
    {
        float time = 0;

        Vector3 startPosition = ShootFrom.transform.position;

        while (time < 1)
        {
            trail.transform.position = Vector3.Lerp(startPosition, hitPoint, time);
            time += Time.deltaTime / trail.time;

            yield return null;
        }

        trail.transform.position = hitPoint;

        Destroy(trail.gameObject, trail.time);
    }

    /// <summary>
    /// When the player hits something
    /// </summary>
    /// <param name="trail"></param>
    /// <param name="hitInfo"></param>
    /// <param name="hitEffect"></param>
    /// <returns></returns>
    private IEnumerator SpawnTrail(TrailRenderer trail, RaycastHit hitInfo, GameObject hitEffect = null)
    {
        float time = 0;

        Vector3 startPosition = ShootFrom.transform.position;

        while (time < 1)
        {
            trail.transform.position = Vector3.Lerp(startPosition, hitInfo.point, time);
            time += Time.deltaTime / trail.time;

            yield return null;
        }

        trail.transform.position = hitInfo.point;

        Destroy(trail.gameObject, trail.time);

        if (hitEffect != null)
        {
            var damageableTarget = hitInfo.transform.GetComponent<IDamageable>();
            HitEnemyBehavior(hitInfo, damageableTarget);
        }
    }

    private void HitEnemyBehavior(RaycastHit hitInfo, IDamageable damageableTarget)
    {
        if (damageableTarget != null)
        {
            //using a try catch to prevent destroyed enemies from throwing null reference exceptions
            try
            {
                //Get the position of the hit enemy
                Vector3 targetPosition = hitInfo.transform.position;

                //Play blood particle effects on the enemy, where they were hit
                var p = Instantiate(HitEnemy, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                Destroy(p, 1);

                //Get the distance between the enemy and the gun
                float distance = Vector3.Distance(targetPosition, ShootFrom.transform.position);

                //calculate damage dropoff
                float totalDamage = Mathf.Abs(BulletDamage / ((distance / 2)));

                //Damage the target
                damageableTarget.TakeDamage(totalDamage);
            }
            catch
            {
                var p = Instantiate(HitEnemy, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                Destroy(p, 1);
            }
        }
        else
        {
            var p = Instantiate(HitNonEnemy, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
            Destroy(p, 1);
        }

    }
    //public void StartReload()
    //{
    //    GunManager.Reloading = true;
    //    reloadStartTime = Time.time;
    //}

    //private void Reload()
    //{
    //    GunManager.HandGunCurrentAmmo = GunManager.HandGunMaxAmmo;
    //    GunManager.Reloading = false;
    //}

    public void StartReload(WaitForSeconds reloadWait)
    {
        reloadCo = GunManager.StartCoroutine(this.Reload(reloadWait));
    }

    public IEnumerator Reload(WaitForSeconds reloadWait)
    {
        GunManager.Reloading = true;
        yield return reloadWait;
        if (GunManager.Reloading)
        {
            GunManager.Reloading = false;
            GunManager.HandGunCurrentAmmo = GunManager.HandGunMaxAmmo;
        }
    }

    private void OnWeaponSwitch(WaitForSeconds reloadWait)
    {
        Debug.Log("Stopping Reload");

        //if (GunManager.Reloading)
        //{
        //    GunManager.Reloading = false;
        //}
        if (reloadCo != null)
        {
            GunManager.StopCoroutine(reloadCo);
            GunManager.Reloading = false;
        }
    }
}
