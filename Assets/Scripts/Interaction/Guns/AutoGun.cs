using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class AutoGun : MonoBehaviour, IGun
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

    private bool CanShoot => lastShotTime + FireRate < Time.time && holdingTrigger;

    private static bool holdingTrigger;
    private float lastShotTime;
    private float reloadStartTime;

    private void Update()
    {
        if (CanShoot)
        {
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
            holdingTrigger = false;
            //GunManager.StopCoroutine(this.ShootAutoGun());
        }
        else if (context.performed && !GunManager.Reloading)
        {
            holdingTrigger = true;

            //GunManager.StartCoroutine(this.ShootAutoGun());
        }
    }

    private void Shoot()
    {
        if (!GunManager.Reloading && GunManager.AutoGunCurrentAmmo > 0)
        {
            if (!GunManager.InfiniteAmmo)
            {
                GunManager.AutoGunCurrentAmmo--;
            }
            GameObject lineDrawer = new GameObject();
            LineRenderer lineRenderer = lineDrawer.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.025f;
            lineRenderer.endWidth = 0.025f;

            Vector3 aimSpot = GunManager.FPSCam.transform.position;
            aimSpot += GunManager.FPSCam.transform.forward * AimOffset;
            ShootFrom.LookAt(aimSpot);

            Vector3 direction = ShootFrom.transform.forward; // your initial aim.
            Vector3 spread = Vector3.zero;
            spread += ShootFrom.transform.up * Random.Range(-VerticalSpread, VerticalSpread);
            spread += ShootFrom.transform.right * Random.Range(-HorizontalSpread, HorizontalSpread);
            direction += spread.normalized; //* Random.Range(0f, 0.2f);

            RaycastHit hitInfo;

            if (Physics.Raycast(ShootFrom.transform.position, direction, out hitInfo, float.MaxValue, ~LayerToIgnore))
            {

                Debug.DrawLine(ShootFrom.transform.position, hitInfo.point, Color.green, 1f);
                lineRenderer.material.color = Color.green;
                lineRenderer.SetPosition(0, ShootFrom.transform.position);
                lineRenderer.SetPosition(1, hitInfo.point);

                if (hitInfo.transform.name != "Player")
                {
                    try
                    {
                        IDamageable damageableTarget = hitInfo.transform.GetComponent<IDamageable>();
                        Vector3 targetPosition = hitInfo.transform.position;

                        float distance = Vector3.Distance(targetPosition, ShootFrom.transform.position);
                        float totalDamage = Mathf.Abs(BulletDamage / ((distance / 2)));
                        damageableTarget.TakeDamage(totalDamage);

                        Debug.Log($"{hitInfo.transform.name}: {damageableTarget.Health}");
                        Debug.Log($"TakeDamage Dealt: {totalDamage}");
                        var p = Instantiate(HitEnemy, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                        Destroy(p, 1);
                    }
                    catch
                    {
                        Debug.Log("Not an IDamageable");
                        var p = Instantiate(HitNonEnemy, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                        Destroy(p, 1);
                    }
                }
            }
            else
            {
                lineRenderer.material.color = Color.red;
                lineRenderer.SetPosition(0, ShootFrom.transform.position);
                lineRenderer.SetPosition(1, ShootFrom.transform.position + direction * 10);
                Debug.DrawLine(ShootFrom.transform.position, ShootFrom.transform.position + direction * 10, Color.red, 1f);
            }

            lastShotTime = Time.time;
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

    private void OnWeaponSwitch(GunHandler instance, IGun autoGun, WaitForSeconds reloadWait)
    {
        StopCoroutine(autoGun.Reload(instance, reloadWait));
        holdingTrigger = false;
        
        //if (GunManager.Reloading)
        //{
        //    GunManager.Reloading = false;
        //}
    }
    
    public static void StartReload(GunHandler instance, AutoGun autoGun, WaitForSeconds reloadWait)
    {
        instance.StartCoroutine(autoGun.Reload(instance, reloadWait));
    }

    public IEnumerator Reload(GunHandler instance, WaitForSeconds reloadWait)
    {
        instance.Reloading = true;
        yield return reloadWait;
        instance.Reloading = false;
        instance.AutoGunCurrentAmmo = instance.AutoGunMaxAmmo;
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
