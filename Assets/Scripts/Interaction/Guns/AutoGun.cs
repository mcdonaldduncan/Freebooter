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
    public float BulletDamage { get; set; }
    public float DamageDrop { get; set; }
    public float VerticalSpread { get; set; }
    public float HorizontalSpread { get; set; }
    public float AimOffset { get; set; }
    public GameObject HitEnemy { get; set; }
    public GameObject HitNonEnemy { get; set; }
    public float ReloadTime { get; set; }
    public int CurrentAmmo { get { return GunManager.AutoGunCurrentAmmo; } set { GunManager.AutoGunCurrentAmmo = value; } }
    public int CurrentMaxAmmo { get { return GunManager.AutoGunMaxAmmo; } }
    public CanvasGroup GunReticle { get; set; }
    public TrailRenderer BulletTrail { get; set; }
    public AudioClip GunShotAudio { get; set; }
    //public bool Reloading { get { return GunManager.Reloading; } set { GunManager.Reloading = value; } }

    private bool CanShoot => lastShotTime + FireRate < Time.time && CurrentAmmo > 0 && GunManager.CurrentGun is AutoGun;

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
            this.holdingTrigger = false;
            //GunManager.StopCoroutine(this.ShootAutoGun());
        }
        else if (context.performed)
        {
            this.holdingTrigger = true;

            //GunManager.StartCoroutine(this.ShootAutoGun());
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

            Vector3 aimSpot = GunManager.FPSCam.transform.position;
            aimSpot += GunManager.FPSCam.transform.forward * AimOffset;
            ShootFrom.LookAt(aimSpot);

            Vector3 direction = ShootFrom.transform.forward; // your initial aim.
            Vector3 spread = Vector3.zero;
            spread += ShootFrom.transform.up * Random.Range(-VerticalSpread, VerticalSpread);
            spread += ShootFrom.transform.right * Random.Range(-HorizontalSpread, HorizontalSpread);
            direction += spread.normalized; //* Random.Range(0f, 0.2f);

            RaycastHit hitInfo;


            GunManager.GunShotAudioSource.PlayOneShot(GunShotAudio);

            if (Physics.Raycast(GunManager.FPSCam.transform.position, direction, out hitInfo, float.MaxValue, ~LayerToIgnore))
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
                StartCoroutine(SpawnTrail(trail, ShootFrom.transform.position + direction * 10));
            }

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
                float totalDamage = Mathf.Abs(BulletDamage / ((distance / DamageDrop)));

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
    //    if (reloadStartTime + ReloadTime < Time.time)
    //    {
    //        GunManager.AutoGunCurrentAmmo = GunManager.AutoGunMaxAmmo;
    //        GunManager.Reloading = false;
    //    }
    //}


    //fix bug that doesn't restart canceled reload    
    public void StartReload(WaitForSeconds reloadWait)
    {
        reloadCo = GunManager.StartCoroutine(this.Reload(reloadWait));
    }

    private void OnWeaponSwitch(WaitForSeconds reloadWait)
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
            Debug.Log("Reloaded!");
        }
        else
        {
            Debug.Log($"Reloading was canceled");
        }
    }

    //private IEnumerator ShootAutoGun()
    //{
    //    if (!GunManager.Reloading && GunManager.AutoGunCurrentAmmo > 0)
    //    {
    //        if (!GunManager.InfiniteAmmo)
    //        {
    //            GunManager.AutoGunCurrentAmmo--;
    //        }
    //        GameObject lineDrawer = new GameObject();
    //        LineRenderer lineRenderer = lineDrawer.AddComponent<LineRenderer>();
    //        lineRenderer.startWidth = 0.025f;
    //        lineRenderer.endWidth = 0.025f;

    //        Vector3 aimSpot = GunManager.FPSCam.transform.position;
    //        aimSpot += GunManager.FPSCam.transform.forward * AimOffset;
    //        ShootFrom.LookAt(aimSpot);

    //        Vector3 direction = ShootFrom.transform.forward; // your initial aim.
    //        Vector3 spread = Vector3.zero;
    //        spread += ShootFrom.transform.up * Random.Range(-VerticalSpread, VerticalSpread);
    //        spread += ShootFrom.transform.right * Random.Range(-HorizontalSpread, HorizontalSpread);
    //        direction += spread.normalized; //* Random.Range(0f, 0.2f);

    //        RaycastHit hitInfo;

    //        if (Physics.Raycast(ShootFrom.transform.position, direction, out hitInfo, float.MaxValue, ~LayerToIgnore))
    //        {

    //            Debug.DrawLine(ShootFrom.transform.position, hitInfo.point, Color.green, 1f);
    //            lineRenderer.material.color = Color.green;
    //            lineRenderer.SetPosition(0, ShootFrom.transform.position);
    //            lineRenderer.SetPosition(1, hitInfo.point);

    //            if (hitInfo.transform.name != "Player")
    //            {
    //                try
    //                {
    //                    IDamageable damageableTarget = hitInfo.transform.GetComponent<IDamageable>();
    //                    Vector3 targetPosition = hitInfo.transform.position;

    //                    float distance = Vector3.Distance(targetPosition, ShootFrom.transform.position);
    //                    float totalDamage = Mathf.Abs(BulletDamage / ((distance / 2)));
    //                    damageableTarget.TakeDamage(totalDamage);

    //                    Debug.Log($"{hitInfo.transform.name}: {damageableTarget.Health}");
    //                    Debug.Log($"TakeDamage Dealt: {totalDamage}");
    //                    var p = Instantiate(HitEnemy, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
    //                    Destroy(p, 1);
    //                }
    //                catch
    //                {
    //                    Debug.Log("Not an IDamageable");
    //                    var p = Instantiate(HitNonEnemy, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
    //                    Destroy(p, 1);
    //                }
    //            }
    //        }
    //        else
    //        {
    //            lineRenderer.material.color = Color.red;
    //            lineRenderer.SetPosition(0, ShootFrom.transform.position);
    //            lineRenderer.SetPosition(1, ShootFrom.transform.position + direction * 10);
    //            Debug.DrawLine(ShootFrom.transform.position, ShootFrom.transform.position + direction * 10, Color.red, 1f);
    //        }

    //        yield return FireRate;

    //        if (holdingTrigger && !GunManager.Reloading)
    //        {
    //            GunManager.StartCoroutine(this.ShootAutoGun());
    //        }
    //    }
    //}


}
