using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ShotGun : MonoBehaviour, IGun
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
    //public bool Reloading { get { return GunManager.Reloading; } set { GunManager.Reloading = value; } }
    private float ShotGunBulletAmount { get { return GunManager.ShotGunBulletAmount; } }
    public int CurrentAmmo { get { return GunManager.ShotGunCurrentAmmo; } set { GunManager.ShotGunCurrentAmmo = value; } }
    public int CurrentMaxAmmo { get { return GunManager.ShotGunMaxAmmo; } }
    public CanvasGroup GunReticle { get; set; }
    public TrailRenderer BulletTrail { get; set; }

    private bool CanShoot => lastShotTime + FireRate < Time.time && !GunManager.Reloading;

    private float lastShotTime;
    private float reloadStartTime;
    private Coroutine reloadCo;

    //private void Update()
    //{
    //    if (GunManager.Reloading)
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
        if (CanShoot)
        {
            GunManager.GunShotAudioSource.PlayOneShot(GunManager.GunShotAudio);
            for (int i = 0; i < ShotGunBulletAmount; i++)
            {
                Vector3 aimSpot = GunManager.FPSCam.transform.position;
                aimSpot += GunManager.FPSCam.transform.forward * this.AimOffset;
                this.ShootFrom.LookAt(aimSpot);

                Vector3 direction = ShootFrom.transform.forward; // your initial aim.
                Vector3 spread = Vector3.zero;
                spread += ShootFrom.transform.up * Random.Range(-VerticalSpread, VerticalSpread);
                spread += ShootFrom.transform.right * Random.Range(-HorizontalSpread, HorizontalSpread);
                direction += spread.normalized * Random.Range(0f, 0.2f);

                RaycastHit hitInfo;

                if (Physics.Raycast(ShootFrom.transform.position, direction, out hitInfo, float.MaxValue, ~LayerToIgnore))
                {
                    TrailRenderer trail = Instantiate(BulletTrail, ShootFrom.transform.position, Quaternion.identity);
                    StartCoroutine(SpawnTrail(trail, hitInfo.point, aimSpot));

                    if (hitInfo.transform.name != "Player")
                    {
                        IDamageable damageableTarget = hitInfo.transform.GetComponent<IDamageable>();
                        if (damageableTarget != null)
                        {
                            try
                            {
                                Vector3 targetPosition = hitInfo.transform.position;

                                var p = Instantiate(HitEnemy, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                                Destroy(p, 1);
                                float distance = Vector3.Distance(targetPosition, ShootFrom.transform.position);
                                float totalDamage = Mathf.Abs(BulletDamage / ((distance / 2)));
                                damageableTarget.TakeDamage(totalDamage);

                                Debug.Log($"{hitInfo.transform.name}: {damageableTarget.Health}");
                                Debug.Log($"TakeDamage Dealt: {totalDamage}");
                            }
                            catch
                            {
                                var p = Instantiate(HitEnemy, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                                Destroy(p, 1);
                            }

                        }
                        else
                        {
                            Debug.Log("Not an IDamageable");
                            var p = Instantiate(HitNonEnemy, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                            Destroy(p, 1);
                        }
                    }
                }
                else
                {
                    Debug.Log("Didn't hit anything");
                    TrailRenderer trail = Instantiate(BulletTrail, ShootFrom.transform.position, Quaternion.identity);
                    StartCoroutine(SpawnTrail(trail, ShootFrom.transform.position + direction * 10, aimSpot));
                }
            }

            if (!GunManager.InfiniteAmmo)
            {
                CurrentAmmo--;
            }

            lastShotTime = Time.time;
        }
    }

    private IEnumerator SpawnTrail(TrailRenderer trail, Vector3 endPoint, Vector3 aimSpot)
    {
        float time = 0;

        trail.transform.LookAt(aimSpot);

        Vector3 startPosition = trail.transform.position;

        while (trail.transform.position != endPoint)
        {
            trail.transform.position = Vector3.Lerp(startPosition, endPoint, time);
            time += Time.deltaTime / trail.time;

            yield return null;
        }

        trail.transform.position = endPoint;

        Destroy(trail);
    }

    //public void StartReload()
    //{
    //    GunManager.Reloading = true;
    //    reloadStartTime = Time.time;
    //}

    //private void Reload()
    //{
    //    if (reloadStartTime + ReloadTime < Time.time)
    //    {
    //        GunManager.ShotGunCurrentAmmo = GunManager.ShotGunMaxAmmo;
    //        GunManager.Reloading = false;
    //    }
    //}

    public void StartReload(WaitForSeconds reloadWait)
    {
        reloadCo = GunManager.StartCoroutine(this.Reload(reloadWait));
    }

    public IEnumerator Reload(WaitForSeconds reloadWait)
    {
        GunManager.Reloading = true;
        yield return reloadWait;
        GunManager.Reloading = false;
        GunManager.ShotGunCurrentAmmo = GunManager.ShotGunMaxAmmo;
    }

    private void OnWeaponSwitch(WaitForSeconds reloadWait)
    {
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
