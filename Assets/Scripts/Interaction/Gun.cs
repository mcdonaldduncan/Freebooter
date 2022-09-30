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
        longGun,
        autoGun
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

    [SerializeField] private int autoGunHorizontalSpread;
    [SerializeField] private int autoGunVerticalSpread;
    [SerializeField] private float fireRate;

    [SerializeField] private List<GameObject> gunInventory;

    private bool holdingTrigger;

    private GameObject lineDrawer;
    private LineRenderer lineRenderer;
    private LinkedList<GunType> gunList;
    private LinkedListNode<GunType> handGunNode;
    private LinkedListNode<GunType> shotGunNode;
    private LinkedListNode<GunType> autoGunNode;
    private LinkedListNode<GunType> currentGun;

    private void Start()
    {
        gunList = new LinkedList<GunType>();
        handGunNode = new LinkedListNode<GunType>(GunType.handGun);
        shotGunNode = new LinkedListNode<GunType>(GunType.shotGun);
        autoGunNode = new LinkedListNode<GunType>(GunType.autoGun);

        lineDrawer = new GameObject();
        lineRenderer = lineDrawer.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        //This will be updated when we have player inventory
        gunList.AddLast(handGunNode);
        gunList.AddLast(shotGunNode);
        gunList.AddLast(autoGunNode);

        currentGun = gunList.First;
        gunType = currentGun.Value;
    }

    public void SwitchWeapon(InputAction.CallbackContext context)
    {
        if (currentGun == gunList.Last)
        {
            currentGun = gunList.First;
        }
        else
        {
            currentGun = currentGun.Next;
        }

        gunType = currentGun.Value;
        Debug.Log($"Equipped gun: {gunType.ToString()}");
    }

    //TODO: make the method unique to the gun and then call it based on the state
    //The pickups won't change the amount of guns the player has, it will simply change the behavior of the gun
    //so each gun type should have it's own script that implements an interface (or abstract class, idk yet)
    //within those gun classes, they will have enums with the different possible pickup versions of the gun
    //so like a small state machine within those gun scripts that can change the behavior of the gun
    //this will not only help to add unique abilities for each gun, but it will also help clean up the code a little bit
    //and likely help performance
    public void Shoot(InputAction.CallbackContext context)
    {
        if (gunType == GunType.autoGun)
        {
            if (context.canceled)
            {
                holdingTrigger = false;
                StopCoroutine(ShootAutoGun());
            }
            else if (context.performed)
            {
                holdingTrigger = true;

                StartCoroutine(ShootAutoGun());
            }
        }
        if ((gunType == GunType.handGun || gunType == GunType.longGun) && context.performed)
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
                        damageableTarget.Damage(bulletDamage / (Mathf.Abs(distance / 2)));

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
        
        if (gunType == GunType.shotGun && context.performed)
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
                    //lineRenderer.startColor = Color.red;
                    //lineRenderer.SetPosition(0, shootFrom.transform.position);
                    //lineRenderer.SetPosition(1, shootFrom.transform.position + direction * range);
                    Debug.DrawLine(shootFrom.transform.position, shootFrom.transform.position + direction * range, Color.red, 1f);
                }
            }
        }

    }

    private IEnumerator ShootAutoGun()
    {
        Debug.Log("Shooting bullet");
        Vector3 direction = shootFrom.transform.forward; // your initial aim.
        Vector3 spread = Vector3.zero;
        spread += shootFrom.transform.up * Random.Range(-autoGunVerticalSpread, autoGunVerticalSpread);
        spread += shootFrom.transform.right * Random.Range(-autoGunHorizontalSpread, autoGunHorizontalSpread);
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
            //lineRenderer.startColor = Color.red;
            //lineRenderer.SetPosition(0, shootFrom.transform.position);
            //lineRenderer.SetPosition(1, shootFrom.transform.position + direction * range);
            Debug.DrawLine(shootFrom.transform.position, shootFrom.transform.position + direction * range, Color.red, 1f);
        }

        yield return new WaitForSeconds(fireRate);

        if (holdingTrigger)
        {
            StartCoroutine(ShootAutoGun());
        }
    }
}
