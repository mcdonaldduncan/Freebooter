using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ShotGun : MonoBehaviour
{
    public static void Shoot(Transform shootFrom, GameObject gameObject, LayerMask layerToIgnore, float bulletDamage, float shotGunBulletAmount, float verticalSpread, float horizontalSpread)
    {
        for (int i = 0; i < shotGunBulletAmount; i++)
        {
            GameObject lineDrawer = new GameObject();
            LineRenderer lineRenderer = lineDrawer.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;

            Vector3 direction = shootFrom.transform.forward; // your initial aim.
            Vector3 spread = Vector3.zero;
            spread += shootFrom.transform.up * Random.Range(-verticalSpread, verticalSpread);
            spread += shootFrom.transform.right * Random.Range(-horizontalSpread, horizontalSpread);
            direction += spread.normalized * Random.Range(0f, 0.2f);

            RaycastHit hitInfo;

            if (Physics.Raycast(shootFrom.transform.position, direction, out hitInfo, 100f, ~layerToIgnore))
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
                        damageableTarget.Damage(totalDamage);

                        Debug.Log($"{hitInfo.transform.name}: {damageableTarget.Health}");
                        Debug.Log($"Damage Dealt: {totalDamage}");
                    }
                    catch
                    {
                        Debug.Log("Not an IDamageable");
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

    private IEnumerator Reload(GunHandler instance, WaitForSeconds reloadWait)
    {
        instance.reloading = true;
        yield return reloadWait;
        instance.reloading = false;
        instance.shotGunCurrentAmmo = instance.shotGunMaxAmmo;
    }
}
