using Assets.Scripts.Enemies.Agent_Base.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald
public class Inquisitor : MonoBehaviour, IDamageable, IGroupable
{
    [SerializeField] Animator m_Animator;
    [SerializeField] GameObject m_Follower_GO;
    [SerializeField] GameObject m_AttackSpawn;
    [SerializeField] GameObject m_Shield;
    [SerializeField] GameObject m_DeathExplosion;
    [SerializeField] GameObject m_PhaseEntry;
    [SerializeField] GameObject m_ShieldExplosion;
    [SerializeField] Transform m_Target;
    [SerializeField] float m_TimeBetweenAttacks;
    [SerializeField] float m_StartingHealth;
    [SerializeField] float m_FollowerCooldown;
    [SerializeField] float m_LaserDamage;
    [SerializeField] float m_MaxFollowerDistance;
    [SerializeField] float m_MaxForce;
    [SerializeField] float m_Speed;
    [SerializeField] float m_RotSpeed;
    [SerializeField] public List<FakeOrbit> m_Orbits;
    [SerializeField] List<Transform> m_FollowerSpawns;

    float lastAttackTime;

    bool canAttack => Time.time - lastAttackTime > m_TimeBetweenAttacks && !isReacting;

    Follower m_Follower;

    public float Health { get; set; }

    float cooldownProgress;

    bool isTracking = false;
    bool shieldsUp = true;
    bool inPhase2;
    bool isAttacking;
    bool isReacting;
    bool laserActive;

    Vector3 m_Velocity;
    Vector3 m_Acceleration;
    Vector3 m_TargetPosition;

    WaitForSeconds resetDelay = new WaitForSeconds(.25f);

    [Header("DamagePopUp")]
    [SerializeField] GameObject m_DamagePopUpPrefab;
    [SerializeField] Transform m_PopupFromHere;
    [SerializeField] bool m_showDamageNumbers;
    float m_fontSize = 5;

    public GameObject DamageTextPrefab { get => m_DamagePopUpPrefab; set => m_DamagePopUpPrefab = value; }
    public Transform TextSpawnLocation { get => m_PopupFromHere; set => m_PopupFromHere = value; }
    public float FontSize { get => m_fontSize; set => m_fontSize = value; }
    public bool ShowDamageNumbers { get => m_showDamageNumbers; set => m_showDamageNumbers = value; }
    public TextMeshPro Text { get; set; }
    public bool IsDead { get; set; }
    private IDamageable m_Damageable;

    private void OnEnable()
    {
        //potentialTargets = FindObjectsOfType<FirstPersonController>().Select(item => item.transform).ToList();
    }

    private void Awake()
    {
        m_Damageable = this;
    }

    void Start()
    {
        Init();
    }

    public void Init()
    {
        IsDead = false;
        m_Target = LevelManager.Instance.Player.transform;
        Health = m_StartingHealth;
    }

    public void TakeDamage(float damageTaken, HitBoxType hitbox, Vector3 hitPoint = default(Vector3))
    {
        if (damageTaken < 1) return;
        if (m_Shield.activeInHierarchy)
        {
            m_Damageable.InstantiateDamageNumber(0, HitBoxType.armored);
        }
        else 
        {
            Health -= damageTaken;
            m_Damageable.InstantiateDamageNumber(damageTaken, hitbox);
            CheckForDeath();
        }
    }

    public void CheckOrbits()
    {
        if (m_Orbits.Where(x => x.gameObject.activeSelf).Any())
            return;

        if (!shieldsUp)
        {
            EndAttack();
            m_Animator.SetBool("isAttacking", false);
            m_Animator.SetBool("isReacting", true);
            isReacting = true;
            Invoke(nameof(DeactivateShield), 2.5f);
        }

        if (shieldsUp && !inPhase2)
        {
            m_Animator.SetBool("Phase2", true);
            StartCoroutine(ResetOrbits());
            m_PhaseEntry.SetActive(true);
            shieldsUp = false;
            inPhase2 = true;
        }
    }

    void DeactivateShield()
    {
        m_ShieldExplosion.SetActive(true);
        m_Shield.SetActive(false);
    }

    IEnumerator ResetOrbits()
    {
        int i = 0;

        while (i < 2 && i < m_Orbits.Count)
        {
            m_Orbits[i].gameObject.SetActive(true);
            m_Orbits[i].Health = 3;
            i++;
            yield return null;
        }

        foreach (var orbit in m_Orbits)
        {
            if (!orbit.gameObject.activeSelf)
            {
                yield return resetDelay;
                orbit.gameObject.SetActive(true);
                orbit.Health = 3;
            }
        }
    }

    public void SetTracking(bool state)
    {
        isTracking = state;
    }

    public void CheckForDeath()
    {
        if (Health <= 0)
        {
            IsDead = true;
            m_DeathExplosion.SetActive(true);
            gameObject.SetActive(false);
            if (m_Follower == null) return;
            m_Follower.Despawn();
        }
    }

    public void ReactOver()
    {
        m_Animator.SetBool("isReacting", false);
        isReacting = false;
    }

    public void AttackOver()
    {
        m_Animator.SetBool("isAttacking", false);
    }

    public void MaintainAttack()
    {
        m_Animator.SetFloat("SpeedMult", 0);
        Invoke(nameof(EndAttack), 8);
        m_AttackSpawn.SetActive(true);
        isAttacking = true;
        laserActive = true;
        m_TargetPosition = m_Target.position;
        m_AttackSpawn.transform.LookAt(LevelManager.Instance.Player.transform.position);
    }

    private void EndAttack()
    {
        isAttacking = false;
        m_Animator.SetFloat("SpeedMult", 1);
        Invoke(nameof(Endbeam), 1);
        lastAttackTime = Time.time;
    }

    void Endbeam()
    {
        m_AttackSpawn.SetActive(false);
        laserActive = false;
    }

    void Update()
    {
        SpawnFollower();
        HandleAttacking();
        HandleRotation();
        HandleDamage();
        HandleTargeting();
    }

    public void SpawnFollower()
    {
        if (Vector3.Distance(transform.position, m_Target.position) > m_MaxFollowerDistance) return;
        
        if (isTracking) return;

        cooldownProgress += Time.deltaTime;

        if (cooldownProgress < m_FollowerCooldown) return;

        if (m_Follower == null)
        {
            m_Follower = Instantiate(m_Follower_GO).GetComponent<Follower>();
            m_Follower.Init(m_Target, this, m_FollowerSpawns[0].position);
        }
        else
        {
            m_Follower.Init(m_Target, this, m_FollowerSpawns[0].position);
        }
        
        isTracking = true;
        cooldownProgress = 0;
    }

    private void HandleAttacking()
    {
        if (!inPhase2) return;
        if (!canAttack) return;

        m_Animator.SetBool("isAttacking", true);
        isAttacking = true;
    }

    void HandleRotation()
    {
        if (!inPhase2) return;

        Vector3 lookDirection = transform.position - m_Target.position;
        Vector3 cross = Vector3.Cross(m_Target.TransformDirection(Vector3.up), lookDirection);
        Vector3 normalCross = cross.normalized;
        
        if (!isAttacking)
        {
            normalCross = Vector3.zero;
        }
        Vector3 adjustedLook = (m_Target.position + normalCross * 30f) - transform.position;
        var rotGoal = Quaternion.LookRotation(new Vector3(adjustedLook.x, transform.position.y, adjustedLook.z));
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotGoal, m_RotSpeed * Time.deltaTime);
        
        m_AttackSpawn.transform.LookAt(m_TargetPosition);

    }

    void HandleDamage()
    {
        if (!laserActive) return;

        if (Physics.Raycast(m_AttackSpawn.transform.position, m_TargetPosition - m_AttackSpawn.transform.position, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Player"))
            {
                LevelManager.Instance.Player.TakeDamage(m_LaserDamage * Time.deltaTime, HitBoxType.normal);
            }
        }
    }

    void HandleTargeting()
    {
        m_Acceleration += CalculateSteering(m_Target.position);
        m_Velocity += m_Acceleration;
        m_TargetPosition += m_Velocity * Time.deltaTime;
        m_Acceleration = Vector3.zero;
    }

    Vector3 CalculateSteering(Vector3 currentTarget)
    {
        Vector3 desired = currentTarget - m_TargetPosition;
        desired = desired.normalized;
        desired *= m_Speed;
        Vector3 steer = desired - m_Velocity;
        steer = steer.normalized;
        steer *= m_MaxForce;
        return steer;
    }
}
