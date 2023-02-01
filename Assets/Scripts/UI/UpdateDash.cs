using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateDash : MonoBehaviour
{
    private Image Dashbar;
    public float DashAmount = 100;
    FirstPersonController Player;

    private void Start()
    {
        Dashbar = GetComponent<Image>();
        Player = FindObjectOfType<FirstPersonController>();
        Dashbar.fillAmount = 0;
    }

    private void Update()
    {
        Dashbar.fillAmount = Player.DashesRemaining / Player.DashesAllowed;
    }

    //private void OnEnable()
    //{
    //    FirstPersonController.playerDashed += PlayerDashed;
    //    FirstPersonController.dashCooldown += DashCooldown;
    //}
    //private void OnDisable()
    //{
    //    FirstPersonController.playerDashed -= PlayerDashed;
    //    FirstPersonController.dashCooldown -= DashCooldown;
    //}

    //private void PlayerDashed()
    //{
    //    Dashbar.fillAmount -= Player.DashTimeDifference;
    //}

    //private void DashCooldown()
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
