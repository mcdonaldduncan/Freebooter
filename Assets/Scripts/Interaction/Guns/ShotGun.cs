using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShotGun : MonoBehaviour, IGun
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
    //public bool Reloading { get { return GunManager.Reloading; } set { GunManager.Reloading = value; } }
    private float ShotGunBulletAmount { get { return GunManager.ShotGunBulletAmount; } }
    public int CurrentAmmo { get { return GunManager.ShotGunCurrentAmmo; } set { GunManager.ShotGunCurrentAmmo = value; } }
    public int MaxAmmo { get { return GunManager.ShotGunMaxAmmo; } }
    public CanvasGroup GunReticle { get; set; }
    public GameObject Bullet { get; set; }
    public AudioClip GunShotAudio { get; set; }
    public GameObject GunModel { get; set; }
    public TrailRenderer BulletTrailRenderer { get; set; }
    public ShotgunAnimationHandler GunAnimationHandler { get; set; }
    public float HitStopDuration { get; set; }

    public bool CanShoot => lastShotTime + FireRate < Time.time && CurrentAmmo > 0;

    private float lastShotTime;
    private float reloadStartTime;
    private Coroutine reloadCo;
    private GameObject bulletFromPool;
    private List<GameObject> bulletPoolList;
    private int trailCounter;

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
        return;
    }

    public void Shoot()
    {
        bulletPoolList = new List<GameObject>();
        GunManager.GunShotAudioSource.PlayOneShot(GunShotAudio);
        GunAnimationHandler.RecoilAnim.SetTrigger("RecoilTrigger");
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

            if (Physics.Raycast(GunManager.FPSCam.transform.position, direction, out hitInfo, float.MaxValue, ~LayerToIgnore))
            {
                //Instantiate a bulletFromPool trail
                ProjectileManager.Instance.TakeFromPool(Bullet, ShootFrom.transform.position, out BulletTrail trail);
                trail.Launch(hitInfo.point);
                HitEnemyBehavior(hitInfo, hitInfo.transform.GetComponent<IDamageable>());
            }
            //if the player hit nothing
            else
            {
                //Spawn the bulletFromPool trail
                ProjectileManager.Instance.TakeFromPool(Bullet, ShootFrom.transform.position, out BulletTrail trail);
                trail.Launch(ShootFrom.transform.position + direction * 100);
            }
        }

        //if (!GunManager.InfiniteAmmo)
        //{
        //    CurrentAmmo--;
        //}

        lastShotTime = Time.time;
    }

    /// <summary>
    /// For when the player doesn't hit anything
    /// </summary>
    /// <param name="trail"></param>
    /// <param name="hitPoint"></param>
    /// <returns></returns>
    private IEnumerator SpawnTrail(GameObject bulletFromPoolTemp, TrailRenderer trail, Vector3 hitPoint)
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

        trailCounter++;

        if (trailCounter >= ShotGunBulletAmount)
        {
            ReturnBulletsToPool();
        }
    }

    /// <summary>
    /// When the player hits something
    /// </summary>
    /// <param name="trail"></param>
    /// <param name="hitInfo"></param>
    /// <param name="hitEffect"></param>
    /// <returns></returns>
    private IEnumerator SpawnTrail(GameObject bulletFromPoolTemp, TrailRenderer trail, RaycastHit hitInfo, GameObject hitEffect = null)
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

        trailCounter++;

        Debug.Log($"Trail Counter: {trailCounter}");

        if (trailCounter >= ShotGunBulletAmount)
        {
            ReturnBulletsToPool();
        }

        if (hitEffect != null)
        {
            try
            {
                var damageableTarget = hitInfo.transform.GetComponent<IDamageable>();
                HitEnemyBehavior(hitInfo, damageableTarget);
            }
            catch
            {
                HitEnemyBehavior(hitInfo);
            }
        }
    }

    private void ReturnBulletsToPool()
    {
        int bulletCount = 0;
        foreach (var bullet in bulletPoolList)
        {
            ProjectileManager.Instance.ReturnToPool(bullet);
            bulletCount++;
        }

        bulletPoolList.Clear();

        Debug.Log($"Bullets returned: {bulletCount}!");
    }

    private void HitEnemyBehavior(RaycastHit hitInfo, IDamageable damageableTarget = null)
    {

        if (damageableTarget != null)
        {
            bool breakableObject = hitInfo.transform.TryGetComponent<Fracture>(out Fracture component);

            //using a try catch to prevent destroyed enemies from throwing null reference exceptions
            try
            {
                //Get the position of the hit enemy
                Vector3 targetPosition = hitInfo.transform.position;
                ProjectileManager.Instance.TakeFromPool(breakableObject ? HitNonEnemy : HitEnemy, hitInfo.point);
                LevelManager.TimeStop(HitStopDuration);
                CameraShake.ShakeCamera();

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
                    float distancePercent = 100 - clampedDistance * (100 / (DropEnd - DropStart)); //Listen idk why this needs to be subtracted from 100 to work but it does so yeah, because its a percent out of 100!
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
                ProjectileManager.Instance.TakeFromPool(breakableObject ? HitNonEnemy : HitEnemy, hitInfo.point);
            }
        }
        else
        {
            ProjectileManager.Instance.TakeFromPool(HitNonEnemy, hitInfo.point);
        }
    }

    private void OnWeaponSwitch()
    {
        GunAnimationHandler.RecoilAnim.ResetTrigger("RecoilTrigger");
    }
}
