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
                TrailRenderer trail = Instantiate(BulletTrail, ShootFrom.transform.position, gameObject.transform.rotation);
                trail.transform.parent = ShootFrom.transform;
                Debug.Log($"Shoot From Local Position: {ShootFrom.transform.localPosition}");
                Debug.Log($"Trail Local Position: {trail.transform.localPosition}");
                StartCoroutine(SpawnTrail(trail, hitInfo.point));


                if (hitInfo.transform.name != "Player")
                {
                    //IDamageable is the interface used for anything that can take damage (Enemies, Player, Buttons, etc)
                    //Get the IDamageable component of the hit object
                    var damageableTarget = hitInfo.transform.GetComponent<IDamageable>();
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
                            //Play the blood effect
                            var p = Instantiate(HitEnemy, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                            Destroy(p, 1);
                        }

                    }
                    //If the player did not hit an enemy
                    else
                    {
                        //Play the bullet spark/impact particle effect
                        var p = Instantiate(HitNonEnemy, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                        Destroy(p, 1);
                    }
                }
            }
            //if the player hit nothing
            else
            {
                //Spawn the bullet trail
                TrailRenderer trail = Instantiate(BulletTrail, ShootFrom.transform.localPosition, Quaternion.identity);
                Debug.Log($"Shoot From Local Position: {ShootFrom.transform.localPosition}");
                Debug.Log($"Trail Local Position: {trail.transform.localPosition}");
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

    private IEnumerator SpawnTrail(TrailRenderer trail, Vector3 hitPoint)
    {
        //Vector3 startPosition = trail.transform.position;
        ////Vector3 direction = (hitPoint - trail.transform.position).normalized;
        ////trail.transform.LookAt(hitPoint);

        //float distance = Vector3.Distance(trail.transform.position, hitPoint);
        //float startingDistance = distance;

        //while (distance > 0)
        //{
        //    trail.transform.position = Vector3.Lerp(startPosition, hitPoint, 1 - (distance / startingDistance));
        //    distance -= Time.deltaTime * 10;

        //    yield return null;
        //}

        //trail.transform.position = hitPoint;

        //------------------------------------------

        float time = 0;

        //trail.transform.LookAt(hitPoint);

        Vector3 startPosition = trail.transform.position;

        while (time < 1)
        {
            trail.transform.position = Vector3.Lerp(startPosition, hitPoint, time);
            time += Time.deltaTime / trail.time;

            yield return null;
        }

        trail.transform.position = hitPoint;

        Destroy(trail.gameObject, trail.time);
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
