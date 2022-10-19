using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGun
{
    GunHandler GunManager { get; set; }
    Transform ShootFrom { get; set; }
    LayerMask LayerToIgnore { get; set; }
    float FireRate { get; set; }
    float BulletDamage { get; set; }
    float VerticalSpread { get; set; }
    float HorizontalSpread { get; set; }
    float AimOffset { get; set; }
    GameObject HitEnemy { get; set; }
    GameObject HitNonEnemy { get; set; }
    float ReloadTime { get; set; }
    int CurrentAmmo { get; set; }
    int CurrentMaxAmmo { get; }
    CanvasGroup GunReticle { get; set; }
    TrailRenderer BulletTrail { get; set; }

    //void Shoot();
    IEnumerator Reload(WaitForSeconds reloadWait);
    //IEnumerator SpawnTrail(TrailRenderer trail, RaycastHit hit);
}
