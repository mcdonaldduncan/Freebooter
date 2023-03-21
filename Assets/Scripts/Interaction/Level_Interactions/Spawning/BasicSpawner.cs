using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald
public class BasicSpawner : MonoBehaviour, IRecipient, IRespawn
{
    [SerializeField] GameObject m_Activator;
    [SerializeField] public bool m_UseChildren;
    [SerializeField] List<GameObject> m_SpawnObjects;
    

    public GameObject ActivatorObject => m_Activator;

    public IActivator Activator { get; set; }
    public IRecipient Recipient { get; set; }
    public IRespawn Respawn { get; set; }

    void Start()
    {
        Recipient = this;
        Respawn = this;
        Recipient.ActivatorSetUp();
        Respawn.SubscribeToRespawn();
        CollectAndPrepareChildren();
        PrepareGroupForSpawn();
    }

    void CollectAndPrepareChildren()
    {
        if (!m_UseChildren) return;

        foreach (GameObject child in transform)
        {
            m_SpawnObjects.Add(child);
        }

        transform.DetachChildren();
    }

    private void OnDisable()
    {
        Recipient.Unsubscribe();
    }

    public void OnActivate()
    {
        foreach (var obj in m_SpawnObjects)
        {
            obj.SetActive(true);
        }
        Respawn.SubscribeToCheckpointReached();
    }

    void PrepareGroupForSpawn()
    {
        foreach (var obj in m_SpawnObjects)
        {
            obj.SetActive(false);
        }
    }

    public void OnPlayerRespawn()
    {
        StartCoroutine(SetStateNextFrame());
    }

    IEnumerator SetStateNextFrame()
    {
        yield return null;
        PrepareGroupForSpawn();
    }
}
