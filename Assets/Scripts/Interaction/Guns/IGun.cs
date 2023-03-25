using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IGun
{
    GunHandler GunManager { get; set; }
    Transform ShootFrom { get; set; }
    LayerMask LayerToIgnore { get; set; }
    float FireRate { get; set; }
    //float GrenadeDamage { get; set; }
    float MaxDamage { get; set; }
    float MinDamage { get; set; }
    float DropStart { get; set; }
    float DropEnd { get; set; }
    //float DamageDrop { get; set; }
    float VerticalSpread { get; set; }
    float HorizontalSpread { get; set; }
    float AimOffset { get; set; }
    GameObject HitEnemy { get; set; }
    GameObject HitNonEnemy { get; set; }
    int CurrentAmmo { get; set; }
    int MaxAmmo { get; }
    CanvasGroup GunReticle { get; set; }
    GameObject Bullet { get; set; }
    AudioClip GunShotAudio { get; set; }
    GameObject GunModel { get; set; }
    GunAnimationHandler GunAnimationHandler { get; set; }
    TrailRenderer BulletTrailRenderer { get; set; }
    bool CanShoot { get; }
    float ShakeDuration { get; set; }
    float ShakeMagnitude { get; set; }
    float ShakeDampen { get; set; }

    void ShootTriggered(InputAction.CallbackContext context);
    void AlternateTriggered(InputAction.CallbackContext context);
    //void StartReload();
    //IEnumerator Reload(WaitForSeconds reloadWait);
}
