using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class GunHandler : MonoBehaviour
{
    [SerializeField]
    private enum GunType
    {
        handGun,
        shotGun,
        longGun,
        autoGun
    }

    [Header("Gun Handler Parameters")]
    [SerializeField] private GunType gunType;
    [SerializeField] private Transform shootFrom;
    [SerializeField] private LayerMask playerLayer;


    [Header("Handgun Parameters")]
    [SerializeField] private float handGunBulletDamage = 10f;
    [SerializeField] private float handGunVerticalSpread;
    [SerializeField] private float handGunHorizontalSpread;

    [Header("Shotgun Parameters")]
    [Tooltip("This will apply to EACH 'bullet' the shotgun fires")]
    [SerializeField] private float shotGunBulletDamage = 10f;
    [SerializeField] private int shotGunBulletAmount;
    [Tooltip("Increase for wider vertical spread. This will be used to a find a random number between the negative of this and the positive.")]
    [SerializeField] private float shotGunVerticalSpread;
    [Tooltip("Increase for wider horizontal spread. This will be used to a find a random number between the negative of this and the positive.")]
    [SerializeField] private float shotGunHorizontalSpread;

    [Header("Autogun Parameters")]
    [SerializeField] private float autoGunBulletDamage = 10f;
    [SerializeField] private float autoGunHorizontalSpread;
    [SerializeField] private float autoGunVerticalSpread;
    [SerializeField] private float fireRate;

    [Header("Longgun Parameters")]
    [SerializeField] private float longGunBulletDamage = 10f;

    [SerializeField] private List<GameObject> gunInventory;

    private bool holdingTrigger;

    private AutoGun autoGun;
    private WaitForSeconds fireRateWait;
    private GameObject lineDrawer;
    private LineRenderer lineRenderer;
    private LinkedList<GunType> gunList;
    private LinkedListNode<GunType> handGunNode;
    private LinkedListNode<GunType> shotGunNode;
    private LinkedListNode<GunType> autoGunNode;
    private LinkedListNode<GunType> currentGun;

    private void Start()
    {
        fireRateWait = new WaitForSeconds(fireRate);

        autoGun = new AutoGun();

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
            AutoGun.Shoot(this, autoGun, shootFrom, gameObject, playerLayer, context, fireRateWait, autoGunBulletDamage, autoGunVerticalSpread, autoGunHorizontalSpread);
        }
        if ((gunType == GunType.handGun || gunType == GunType.longGun) && context.performed)
        {
            HandGun.Shoot(shootFrom, gameObject, playerLayer, handGunBulletDamage, handGunVerticalSpread, handGunHorizontalSpread);
        }
        
        if (gunType == GunType.shotGun && context.performed)
        {
            ShotGun.Shoot(shootFrom, gameObject, playerLayer, shotGunBulletDamage, shotGunBulletAmount, shotGunVerticalSpread, shotGunHorizontalSpread);
        }

    }
}
