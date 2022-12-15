using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Inquisitor : MonoBehaviour, IDamageable
{
    [SerializeField] Animator m_Animator;
    [SerializeField] GameObject follower_GO;
    [SerializeField] public List<FakeOrbit> orbits;
    [SerializeField] float StartingHealth;
    [SerializeField] float cooldown;
    [SerializeField] List<Transform> followerSpawns;
    [SerializeField] GameObject Shield;
    [SerializeField] float timeBetweenAttacks;
    [SerializeField] GameObject attackSpawn;
    [SerializeField] float m_Damping;
    [SerializeField] float laserDamage;

    float lastAttackTime;

    bool canAttack => Time.time - lastAttackTime > timeBetweenAttacks && !isReacting;

    Follower follower;

    public float Health { get; set; }

    //public List<Transform> potentialTargets;

    public bool isTracking = false;

    float cooldownProgress;

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

    public void TakeDamage(float damageTaken)
    {
        Health -= damageTaken;
        CheckForDeath();
    }

    public void CheckOrbits()
    {
        if (orbits.Where(x => x.gameObject.activeSelf).Any())
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
        Shield.SetActive(false);
    }

    IEnumerator ResetOrbits()
    {
        int i = 0;

        while (i < 2 && i < orbits.Count)
        {
            orbits[i].gameObject.SetActive(true);
            orbits[i].Health = 3;
            i++;
            yield return null;
        }

        foreach (var orbit in orbits)
        {
            if (!orbit.gameObject.activeSelf)
            {
                yield return resetDelay;
                orbit.gameObject.SetActive(true);
                orbit.Health = 3;
            }
        }
    }

    public void CheckForDeath()
    {
        if (Health <= 0)
        {
            //Debug.Log("Inquisitor Destroyed");
            
            gameObject.SetActive(false);
            if (follower == null) return;
            follower.Despawn();
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
        attackSpawn.SetActive(true);
        m_TargetPosition = LevelManager.Instance.Player.transform.position;
        attackSpawn.transform.LookAt(LevelManager.Instance.Player.transform.position);
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
        attackSpawn.SetActive(false);
    }

    void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        Health = StartingHealth;
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
        if (isTracking)
            return;

        cooldownProgress += Time.deltaTime;

        if (cooldownProgress < cooldown)
            return;

        if (follower == null)
        {
            follower = Instantiate(follower_GO).GetComponent<Follower>();
            follower.Init(LevelManager.Instance.Player.transform, this, followerSpawns[0].position);
        }
        else
        {
            follower.Init(LevelManager.Instance.Player.transform, this, followerSpawns[0].position);
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

        attackSpawn.transform.LookAt(m_TargetPosition);

    }

    void HandleDamage()
    {
        if (!isAttacking) return;

        if (Physics.Raycast(attackSpawn.transform.position, m_TargetPosition - attackSpawn.transform.position, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Player"))
            {
                LevelManager.Instance.Player.TakeDamage(laserDamage * Time.deltaTime);
            }
        }

    }
}
