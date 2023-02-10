using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankScript : AgentBase
{
    [Header("Shield GameObject")]
    public GameObject Shield; //for cycle

    [Header("ExplosiveGun")]
    [SerializeField] protected GameObject m_ProjectilePrefabExplosive;
    [SerializeField] protected Transform m_ShootFromTopGun;

    [Header("Shooting Options for ExplosiveGun")] 
    [SerializeField] protected float m_RangeExplosive;
    [SerializeField] float m_TimeBetweenShotsExplosive;

    protected float lastShotTimeExplosive; //made new timer for the explosive gun to shoot slower then normal guns
    protected bool shouldShootExplosive => Time.time > m_TimeBetweenShotsExplosive + lastShotTimeExplosive;

    private void Start()
    {
        HandleSetup();
    }

    public override void Shoot()
    {
        //shoots two lasers at a time
        if (!shouldShoot || distanceToPlayer > m_Range) return; // change logic to stop shooting when out of range because the aim method doesnt update the direction when out of range so they would shoot forward without aiming

        //laser 1
        GameObject obj = ProjectileManager.Instance.TakeFromPool(m_ProjectilePrefab, m_ShootFrom.position, out Projectile projectile);
        projectile.Launch(m_TargetDirection);
        projectile.transform.LookAt(projectile.transform.position + m_TargetDirection);

        //laser 2
        GameObject obj2 = ProjectileManager.Instance.TakeFromPool(m_ProjectilePrefab, m_ShootFrom2.position, out Projectile projectile2);
        projectile2.Launch(m_TargetDirection);
        projectile2.transform.LookAt(projectile2.transform.position + m_TargetDirection);

        altShoootFrom = !altShoootFrom;
        lastShotTime = Time.time;
    }

    public void ShootExplosiveGun()
    {
        //shoots rockets on a different timer
        if (!shouldShootExplosive || distanceToPlayer > m_RangeExplosive) return; // change logic to stop shooting when out of range because the aim method doesnt update the direction when out of range so they would shoot forward without aiming

        GameObject obj = ProjectileManager.Instance.TakeFromPool(m_ProjectilePrefabExplosive, m_ShootFromTopGun.position, out Projectile projectile);
        projectile.Launch(m_TargetDirection);
        projectile.transform.LookAt(projectile.transform.position + m_TargetDirection);

        altShoootFrom = !altShoootFrom;
        lastShotTimeExplosive = Time.time;
    }

    private void Update()
    {
        HandleAgentState();
    }

    public override void HandleAgentState()
    {
        distanceToPlayer = Vector3.Distance(transform.position, m_Target.position);

        switch (m_State)
        {
            case AgentState.GUARD:
                AimRestricted();
                if (CheckLineOfSight()) m_State = AgentState.CHASE;
                break;
            case AgentState.WANDER:
                Wander(); 
                AimRestricted();
                if (CheckLineOfSight()) m_State = AgentState.CHASE;
                break;
            case AgentState.CHASE:
                AimRestricted();
                Shoot();
                ShootExplosiveGun(); // added explosive gun to the state
                ChasePlayer();
                break;
            case AgentState.RETURN:
                ReturnToOrigin();
                break;
            default:
                break;
        }
    }

    protected override void CycleAgent() //added shield to the cycle
    {
        base.CycleAgent();
        var shieldScript = Shield.GetComponent<SpecialHitBoxScript>();
        shieldScript._health = shieldScript.maxHealth;
        Shield.SetActive(true);
    }
}
