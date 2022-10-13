using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class AutoGun : MonoBehaviour, IGun
{
    private static bool holdingTrigger;

    private void OnEnable()
    {
        GunHandler.weaponSwitched += OnWeaponSwitch;
    }
    private void OnDisable()
    {
        GunHandler.weaponSwitched -= OnWeaponSwitch;
    }

    public static void Shoot(GunHandler instance, AutoGun autoGun, Transform shootFrom, GameObject gameObject, LayerMask layerToIgnore, InputAction.CallbackContext context, WaitForSeconds fireRateWait, float bulletDamage, float verticalSpread, float horizontalSpread, float aimOffset, GameObject hitEnemy, GameObject HitNONEnemy)
    {
        if (context.canceled)
        {
            holdingTrigger = false;
            instance.StopCoroutine(autoGun.ShootAutoGun(instance, autoGun, shootFrom, gameObject, layerToIgnore, fireRateWait, bulletDamage, verticalSpread, horizontalSpread, aimOffset,hitEnemy,HitNONEnemy));
        }
        else if (context.performed && !instance.Reloading)
        {
            holdingTrigger = true;

            instance.StartCoroutine(autoGun.ShootAutoGun(instance, autoGun, shootFrom, gameObject, layerToIgnore, fireRateWait, bulletDamage, verticalSpread, horizontalSpread, aimOffset, hitEnemy, HitNONEnemy));
        }
    }

    private IEnumerator ShootAutoGun(GunHandler instance, AutoGun autoGun, Transform shootFrom, GameObject gameObject, LayerMask layerToIgnore, WaitForSeconds fireRateWait, float bulletDamage, float verticalSpread, float horizontalSpread, float aimOffset, GameObject hitEnemy, GameObject hitNONenemy)
    {
        if (!instance.Reloading && instance.AutoGunCurrentAmmo > 0)
        {
            if (!instance.InfiniteAmmo)
            {
                instance.AutoGunCurrentAmmo--;
            }
            GameObject lineDrawer = new GameObject();
            LineRenderer lineRenderer = lineDrawer.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            
            Vector3 aimSpot = instance.FPSCam.transform.position;
            aimSpot += instance.FPSCam.transform.forward * aimOffset;
            shootFrom.LookAt(aimSpot);

            Vector3 direction = shootFrom.transform.forward; // your initial aim.
            Vector3 spread = Vector3.zero;
            spread += shootFrom.transform.up * Random.Range(-verticalSpread, verticalSpread);
            spread += shootFrom.transform.right * Random.Range(-horizontalSpread, horizontalSpread);
            direction += spread.normalized; //* Random.Range(0f, 0.2f);

            RaycastHit hitInfo;

            if (Physics.Raycast(shootFrom.transform.position, direction, out hitInfo, float.MaxValue, ~layerToIgnore))
            {

                Debug.DrawLine(shootFrom.transform.position, hitInfo.point, Color.green, 1f);
                lineRenderer.material.color = Color.green;
                lineRenderer.SetPosition(0, shootFrom.transform.position);
                lineRenderer.SetPosition(1, hitInfo.point);

                if (hitInfo.transform.name != "Player")
                {
                    try
                    {
                        IDamageable damageableTarget = hitInfo.transform.GetComponent<IDamageable>();
                        Vector3 targetPosition = hitInfo.transform.position;

                        float distance = Vector3.Distance(targetPosition, gameObject.transform.position);
                        float totalDamage = Mathf.Abs(bulletDamage / ((distance / 2)));
                        damageableTarget.TakeDamage(totalDamage);

                        Debug.Log($"{hitInfo.transform.name}: {damageableTarget.Health}");
                        Debug.Log($"TakeDamage Dealt: {totalDamage}");
                        Instantiate(hitEnemy, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                    }
                    catch
                    {
                        Debug.Log("Not an IDamageable");
                        Instantiate(hitNONenemy, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                    }
                }
            }
            else
            {
                lineRenderer.material.color = Color.red;
                lineRenderer.SetPosition(0, shootFrom.transform.position);
                lineRenderer.SetPosition(1, shootFrom.transform.position + direction * 10);
                Debug.DrawLine(shootFrom.transform.position, shootFrom.transform.position + direction * 10, Color.red, 1f);
            }

            yield return fireRateWait;

            if (holdingTrigger && !instance.Reloading)
            {
                instance.StartCoroutine(autoGun.ShootAutoGun(instance, autoGun, shootFrom, gameObject, layerToIgnore, fireRateWait, bulletDamage, verticalSpread, horizontalSpread, aimOffset, hitEnemy, hitNONenemy));
            }
        }
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

    private void OnWeaponSwitch(GunHandler instance, IGun autoGun, WaitForSeconds reloadWait)
    {
        StopCoroutine(autoGun.Reload(instance, reloadWait));
    }
}
