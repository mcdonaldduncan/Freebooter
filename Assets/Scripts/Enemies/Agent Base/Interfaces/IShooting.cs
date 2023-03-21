using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IShooting
{
    GameObject ProjectilePrefab { get; }
    Vector3 ShootFrom { get; }

    float TimeBetweenShots { get; }
    float LastShotTime { get; set; }

    bool ShouldShoot => Time.time > LastShotTime + TimeBetweenShots;

    bool AltShootFrom { get; set; }

    void Shoot()
    {
        if (!ShouldShoot) return;

        Vector3 direction = LevelManager.Instance.Player.transform.position - ShootFrom;

        ProjectileManager.Instance.TakeFromPool(ProjectilePrefab, ShootFrom, out Projectile projectile);
        projectile.Launch(direction);
        projectile.transform.LookAt(projectile.transform.position + direction);

        LastShotTime = Time.time;
        AltShootFrom = !AltShootFrom;
    }

    void Shoot(Vector3 shootFrom)
    {
        if (!ShouldShoot) return;

        Vector3 direction = LevelManager.Instance.Player.transform.position - shootFrom;

        ProjectileManager.Instance.TakeFromPool(ProjectilePrefab, shootFrom, out Projectile projectile);
        projectile.Launch(direction);
        projectile.transform.LookAt(projectile.transform.position + direction);

        LastShotTime = Time.time;
        AltShootFrom = !AltShootFrom;
    }
}
