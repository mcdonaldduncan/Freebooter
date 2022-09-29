using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class Gun : MonoBehaviour
{
    [SerializeField]
    private enum GunType
    {
        handGun,
        shotGun,
        longGun
    }

    [Header("Gun Parameters")]
    [SerializeField]private GunType gunType;
    [Tooltip("This will apply to EACH 'bullet' the shotgun fires")][SerializeField] private float bulletDamage = 10f;
    [SerializeField] private float range;
    [SerializeField] private Transform shootFrom;
    [SerializeField] private int shotGunBulletAmount;

    [Tooltip("Increase for wider horizontal spread. This will be used to a find a random number between the negative of this and the positive.")]
    [SerializeField] private int shotGunHorizontalSpread;
    [Tooltip("Increase for wider vertical spread. This will be used to a find a random number between the negative of this and the positive.")]
    [SerializeField] private int shotGunVerticalSpread;

    private GameObject lineDrawer;
    private LineRenderer lineRenderer;

    private void Start()
    {
        lineDrawer = new GameObject();
        lineRenderer = lineDrawer.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
    }

    public void Shoot(InputAction.CallbackContext context)
    {
        if (gunType == GunType.handGun || gunType == GunType.longGun)
        {
            RaycastHit hitInfo;


            if (Physics.Raycast(shootFrom.transform.position, shootFrom.transform.forward, out hitInfo))
            {

                lineRenderer.startColor = Color.green;
                lineRenderer.SetPosition(0, shootFrom.transform.position);
                lineRenderer.SetPosition(1, hitInfo.point);

                Debug.DrawLine(shootFrom.transform.position, hitInfo.point, Color.green, 1f);

                if (hitInfo.transform.name != "Player")
                {
                    try
                    {
                        IDamageable damageableTarget = hitInfo.transform.GetComponent<IDamageable>();
                        Vector3 targetPosition = hitInfo.transform.position;

                        float distance = Vector3.Distance(targetPosition, gameObject.transform.position);
                        damageableTarget.Damage(gunDamage / (Mathf.Abs(distance / 2)));

                        Debug.Log($"{hitInfo.transform.name}: {damageableTarget.Health}");
                    }
                    catch
                    {
                        Debug.Log("Not an IDamageable");
                    }
                }
            }
            else
            {
                Debug.DrawLine(shootFrom.transform.position, shootFrom.transform.forward * range, Color.red, 1f);
                lineRenderer.startColor = Color.red;
                lineRenderer.SetPosition(0, shootFrom.transform.position);
                lineRenderer.SetPosition(1, shootFrom.transform.forward * range);
            }
        }
        
        if (gunType == GunType.shotGun)
        {
            for (int i = 0; i < shotGunBulletAmount; i++)
            {
                Debug.Log("Shooting bullet");
                Vector3 direction = shootFrom.transform.forward; // your initial aim.
                Vector3 spread = Vector3.zero;
                spread += shootFrom.transform.up * Random.Range(-shotGunVerticalSpread, shotGunVerticalSpread);
                spread += shootFrom.transform.right * Random.Range(-shotGunHorizontalSpread, shotGunHorizontalSpread);
                direction += spread.normalized * Random.Range(0f, 0.2f);

                RaycastHit hitInfo;

                if (Physics.Raycast(shootFrom.transform.position, direction, out hitInfo))
                {

                    Debug.DrawLine(shootFrom.transform.position, hitInfo.point, Color.green, 1f);
                    //lineRenderer.startColor = Color.green;
                    //lineRenderer.SetColors(Color.green, Color.green);
                    //lineRenderer.SetPosition(0, shootFrom.transform.position);
                    //lineRenderer.SetPosition(1, hitInfo.point);

                    if (hitInfo.transform.name != "Player")
                    {
                        try
                        {
                            IDamageable damageableTarget = hitInfo.transform.GetComponent<IDamageable>();
                            Vector3 targetPosition = hitInfo.transform.position;

                            float distance = Vector3.Distance(targetPosition, gameObject.transform.position);
                            float totalDamage = Mathf.Abs(gunDamage / ((distance / 2)));
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
                    //lineRenderer.startColor = Color.red;
                    //lineRenderer.SetPosition(0, shootFrom.transform.position);
                    //lineRenderer.SetPosition(1, shootFrom.transform.position + direction * range);
                    Debug.DrawLine(shootFrom.transform.position, shootFrom.transform.position + direction * range, Color.red, 1f);
                }
            }
        }
    }
}
