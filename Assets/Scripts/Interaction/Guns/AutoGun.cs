using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AutoGun : MonoBehaviour, IGun, IDamageTracking
{
    public string GunName { get { return "Rifle"; } }
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
    public int CurrentAmmo { get { return GunManager.AutoGunCurrentAmmo; } set { GunManager.AutoGunCurrentAmmo = value; } }
    public int MaxAmmo { get { return GunManager.AutoGunMaxAmmo; } }
    public CanvasGroup GunReticle { get; set; }
    public GameObject Bullet { get; set; }
    public AudioClip GunShotAudio { get; set; }
    //public AudioClip TriggerReleasedAudio { get; set; }
    public AudioClip[] GunShotAudioList { get; set; }
    public TrailRenderer BulletTrailRenderer { get; set; }
    public GameObject GunModel { get; set; }
    public GunAnimationHandler GunAnimationHandler { get; set; }
    public float ShakeDuration { get; set; }
    public float ShakeMagnitude { get; set; }
    public float ShakeDampen { get; set; }
    //public bool Reloading { get { return GunManager.Reloading; } set { GunManager.Reloading = value; } }

    public bool CanShoot => lastShotTime + FireRate < Time.time && CurrentAmmo > 0 && GunManager.CurrentGun is AutoGun;
    public bool FireRateCooldown => lastShotTime + FireRate > Time.time;

    public PlayerDamageDelegate DamageDealt { get; set; }

    private bool holdingTrigger;
    private float lastShotTime;
    private float reloadStartTime;
    private Coroutine reloadCo;
    private GameObject bulletFromPool;

    private void Update()
    {
        if (CanShoot && this.holdingTrigger)
        {
            //Debug.Log($"Holding Trigger: {holdingTrigger}");
            Shoot();
        }
        //if (GunManager.Reloading)
        //{
        //    Reload();
        //}

    }

    private void Start()
    {
        LevelManager.Instance.RegisterDamageTracker(this);
    }

    private void OnEnable()
    {
        GunHandler.weaponSwitched += OnWeaponSwitch;
    }
    private void OnDisable()
    {
        GunHandler.weaponSwitched -= OnWeaponSwitch;
    }

    //Doesn't need to be static anymore since this script is added as a component now
    public void ShootTriggered(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            //if (this.holdingTrigger && GunManager.AutoGunCurrentAmmo > 0)
            //{
            //    GunManager.GunShotAudioSource.PlayOneShot(TriggerReleasedAudio);
            //}
            this.holdingTrigger = false;
            GunAnimationHandler.RecoilAnim.ResetTrigger("RecoilTrigger");
        }
        else if (context.performed)
        {
            this.holdingTrigger = true;
        }
    }
    public void AlternateTriggered(InputAction.CallbackContext context)
    {
        return;
    }

    private void Shoot()
    {
        if (GunManager.AutoGunCurrentAmmo > 0)
        {
            if (!GunManager.InfiniteAmmo)
            {
                CurrentAmmo--;
            }
            GunManager.UpdateAmmoDisplay();

            GunAnimationHandler.RecoilAnim.SetTrigger("RecoilTrigger");

            //Add the customized spread of the specific gun
            Vector3 spread = Vector3.zero;
            spread += GunManager.FPSCam.transform.up * Random.Range(-VerticalSpread, VerticalSpread);
            spread += GunManager.FPSCam.transform.right * Random.Range(-HorizontalSpread, HorizontalSpread);

            Ray ray = GunManager.FPSCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0) + spread.normalized);

            RaycastHit hitInfo;

            int gunShotIndex = Random.Range(0, GunShotAudioList.Length - 1);
            GunShotAudio = GunShotAudioList[gunShotIndex];
            GunManager.GunShotAudioSource.PlayOneShot(GunShotAudio);
            CameraShake.ShakeCamera(ShakeDuration, ShakeMagnitude, ShakeDampen);

            if (Physics.Raycast(ray, out hitInfo, float.MaxValue, ~LayerToIgnore))
            {
                ProjectileManager.Instance.TakeFromPool(Bullet, ShootFrom.transform.position, out BulletTrail trail);

                Quaternion muzzleLook = Quaternion.LookRotation(-GunManager.FPSCam.transform.forward);
                var muzzleFlash = ProjectileManager.Instance.TakeFromPool(GunManager.MuzzleFlash, ShootFrom.transform.position, muzzleLook);
                muzzleFlash.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                muzzleFlash.transform.SetParent(ShootFrom);

                trail.Launch(hitInfo.point);
                HitEnemyBehavior(hitInfo, hitInfo.transform.GetComponent<IDamageable>());
                CameraShake.ShakeCamera(ShakeDuration, ShakeMagnitude, ShakeDampen);


                //Instantiate a bulletFromPool trail
                //TrailRenderer trail = Instantiate(Bullet, ShootFrom.transform.position, ShootFrom.transform.localRotation);
                //bulletFromPool = ProjectileManager.Instance.TakeFromPool(Bullet, ShootFrom.transform.position);
                //BulletTrailRenderer = bulletFromPool.GetComponent<TrailRenderer>();

                //if (hitInfo.transform.name != "Player")
                //{
                //    StartCoroutine(SpawnTrail(BulletTrailRenderer, hitInfo, HitEnemy));
                //}
            }
            //if the player hit nothing
            else
            {
                ProjectileManager.Instance.TakeFromPool(Bullet, ShootFrom.transform.position, out BulletTrail trail);
                var muzzleFlash = ProjectileManager.Instance.TakeFromPool(GunManager.MuzzleFlash, ShootFrom.transform.position, Quaternion.LookRotation(-GunManager.FPSCam.transform.forward));
                muzzleFlash.transform.SetParent(ShootFrom);
                trail.Launch(ShootFrom.transform.position + ray.direction * 100);

                //Spawn the bulletFromPool trail
                //bulletFromPool = ProjectileManager.Instance.TakeFromPool(Bullet, ShootFrom.transform.position);
                //BulletTrailRenderer = bulletFromPool.GetComponent<TrailRenderer>();
                //StartCoroutine(SpawnTrail(BulletTrailRenderer, ShootFrom.transform.position + ray.direction * 10));
            }

            lastShotTime = Time.time;
        }

        if (GunManager.AutoGunCurrentAmmo <= 0)
        {
            this.holdingTrigger = false;
            //GunManager.GunShotAudioSource.PlayOneShot(TriggerReleasedAudio);
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

        ProjectileManager.Instance.ReturnToPool(bulletFromPool);
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

        ProjectileManager.Instance.ReturnToPool(bulletFromPool);

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
            bool bloodlessObj = hitInfo.transform.TryGetComponent<IBloodless>(out IBloodless component);

            // Could we just use an if statement here? try catch is very inefficient
            //using a try catch to prevent destroyed enemies from throwing null reference exceptions
            try
            {
                //Get the position of the hit enemy
                Vector3 targetPosition = hitInfo.transform.position;

                //Play blood particle effects on the enemy, where they were hit
                //var p = Instantiate(breakableObject ? HitNonEnemy : HitEnemy, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                //Destroy(p, 1);

                ProjectileManager.Instance.TakeFromPool(bloodlessObj ? HitNonEnemy : HitEnemy, hitInfo.point);
                //CameraShake.ShakeCamera();

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
                damageableTarget.TakeDamage(realDamage, HitBoxType.normal, hitInfo.point);
                DamageDealt?.Invoke(realDamage);
            }
            catch
            {
                //var p = Instantiate(breakableObject ? HitNonEnemy : HitEnemy, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                //Destroy(p, 1);

                ProjectileManager.Instance.TakeFromPool(bloodlessObj ? HitNonEnemy : HitEnemy, hitInfo.point);
            }
        }
        else if (hitInfo.collider.gameObject.TryGetComponent(out Projectile projectile))
        {
            projectile.ProjectileHit();
        }
        else
        {
            //var p = Instantiate(HitNonEnemy, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
            //Destroy(p, 1);

            ProjectileManager.Instance.TakeFromPool(HitNonEnemy, hitInfo.point);
        }
    }

    private void OnWeaponSwitch()
    {
        this.holdingTrigger = false;
        GunAnimationHandler.RecoilAnim.ResetTrigger("RecoilTrigger");
    }
}
