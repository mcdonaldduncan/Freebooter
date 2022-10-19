using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Build.Content;
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
    //public bool Reloading { get { return GunManager.Reloading; } set { GunManager.Reloading = value; } }

    private bool CanShoot => lastShotTime + FireRate < Time.time && !GunManager.Reloading;
    private bool ReloadNow => reloadStartTime + ReloadTime < Time.time && GunManager.Reloading;
    private float lastShotTime;
    private float reloadStartTime;

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
        if (CanShoot)
        {
            RaycastHit hitInfo;

            GameObject lineDrawer = new GameObject();
            LineRenderer lineRenderer = lineDrawer.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.025f;
            lineRenderer.endWidth = 0.025f;

            Vector3 aimSpot = GunManager.FPSCam.transform.position;
            aimSpot += GunManager.FPSCam.transform.forward * this.AimOffset;
            this.ShootFrom.LookAt(aimSpot);

            Vector3 direction = ShootFrom.transform.forward; // your initial aim.
            Vector3 spread = Vector3.zero;
            spread += ShootFrom.transform.up * Random.Range(-VerticalSpread, VerticalSpread);
            spread += ShootFrom.transform.right * Random.Range(-HorizontalSpread, HorizontalSpread);
            direction += spread.normalized; //* Random.Range(0f, 0.2f);

            if (Physics.Raycast(ShootFrom.transform.position, direction, out hitInfo, float.MaxValue, ~LayerToIgnore))
            {
                lineRenderer.material.color = Color.green;
                lineRenderer.SetPosition(0, ShootFrom.transform.position);
                lineRenderer.SetPosition(1, hitInfo.point);

                Debug.DrawLine(ShootFrom.transform.position, hitInfo.point, Color.green, 1f);
                try
                {
                    IDamageable damageableTarget = hitInfo.transform.GetComponent<IDamageable>();
                    Vector3 targetPosition = hitInfo.transform.position;

                    float distance = Vector3.Distance(targetPosition, ShootFrom.transform.position);
                    damageableTarget.TakeDamage(BulletDamage / (Mathf.Abs(distance / 2)));

                    Debug.Log($"{hitInfo.transform.name}: {damageableTarget.Health}");
                    var p = Instantiate(HitEnemy, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                    Destroy(p, 1);

                }
                catch
                {
                    //Debug.Log($"Hit {hitInfo.transform.name}");
                    Debug.Log("Not an IDamageable");
                    var p = Instantiate(HitNonEnemy, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                    Destroy(p, 1);
                }
            }
            else
            {
                Debug.DrawLine(ShootFrom.transform.position, ShootFrom.transform.forward + direction * 10, Color.red, 1f);
                lineRenderer.material.color = Color.red;
                lineRenderer.SetPosition(0, ShootFrom.transform.position);
                lineRenderer.SetPosition(1, ShootFrom.transform.position + direction * 10);
            }

            if (!GunManager.InfiniteAmmo)
            {
                GunManager.HandGunCurrentAmmo--;
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
    //    GunManager.HandGunCurrentAmmo = GunManager.HandGunMaxAmmo;
    //    GunManager.Reloading = false;
    //}

    public static void StartReload(GunHandler instance, HandGun handGun, WaitForSeconds reloadWait)
    {
        instance.StartCoroutine(handGun.Reload(instance, reloadWait));
    }

    public IEnumerator Reload(GunHandler instance, WaitForSeconds reloadWait)
    {
        instance.Reloading = true;
        yield return reloadWait;
        if (instance.Reloading)
        {
            instance.Reloading = false;
            instance.HandGunCurrentAmmo = instance.HandGunMaxAmmo;
        }
    }

    private void OnWeaponSwitch(GunHandler instance, IGun handGun, WaitForSeconds reloadWait)
    {
        Debug.Log("Stopping Reload");

        //if (GunManager.Reloading)
        //{
        //    GunManager.Reloading = false;
        //}
        StopCoroutine(handGun.Reload(instance, reloadWait));
    }
}
