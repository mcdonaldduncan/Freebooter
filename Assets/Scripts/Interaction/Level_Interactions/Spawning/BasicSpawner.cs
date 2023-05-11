using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald
public class BasicSpawner : MonoBehaviour, IRecipient, IRespawn
{
    [SerializeField] GameObject m_SpawnVFX;
    [SerializeField] GameObject m_Activator;
    [SerializeField] public bool m_UseChildren;
    [SerializeField] bool m_CheckpointDependent = true;
    [SerializeField] List<GameObject> m_SpawnObjects;
    private bool m_ShouldActivate;

    public GameObject ActivatorObject => m_Activator;

    public IActivator Activator { get; set; }
    public IRecipient Recipient { get; set; }
    public IRespawn Respawn { get; set; }

    void Start()
    {
        Recipient = this;
        Respawn = this;
        m_ShouldActivate = true;
        Recipient.ActivatorSetUp();
        Respawn.SubscribeToRespawn();
        CollectAndPrepareChildren();
        StartCoroutine(SetStateNextFrame());
    }

    void CollectAndPrepareChildren()
    {
        if (!m_UseChildren) return;

        foreach (Transform child in transform)
        {
            m_SpawnObjects.Add(child.gameObject);
        }

        //transform.DetachChildren();
    }

    private void OnDisable()
    {
        Recipient.Unsubscribe();
    }

    public void OnActivate()
    {
        if (!m_ShouldActivate) return;
        foreach (var obj in m_SpawnObjects)
        {
            _ = ProjectileManager.Instance.TakeFromPool(m_SpawnVFX, obj.transform.position);
            obj.SetActive(true);
        }
        beenActivatedOnce = true;
        Respawn.SubscribeToCheckpointReached();
    }

    void PrepareGroupForSpawn()
    {
        if (beenActivatedOnce && shouldRemainActive && m_ShouldActivate) return;
        foreach (var obj in m_SpawnObjects)
        {
            obj.SetActive(false);
        }
    }

    public void OnCheckPointReached()
    {
        LevelManager.Instance.PlayerRespawn -= OnPlayerRespawn;
        if (m_CheckpointDependent) m_ShouldActivate = false;
    }

    public void OnPlayerRespawn()
    {
        StartCoroutine(SetStateNextFrame());
    }

    [SerializeField] bool shouldRemainActive = false;
    bool beenActivatedOnce = false;
    // Joseph: Because the set state next frame code happens at the same frame for both the spawner and the enemy group activator,
    // the enemy group activator fires first and then the spawner which causes enemies that should be active to be deactivated.
    // My messy temporary solution is to add lines 60, 66, 84, and 85, feel free to fix this problem in a better way later.
    IEnumerator SetStateNextFrame()
    {
        yield return null;
        PrepareGroupForSpawn();
    }
}
