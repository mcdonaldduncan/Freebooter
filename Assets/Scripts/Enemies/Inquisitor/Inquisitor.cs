using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inquisitor : MonoBehaviour, IDamageable
{
    [SerializeField] Animator m_Animator;
    [SerializeField] GameObject m_Follower_GO;
    [SerializeField] GameObject m_AttackSpawn;
    [SerializeField] GameObject m_Shield;
    [SerializeField] float m_TimeBetweenAttacks;
    [SerializeField] float m_StartingHealth;
    [SerializeField] float m_FollowerCooldown;
    [SerializeField] float m_Damping;
    [SerializeField] float m_LaserDamage;
    [SerializeField] float m_MaxFollowerDistance;
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

    Vector3 m_Velocity;
    Vector3 m_TargetPosition;

    WaitForSeconds resetDelay = new WaitForSeconds(.25f);

    private void OnEnable()
    {
        //potentialTargets = FindObjectsOfType<FirstPersonController>().Select(item => item.transform).ToList();
    }

    void Start()
    {
        Init();
    }

    public void Init()
    {
        Health = m_StartingHealth;
    }

    public void TakeDamage(float damageTaken)
    {
        Health -= damageTaken;
        CheckForDeath();
    }

    public void CheckOrbits()
    {
        if (m_Orbits.Where(x => x.gameObject.activeSelf).Any())
            return;

        if (!shieldsUp)
        {
            m_Animator.SetBool("isReacting", true);
            isReacting = true;
            Invoke(nameof(DeactivateShield), 2);
        }

        if (shieldsUp && !inPhase2)
        {
            m_Animator.SetBool("Phase2", true);
            StartCoroutine(ResetOrbits());
            shieldsUp = false;
            inPhase2 = true;
        }
    }

    void DeactivateShield()
    {
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
            //Debug.Log("Inquisitor Destroyed");
            
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
        m_TargetPosition = LevelManager.Instance.Player.transform.position;
        m_AttackSpawn.transform.LookAt(LevelManager.Instance.Player.transform.position);
    }

    private void EndAttack()
    {
        m_Animator.SetFloat("SpeedMult", 1);
        Invoke(nameof(Endbeam), 1);
        isAttacking = false;
        lastAttackTime = Time.time;
    }

    void Endbeam()
    {
        m_AttackSpawn.SetActive(false);
    }

    void Update()
    {
        SpawnFollower();
        HandleAttacking();
        HandleRotation();
        HandleDamage();
    }

    public void SpawnFollower()
    {
        if (Vector3.Distance(transform.position, LevelManager.Instance.Player.transform.position) > m_MaxFollowerDistance) return;
        
        if (isTracking) return;

        cooldownProgress += Time.deltaTime;

        if (cooldownProgress < m_FollowerCooldown) return;

        if (m_Follower == null)
        {
            m_Follower = Instantiate(m_Follower_GO).GetComponent<Follower>();
            m_Follower.Init(LevelManager.Instance.Player.transform, this, m_FollowerSpawns[0].position);
        }
        else
        {
            m_Follower.Init(LevelManager.Instance.Player.transform, this, m_FollowerSpawns[0].position);
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
        if (!isAttacking) return;
        
        Vector3 lookDirection = transform.position - LevelManager.Instance.Player.transform.position;
        Vector3 cross = Vector3.Cross(LevelManager.Instance.Player.transform.TransformDirection(Vector3.up), lookDirection);
        Vector3 normalCross = cross.normalized;

        transform.LookAt(new Vector3(LevelManager.Instance.Player.transform.position.x, transform.position.y, LevelManager.Instance.Player.transform.position.z)  + normalCross * 15f);

        var n1 = m_Velocity - (m_TargetPosition - LevelManager.Instance.Player.transform.position) * Mathf.Pow(m_Damping, 2) * Time.deltaTime;
        var n2 = 1 + m_Damping * Time.deltaTime;
        m_Velocity = n1 / n2;

        m_TargetPosition += m_Velocity * Time.deltaTime;

        m_AttackSpawn.transform.LookAt(m_TargetPosition);

    }

    void HandleDamage()
    {
        if (!isAttacking) return;

        if (Physics.Raycast(m_AttackSpawn.transform.position, m_TargetPosition - m_AttackSpawn.transform.position, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Player"))
            {
                LevelManager.Instance.Player.TakeDamage(m_LaserDamage * Time.deltaTime);
            }
        }

    }
}
