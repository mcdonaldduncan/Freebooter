using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ShotGun : MonoBehaviour, IGun
{
    private void OnEnable()
    {
        GunHandler.weaponSwitched += OnWeaponSwitch;
    }
    private void OnDisable()
    {
        GunHandler.weaponSwitched -= OnWeaponSwitch;
    }

    public static void Shoot(Camera fpsCam, Transform shootFrom, GameObject gameObject, LayerMask layerToIgnore, float bulletDamage, float shotGunBulletAmount, float verticalSpread, float horizontalSpread, float aimOffset, GameObject hitenemy, GameObject hitNONenemy)
    {
        for (int i = 0; i < shotGunBulletAmount; i++)
        {
            GameObject lineDrawer = new GameObject();
            LineRenderer lineRenderer = lineDrawer.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;

            Vector3 aimSpot = fpsCam.transform.position;
            aimSpot += fpsCam.transform.forward * aimOffset;
            shootFrom.LookAt(aimSpot);

            Vector3 direction = shootFrom.transform.forward; // your initial aim.
            Vector3 spread = Vector3.zero;
            spread += shootFrom.transform.up * Random.Range(-verticalSpread, verticalSpread);
            spread += shootFrom.transform.right * Random.Range(-horizontalSpread, horizontalSpread);
            direction += spread.normalized * Random.Range(0f, 0.2f);

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
                        Instantiate(hitenemy, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                        //hitenemy.Play();
                    }
                    catch
                    {
                        Debug.Log("Not an IDamageable");
                        Instantiate(hitNONenemy, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                        //hitNONenemy.Play();
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
        }
    }

    public static void StartReload(GunHandler instance, ShotGun shotGun, WaitForSeconds reloadWait)
    {
        instance.StartCoroutine(shotGun.Reload(instance, reloadWait));
    }

    public IEnumerator Reload(GunHandler instance, WaitForSeconds reloadWait)
    {
        instance.Reloading = true;
        yield return reloadWait;
        instance.Reloading = false;
        instance.ShotGunCurrentAmmo = instance.ShotGunMaxAmmo;
    }

    private void OnWeaponSwitch(GunHandler instance, IGun shotGun, WaitForSeconds reloadWait)
    {
        StopCoroutine(shotGun.Reload(instance, reloadWait));
    }
}
