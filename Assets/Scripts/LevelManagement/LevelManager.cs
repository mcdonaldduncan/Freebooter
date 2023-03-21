using System.Collections.Generic;
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

    private int CombatantCount;

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
        if (Player == null)
        {
            Player = GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonController>();
        }

        timeStopped = false;
        CombatantCount = 0;

        var enemies = FindObjectsOfType<NewAgentBase>();

        foreach (var enemy in enemies)
        {
            enemy.CombatStateChanged += OnCombatStateChanged;
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

    private void OnCombatStateChanged(bool combatState)
    {
        CombatantCount = combatState ? ++CombatantCount : --CombatantCount;
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