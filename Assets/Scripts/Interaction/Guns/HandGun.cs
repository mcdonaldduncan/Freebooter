using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandGun : MonoBehaviour, IGun
{
    public GunHandler GunManager { get; set; }
    public Transform ShootFrom { get; set; }
    public LayerMask LayerToIgnore { get; set; }
    public float FireRate { get; set; }
    public float MaxDamage { get; set; }
    public float MinDamage { get; set; }
    public float DropStart { get; set; }
    public float DropEnd { get; set; }
    public float VerticalSpread { get; set; }
    public float HorizontalSpread { get; set; }
    public float AimOffset { get; set; }
    public GameObject HitEnemy { get; set; }
    public GameObject HitNonEnemy { get; set; }
    public WaitForSeconds ReloadWait { get; set; }
    public int CurrentAmmo { get { return GunManager.HandGunCurrentAmmo; } set { GunManager.HandGunCurrentAmmo = value; } }
    public int CurrentMaxAmmo { get { return GunManager.HandGunMaxAmmo; } }
    public CanvasGroup GunReticle { get; set; }
    public TrailRenderer BulletTrail { get; set; }
    public AudioClip GunShotAudio { get; set; }
    public GameObject GunModel { get; set; }
    //public bool Reloading { get { return GunManager.Reloading; } set { GunManager.Reloading = value; } }

    public bool CanShoot => lastShotTime + FireRate < Time.time && !GunManager.Reloading && CurrentAmmo > 0;

    //private bool ReloadNow => reloadStartTime + ReloadTime < Time.time && GunManager.Reloading;
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

    public void ShootTriggered(InputAction.CallbackContext context)
    {
        if (CanShoot && context.performed) Shoot();
    }

    public void Shoot()
    {
        var timeShot = Time.time;
        RaycastHit hitInfo;

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
            TrailRenderer trail = Instantiate(BulletTrail, ShootFrom.transform.position, ShootFrom.transform.localRotation);
            StartCoroutine(SpawnTrail(trail, ShootFrom.transform.position + ray.direction * 10));
        }

        //if the player does not have infinite ammo, decrement the gun's ammo by one
        if (!GunManager.InfiniteAmmo)
        {
            CurrentAmmo--;
        }

        //Get the time of the last shot, as this is needed for fire rate timer
        lastShotTime = Time.time;
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
            //if (damageableTarget != null)
            //{
            //    HitEnemyBehavior(hitInfo, damageableTarget);
            //}
            //else
            //{
            //    HitEnemyBehavior(hitInfo);
            //}
            
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
                float realDamage;

                if (distance >= DropEnd)
                {
                    realDamage = MinDamage;
                }
                else if (distance <= DropStart)
                {
                    realDamage = MaxDamage;
                }
                else
                {
                    float clampedDistance = Mathf.Clamp(distance, DropStart, DropEnd) - DropStart;
                    float distancePercent = 100 - clampedDistance * (100 / (DropEnd - DropStart)); //Listen idk why this needs to be subtracted from 100 to work but it does so yeah
                    realDamage = Mathf.Abs(MinDamage + (MaxDamage - MinDamage) * (distancePercent / 100));
                    if (realDamage <= MinDamage)
                    {
                        realDamage = MinDamage;
                    }
                    if (realDamage >= MaxDamage)
                    {
                        realDamage = MaxDamage;
                    }
                }

                //Damage the target
                damageableTarget.TakeDamage(realDamage);
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

    public void StartReload()
    {
        reloadCo = GunManager.StartCoroutine(this.Reload(ReloadWait));
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

    private void OnWeaponSwitch()
    {
        if (reloadCo != null)
        {
            GunManager.StopCoroutine(reloadCo);
            GunManager.Reloading = false;
        }
    }
}
