using Assets.Scripts.Enemies.Agent_Base.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald
public class EnemyGroupActivator : MonoBehaviour, IActivator
{
    [SerializeField] List<GameObject> m_TargetGroup;
    
    List<IGroupable> m_Group;

    public event IActivator.ActivateDelegate Activate;
    public event IActivator.ActivateDelegate Deactivate;

    // Removing this beautiful bit of code bc it allocates memory :(
    //bool m_Inactive => m_TargetGroup.Any(x => x.activeSelf);
    bool m_AnyMemberActive
    {
        get
        {
            foreach (var obj in m_Group)
            {
                if (!obj.IsDead) return true;
            }

            return false;
        }
    }
    // why is ugly code always better performance!!

    private void Start()
    {
        LevelManager.Instance.PlayerRespawn += OnRespawn;

        m_Group = new List<IGroupable>();
        foreach (var target in m_TargetGroup)
        {
            if (target.TryGetComponent(out IGroupable groupable))
            {
                m_Group.Add(groupable);
            }
            else
            {
                Debug.LogWarning($"Invalid object in group, object: {target.name}");
            }
        }
    }

    bool m_IsActivated = false;

    Coroutine m_Coroutine;

    private void OnEnable()
    {
        
    }

    

    private void OnDisable()
    {
        //LevelManager.Instance.PlayerRespawn -= OnRespawn;
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

        if (!m_AnyMemberActive)
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
        m_IsActivated = !m_AnyMemberActive;

        if (m_AnyMemberActive)
        {
            FireDeactivation();
        }
        else
        {
            FireActivation();
        }
    }

    IEnumerator SetStateNextFrame()
    {
        yield return null;
        SetState();
    }
}
