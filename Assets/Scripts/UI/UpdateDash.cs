using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateDash : MonoBehaviour
{
    private Image Dashbar;
    public float DashAmount = 100;
    private float MaxDashAmount;
    FirstPersonController Player;

    private void Start()
    {
        Dashbar = GetComponent<Image>();
        Player = FindObjectOfType<FirstPersonController>();
        //this.MaxDashAmount = ;
    }
}
