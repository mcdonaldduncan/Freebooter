using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald
public sealed class LevelManager : MonoBehaviour
{
    [SerializeField] public FirstPersonController Player;
    [SerializeField] public GameObject CheckPointPrefab;

    List<CheckPoint> m_CheckPoints;

    CheckPoint m_CurrentCheckPoint;

    private static float stopTimeDuration;
    private static float stopTimeStart;
    private static bool timeStopped;
    
    public CheckPoint CurrentCheckPoint { get { return m_CurrentCheckPoint; } set { m_CurrentCheckPoint = value; } }

    public static LevelManager Instance { get; private set; }

    public delegate void PlayerRespawnDelegate();
    public static event PlayerRespawnDelegate PlayerRespawn;
    public static event PlayerRespawnDelegate CheckPointReached;

    public delegate void PlayerCombatDelegate(bool inCombat);
    public event PlayerCombatDelegate CombatStateChanged;

    private int CombatantCount;

    private bool InCombat;

    [SerializeField] private float TotalDamageTaken;
    [SerializeField] private float TotalDamageDealt;
    [SerializeField] private int EnemiesDefeated;
    [SerializeField] private int PlayerDeaths;

    float LevelStartTime;
    float LevelEndTime;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        LevelStartTime = Time.unscaledTime;

        if (Player == null)
        {
            Player = GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonController>();
        }

        TotalDamageTaken = 0;
        Player.PlayerDamaged += OnPlayerDamaged;

        timeStopped = false;
        CombatantCount = 0;

        var baseEnemies = FindObjectsOfType<NewAgentBase>(true).ToArray<IEnemy>();
        var swarmers = FindObjectsOfType<EnemySwarmerBehavior>(true).ToArray<IEnemy>();

        var enemies = baseEnemies.Concat(swarmers).ToArray();

        foreach (var enemy in enemies)
        {
            enemy.CombatStateChanged += OnCombatStateChanged;
            enemy.EnemyDefeated += OnEnemyDefeated;
        }
    }

    private void Update()
    {
        if (timeStopped)
        {
            if (stopTimeStart + stopTimeDuration < Time.unscaledTime)
            {
                Time.timeScale = 1.0f;
                timeStopped = false;
            }
        }
    }

    public void RegisterDamageTracker(IDamageTracking tracker)
    {
        tracker.DamageDealt += OnDamageDealt;
    }
    
    public void DeRegisterDamageTracker(IDamageTracking tracker)
    {
        tracker.DamageDealt -= OnDamageDealt;
    }

    private void OnDamageDealt(float damage)
    {
        TotalDamageDealt += damage;
    }

    private void OnEnemyDefeated(bool isDead)
    {
        EnemiesDefeated++;
    }

    private void OnPlayerDamaged(float damage)
    {
        TotalDamageTaken += damage;
    }

    private void OnLevelEnd()
    {
        LevelEndTime = Time.unscaledTime;
    }

    private void OnCombatStateChanged(bool combatState)
    {
        CombatantCount = combatState ? ++CombatantCount : --CombatantCount;

        if (CombatantCount > 0 && !InCombat)
        {
            CombatStateChanged?.Invoke(true);
            InCombat = true;
        }

        if (CombatantCount <= 0 && InCombat)
        {
            CombatStateChanged?.Invoke(false);
            InCombat = false;
        }
    }

    public static void TimeStop(float duration)
    {
        Time.timeScale = 0.0f;
        stopTimeStart = Time.unscaledTime;
        stopTimeDuration = duration;
        timeStopped = true;
    }

    public static void TogglePause(bool shouldPause)
    {
        if (shouldPause == true)
        {
            Time.timeScale = 0.0f;
        }
        else
        {
            Time.timeScale = 1.0f;
        }
    }

    public void FirePlayerRespawn()
    {
        timeStopped = false;
        Time.timeScale = 1.0f;
        PlayerRespawn?.Invoke();
        PlayerDeaths++;
    }

    public void UpdateCurrentCP(CheckPoint cp)
    {
        m_CurrentCheckPoint = cp;
        CheckPointReached?.Invoke();
    }

    public GameObject AddCheckPoint()
    {
        GameObject cp = Instantiate(CheckPointPrefab, transform);
        m_CheckPoints.Add(cp.GetComponent<CheckPoint>());
        cp.name = $"Checkpoint_{m_CheckPoints.Count}";
        return cp;
    }

    public GameObject RemoveCheckPoint()
    {
        CheckPoint temp = m_CheckPoints[m_CheckPoints.Count - 1];
        m_CheckPoints.RemoveAt(m_CheckPoints.Count - 1);
        return temp.gameObject;
    }

    public void ReturnToMainMenu()
    {
        timeStopped = false;
        Time.timeScale = 1.0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(0);
    }

    void SetIndices()
    {
        for (int i = 0; i < m_CheckPoints.Count; i++)
        {
            m_CheckPoints[i].m_Index = i;
        }
        //Debug.Log("");
    }

}



//List<IDamageable> m_RespawnBuffer;

//public List<IDamageable> RespawnBuffer { get { return m_RespawnBuffer; } set { m_RespawnBuffer = value; } }