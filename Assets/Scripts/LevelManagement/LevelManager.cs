using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    [SerializeField] private GameObject ScorePanel;
    [SerializeField] private GameObject UIPanel;
    [SerializeField] private TextMeshProUGUI LevelTime;
    [SerializeField] private TextMeshProUGUI DamageDealt;
    [SerializeField] private TextMeshProUGUI DamageTaken;
    [SerializeField] private TextMeshProUGUI EnemyKills;
    [SerializeField] private TextMeshProUGUI PlayerDeath;
    [SerializeField] private Image PanelImage;
    

    [SerializeField] float FadeSpeed;

    float LevelStartTime;
    float LevelEndTime;

    float BackgroundPanelAlpha;
    Coroutine FadeRoutineInstance;

    void Awake()
    {
        ScorePanel.SetActive(false);
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
        BackgroundPanelAlpha = 0;
        LevelStartTime = Time.unscaledTime;

        if (Player == null) Player = GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonController>();

        TotalDamageTaken = 0;
        Player.PlayerDamaged += OnPlayerDamaged;

        timeStopped = false;
        TogglePause(false);
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

    private void OnLevelEnd()
    {

        StopCoroutine(FadeRoutineInstance);

        var totalTime = TimeSpan.FromSeconds(LevelEndTime - LevelStartTime);

        StringBuilder sb = new StringBuilder();
        sb.Append(totalTime.Minutes);
        sb.Append(":");
        sb.Append(totalTime.Seconds);

        LevelTime.text = sb.ToString();

        DamageDealt.text = TotalDamageDealt.ToString("n2");
        DamageTaken.text = TotalDamageTaken.ToString("n2");
        EnemyKills.text = EnemiesDefeated.ToString();
        PlayerDeath.text = PlayerDeaths.ToString();

        UIPanel.SetActive(true);
    }

    public void EndLevel()
    {
        Player.enabled = false;
        Player.PlayerGun.CurrentGun.GunReticle.alpha = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        CameraShake.ShakeCamera(0, 0, 0);
        UIPanel.SetActive(false);
        LevelEndTime = Time.unscaledTime;
        BackgroundPanelAlpha = 0;
        FadeRoutineInstance = StartCoroutine(FadeRoutine());
        TogglePause(true);
        ScorePanel.SetActive(true);
    }

    IEnumerator FadeRoutine()
    {
        while (true)
        {
            BackgroundPanelAlpha = Mathf.MoveTowards(BackgroundPanelAlpha, 1f, FadeSpeed * Time.unscaledDeltaTime);
            var temp = PanelImage.color;
            temp.a = BackgroundPanelAlpha;
            PanelImage.color = temp;

            if (BackgroundPanelAlpha >= 1f) OnLevelEnd();

            yield return null;
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
        Time.timeScale = shouldPause ? 0 : 1;  
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

    public void ReloadLevel()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        TogglePause(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}



//List<IDamageable> m_RespawnBuffer;

//public List<IDamageable> RespawnBuffer { get { return m_RespawnBuffer; } set { m_RespawnBuffer = value; } }