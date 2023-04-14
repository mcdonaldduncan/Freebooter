using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateDash : MonoBehaviour
{
    [SerializeField] private DashGearBehavior[] dashGears;
    private FirstPersonController Player;
    private bool subscribedToDash = false;
    public delegate void UpdateDashDelegate();
    public static UpdateDashDelegate DashCooldownCompleted;


    private void Start()
    {
        Player = LevelManager.Instance.Player;
        Player.PlayerDashed += OnPlayerDash;
        //Player.OnDashCooldown += OnDashCooldown;
        subscribedToDash = true;
    }

    private void OnPlayerDash()
    {
        foreach (DashGearBehavior dashGear in dashGears)
        {
            if (dashGear.IsSpinning) continue;

            dashGear.SpinGear();
            break;

        }
    }

    //private void OnDashCooldown()
    //{
    //    foreach (DashGearBehavior dashGear in dashGears)
    //    {
    //        if (!dashGear.IsSpinning) continue;

    //        dashGear.StopGear();
    //        break;
    //    }
    //}

    private void OnEnable()
    {
        if (Player != null && !subscribedToDash)
        {
            Player.PlayerDashed += OnPlayerDash;
            //Player.OnDashCooldown += OnDashCooldown;
            subscribedToDash = true;
        }
    }
    private void OnDisable()
    {
        if (Player != null && subscribedToDash)
        {
            Player.PlayerDashed -= OnPlayerDash;
            //Player.OnDashCooldown -= OnDashCooldown;
            subscribedToDash = false;
        }
    }

    //private void OnEnable()
    //{
    //    FirstPersonController.OnPlayerDashed += OnPlayerDashed;
    //    FirstPersonController.OnDashCooldown += OnDashCooldown;
    //}
    //private void OnDisable()
    //{
    //    FirstPersonController.OnPlayerDashed -= OnPlayerDashed;
    //    FirstPersonController.OnDashCooldown -= OnDashCooldown;
    //}

    //private void OnPlayerDashed()
    //{
    //    Dashbar.fillAmount -= Player.DashTimeDifference;
    //}

    //private void OnDashCooldown()
    //{
    //    if (Player.DashesRemaining > 0)
    //    {
    //        if (Dashbar.fillAmount < 0.5f)
    //        {
    //            Dashbar.fillAmount = 0.5f;
    //        }
    //        else if (Dashbar.fillAmount >= 0.5f)
    //        {
    //            Dashbar.fillAmount = 1f;
    //        }
    //    }
    //}
}
