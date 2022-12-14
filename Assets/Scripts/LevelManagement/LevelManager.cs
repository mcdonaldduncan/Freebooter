using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{
    [SerializeField] public FirstPersonController Player;
    [SerializeField] public GameObject CheckPointPrefab;

    [SerializeField] List<CheckPoint> m_CheckPoints;

    CheckPoint m_CurrentCheckPoint;
    List<IDamageable> m_RespawnBuffer;

    public List<IDamageable> RespawnBuffer { get { return m_RespawnBuffer; } set { m_RespawnBuffer = value; } }
    public CheckPoint CurrentCheckPoint { get { return m_CurrentCheckPoint; } set { m_CurrentCheckPoint = value; } }


    public delegate void PlayerRespawnDelegate();
    public PlayerRespawnDelegate PlayerRespawn;
    public PlayerRespawnDelegate CheckPointReached;

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

    void SetIndices()
    {
        for (int i = 0; i < m_CheckPoints.Count; i++)
        {
            m_CheckPoints[i].m_Index = i;
        }
    }

}
