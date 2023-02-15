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
    
    public CheckPoint CurrentCheckPoint { get { return m_CurrentCheckPoint; } set { m_CurrentCheckPoint = value; } }

    public static LevelManager Instance { get; private set; }

    public delegate void PlayerRespawnDelegate();
    public static event PlayerRespawnDelegate PlayerRespawn;
    public static event PlayerRespawnDelegate CheckPointReached;

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
    }

    public void FirePlayerRespawn()
    {
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