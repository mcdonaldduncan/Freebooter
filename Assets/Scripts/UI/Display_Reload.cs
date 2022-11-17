using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Display_Reload : MonoBehaviour
{
    [SerializeField] private GunHandler Cgun;
    private TextMeshProUGUI reloadText;

    private void Start()
    {
        reloadText = GetComponent<TextMeshProUGUI>();
        reloadText.enabled = false;
    }

    void Update()
    {
        if(Cgun.CurrentGun.CurrentAmmo <= Cgun.CurrentGun.CurrentMaxAmmo * .25 && !Cgun.Reloading)
        {
            reloadText.text = "Press R to reload";
            reloadText.enabled = true;
        }
        if (Cgun.Reloading)
        {
            reloadText.text = "Reloading...";
            reloadText.enabled = true;
        }
        else if(Cgun.CurrentGun.CurrentAmmo > Cgun.CurrentGun.CurrentMaxAmmo * .25 && !Cgun.Reloading)
        {
            reloadText.enabled = false;
        }
    }

}
