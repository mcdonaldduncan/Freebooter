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
    //bool Reloading { get; set; }

    //void Shoot();
    IEnumerator Reload(GunHandler instance, WaitForSeconds reloadWait);
    //IEnumerator SpawnTrail(TrailRenderer trail, RaycastHit hit);
}
