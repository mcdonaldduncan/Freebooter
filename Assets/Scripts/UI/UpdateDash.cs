using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateDash : MonoBehaviour
{
    private Image Dashbar;
    public float DashAmount = 100;
    private float MaxDashAmount;
    private float displayValue;
    private float fillAmountStart;
    private float fillAmountEnd;
    private float dashOneTime;
    private float dashTwoTime;
    private float lowerDashTime;
    FirstPersonController Player;

    private void Start()
    {
        Dashbar = GetComponent<Image>();
        Player = FindObjectOfType<FirstPersonController>();
        Dashbar.fillAmount = 1;
    }

    private void Update()
    {
        if (Player.UpdateDashBar)
        {
            if (Player.DashesRemaining == 1)
            {
                lowerDashTime += Time.deltaTime;
                displayValue = Mathf.Lerp(1, 0.5f, lowerDashTime / Player.DashTime);
            }
            if (Player.DashesRemaining == 0)
            {
                lowerDashTime += Time.deltaTime;
                displayValue = Mathf.Lerp(0.5f, 0, lowerDashTime / Player.DashTime);
            }
            Dashbar.fillAmount = displayValue;
        }
        else
        {
            lowerDashTime = 0;
        }

        if (Dashbar.fillAmount != 1 && !Player.UpdateDashBar)
        {
            if (Player.DashesRemaining == 0f)
            {
                dashOneTime += Time.deltaTime;
                displayValue = Mathf.Lerp(0, 0.5f, dashOneTime / Player.DashCooldownTime);
            }
            if (Player.DashesRemaining == 1f)
            {
                dashTwoTime += Time.deltaTime;
                displayValue = Mathf.Lerp(0.5f, 1, dashTwoTime / Player.DashCooldownTime);
            }

            Dashbar.fillAmount = displayValue;
        }
        else
        {
            dashOneTime = 0;
            dashTwoTime = 0;
        }
    }

    private void OnEnable()
    {
        FirstPersonController.playerDashed += PlayerDashed;
    }
    private void OnDisable()
    {
        FirstPersonController.playerDashed -= PlayerDashed;
    }

    private void PlayerDashed()
    {
        if (Player.DashesRemaining == 1)
        {
            Dashbar.fillAmount = 0.5f;
        }
        if (Player.DashesRemaining == 0)
        {
            Dashbar.fillAmount = 0;
        }
        dashOneTime = 0;
        dashTwoTime = 0;
    }
}
