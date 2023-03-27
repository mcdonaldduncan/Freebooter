using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IShooting
{
    GameObject ProjectilePrefab { get; }
    Transform ShootFrom { get; }

    float TimeBetweenShots { get; }
    float LastShotTime { get; set; }

    bool ShouldShoot => Time.time > LastShotTime + TimeBetweenShots;

    bool AltShootFrom { get; set; }

    void Shoot()
    {
        if (!ShouldShoot) return;

        Vector3 direction = LevelManager.Instance.Player.transform.position - ShootFrom.position;

        ProjectileManager.Instance.TakeFromPool(ProjectilePrefab, ShootFrom.position, out Projectile projectile);
        projectile.Launch(direction);
        projectile.transform.LookAt(projectile.transform.position + direction);

        LastShotTime = Time.time;
        AltShootFrom = !AltShootFrom;
    }

    void Shoot(Transform shootFrom)
    {
        if (!ShouldShoot) return;

        Vector3 direction = LevelManager.Instance.Player.transform.position - shootFrom.position;

        ProjectileManager.Instance.TakeFromPool(ProjectilePrefab, shootFrom.position, out Projectile projectile);
        projectile.Launch(direction);
        projectile.transform.LookAt(projectile.transform.position + direction);

        LastShotTime = Time.time;
        AltShootFrom = !AltShootFrom;
    }

    void Shoot(bool shootStraight)
    {
        if (!ShouldShoot) return;

        //Vector3 direction = LevelManager.Instance.Player.transform.position - ShootFrom;

        ProjectileManager.Instance.TakeFromPool(ProjectilePrefab, ShootFrom.position, out Projectile projectile);
        projectile.Launch(ShootFrom.forward);
        projectile.transform.LookAt(projectile.transform.position + ShootFrom.forward);

        LastShotTime = Time.time;
        AltShootFrom = !AltShootFrom;
    }

    void Shoot(Transform shootFrom, bool shootStraight)
    {
        if (!ShouldShoot) return;

        //Vector3 direction = LevelManager.Instance.Player.transform.position - shootFrom;

        ProjectileManager.Instance.TakeFromPool(ProjectilePrefab, shootFrom.position, out Projectile projectile);
        projectile.Launch(shootFrom.forward);
        projectile.transform.LookAt(projectile.transform.position + shootFrom.forward);

        LastShotTime = Time.time;
        AltShootFrom = !AltShootFrom;
    }
}
