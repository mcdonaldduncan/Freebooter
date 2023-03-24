using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AmmoUIController : MonoBehaviour
{
    [SerializeField] GunHandler gunController;
    [SerializeField] TMP_Text ammoText;
    [SerializeField] Image ammoImage;
    [SerializeField] TMP_FontAsset shotgunFont;
    [SerializeField] TMP_FontAsset autogunFont;
    [SerializeField] TMP_FontAsset grenadegunFont;
    [SerializeField] Sprite shotgunSprite;
    [SerializeField] Sprite autogunSprite;
    [SerializeField] Sprite grenadegunSprite;

    private void OnEnable()
    {
        GunHandler.weaponSwitched += ChangeUI;
    }

    private void OnDisable()
    {
        GunHandler.weaponSwitched -= ChangeUI;
    }

    private void ChangeUI()
    {
        if (gunController.CurrentGun.GetType() == typeof(ShotGun))
        {
            ammoText.font = shotgunFont;
            ammoImage.sprite = shotgunSprite;
        }
        else if (gunController.CurrentGun.GetType() == typeof(AutoGun))
        {
            ammoText.font = autogunFont;
            ammoImage.sprite = autogunSprite;
        }
        else if (gunController.CurrentGun.GetType() == typeof(GrenadeGun))
        {
            ammoText.font = grenadegunFont;
            ammoImage.sprite = grenadegunSprite;
        }
    }


}
