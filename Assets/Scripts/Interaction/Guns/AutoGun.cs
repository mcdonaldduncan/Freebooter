using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AutoGun : MonoBehaviour, IGun
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
    public int CurrentAmmo { get { return GunManager.AutoGunCurrentAmmo; } set { GunManager.AutoGunCurrentAmmo = value; } }
    public int CurrentMaxAmmo { get { return GunManager.AutoGunMaxAmmo; } }
    public CanvasGroup GunReticle { get; set; }
    public TrailRenderer BulletTrail { get; set; }
    public AudioClip GunShotAudio { get; set; }
    public AudioClip TriggerReleasedAudio { get; set; }
    public AudioClip[] GunShotAudioList { get; set; }
    public GameObject GunModel { get; set; }
    //public bool Reloading { get { return GunManager.Reloading; } set { GunManager.Reloading = value; } }

    public bool CanShoot => lastShotTime + FireRate < Time.time && CurrentAmmo > 0 && GunManager.CurrentGun is AutoGun;

    private bool holdingTrigger;
    private float lastShotTime;
    private float reloadStartTime;
    private Coroutine reloadCo;

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
            if (this.holdingTrigger && GunManager.AutoGunCurrentAmmo > 0 && !GunManager.Reloading)
            {
                GunManager.GunShotAudioSource.PlayOneShot(TriggerReleasedAudio);
            }
            this.holdingTrigger = false;
        }
        else if (context.performed)
        {
            this.holdingTrigger = true;
        }
    }

    private void Shoot()
    {
        if (!GunManager.Reloading && GunManager.AutoGunCurrentAmmo > 0)
        {
            if (!GunManager.InfiniteAmmo)
            {
                CurrentAmmo--;
            }

            //Add the customized spread of the specific gun
            Vector3 spread = Vector3.zero;
            spread += GunManager.FPSCam.transform.up * Random.Range(-VerticalSpread, VerticalSpread);
            spread += GunManager.FPSCam.transform.right * Random.Range(-HorizontalSpread, HorizontalSpread);

            Ray ray = GunManager.FPSCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0) + spread.normalized);

            RaycastHit hitInfo;

            int gunShotIndex = Random.RandomRange(0, GunShotAudioList.Length - 1);
            GunShotAudio = GunShotAudioList[gunShotIndex];
            GunManager.GunShotAudioSource.PlayOneShot(GunShotAudio);

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

            lastShotTime = Time.time;
        }

        if (GunManager.AutoGunCurrentAmmo <= 0)
        {
            this.holdingTrigger = false;
            GunManager.GunShotAudioSource.PlayOneShot(TriggerReleasedAudio);
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
                    float distancePercent = DropEnd - clampedDistance * (100 / (DropEnd - DropStart));
                    realDamage = MinDamage + (MaxDamage - MinDamage) * (distancePercent / 100);
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

    //public void StartReload()
    //{
    //    GunManager.Reloading = true;
    //    reloadStartTime = Time.time;
    //}

    //private void Reload()
    //{
    //    if (reloadStartTime + ReloadTime < Time.time)
    //    {
    //        GunManager.AutoGunCurrentAmmo = GunManager.AutoGunMaxAmmo;
    //        GunManager.Reloading = false;
    //    }
    //}


    //fix bug that doesn't restart canceled reload    
    public void StartReload()
    {
        reloadCo = GunManager.StartCoroutine(this.Reload(ReloadWait));
    }

    private void OnWeaponSwitch()
    {
        this.holdingTrigger = false;
        if (reloadCo != null)
        {
            GunManager.StopCoroutine(reloadCo);
            GunManager.Reloading = false;
        }
        
        //if (GunManager.Reloading)
        //{
        //    GunManager.Reloading = false;
        //}
    }

    public IEnumerator Reload(WaitForSeconds reloadWait)
    {
        GunManager.Reloading = true;
        yield return reloadWait;
        if (GunManager.Reloading)
        {
            GunManager.Reloading = false;
            GunManager.AutoGunCurrentAmmo = GunManager.AutoGunMaxAmmo;
        }
    }
}
