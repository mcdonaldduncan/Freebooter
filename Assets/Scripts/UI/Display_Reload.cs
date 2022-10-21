using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Display_Reload : MonoBehaviour
{
    public GunHandler Cgun;
    public TextMeshProUGUI text;

    private void Start()
    {
        text.enabled = false;
    }

     void Update()
    {
         if(Cgun.CurrentGun.CurrentAmmo <= Cgun.CurrentGun.CurrentMaxAmmo * .25)
        {
            text.enabled = true;
        }
        else if(Cgun.CurrentGun.CurrentAmmo > Cgun.CurrentGun.CurrentMaxAmmo * .25)
        {
            text.enabled = false;
        }
    }

}
