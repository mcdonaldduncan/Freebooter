using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald
public class EnemyGroupActivator : MonoBehaviour, IActivator
{
    [SerializeField] List<GameObject> m_TargetGroup;

    public event IActivator.ActivateDelegate Activate;
    public event IActivator.ActivateDelegate Deactivate;

    // Removing this beautiful bit of code bc it allocates memory :(
    //bool m_Inactive => m_TargetGroup.Where(x => x.activeSelf).Any();
    bool m_Inactive
    {
        get
        {
            foreach (var obj in m_TargetGroup)
            {
                if (obj.activeSelf) return true;
            }

            return false;
        }
    }
    // eeeew


    bool m_IsActivated;

    Coroutine m_Coroutine;

    private void OnEnable()
    {
        LevelManager.PlayerRespawn += OnRespawn;
    }

    private void OnDisable()
    {
        LevelManager.PlayerRespawn -= OnRespawn;
    }

    public void FireActivation()
    {
        Activate?.Invoke();
        m_IsActivated = true;
    }

    public void FireDeactivation()
    {
        Deactivate?.Invoke();
        m_IsActivated = false;
    }


    void Update()
    {
        if (m_IsActivated) return;

        if (!m_Inactive)
        {
            FireActivation();
        }
    }

    void OnRespawn()
    {
        StartCoroutine(SetStateNextFrame());
        
    }

    void SetState()
    {
        //m_IsActivated = !m_Inactive;
        if (m_Inactive)
        {
            FireDeactivation();
        }
    }

    IEnumerator SetStateNextFrame()
    {
        yield return null;
        SetState();
    }
}
