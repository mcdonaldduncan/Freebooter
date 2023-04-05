using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDissolveController : MonoBehaviour
{
    [SerializeField] private GameObject m_parentGameObj;
    [SerializeField] private float m_timeBeforeDespawn = 3;
    [SerializeField] private float m_dissolveTime = 0.025f;
    [SerializeField] private float m_dissolveRate = 0.0125f;

    private SkinnedMeshRenderer m_skinnedMesh;
    private Material m_skinnedMaterial;
    private IDissolvable m_enemyScript;

    private float m_dissolveCounter;
    private float m_dissolveRefreshStartTime;
    private float m_dissolveAmountStart;
    private float m_timeDefeated;

    private bool m_enemyDied = false;

    private bool ShouldDissolve => m_skinnedMaterial != null && m_skinnedMaterial.GetFloat("_DissolveAmount") < 1 && m_enemyDied 
        && m_timeDefeated + m_timeBeforeDespawn <= Time.time;
    private bool CompletelyDissolved => m_skinnedMaterial.GetFloat("_DissolveAmount") >= 1;

    // Start is called before the first frame update
    void Start()
    {
        m_skinnedMesh = GetComponent<SkinnedMeshRenderer>();
        m_enemyScript = GetComponentInParent<IDissolvable>();

        if (m_skinnedMesh != null) m_skinnedMaterial = m_skinnedMesh.materials[0];

        m_dissolveAmountStart = m_skinnedMaterial.GetFloat("_DissolveAmount");

        LevelManager.PlayerRespawn += OnPlayerRespawn;
        m_enemyScript.EnemyDied += OnEnemyDeath;
    }

    // Update is called once per frame
    void Update()
    {
        if (ShouldDissolve)
        {
            Dissolve();
        }
    }

    private void Dissolve()
    {
        if (m_dissolveRefreshStartTime + m_dissolveTime < Time.time)
        {
            m_dissolveCounter += m_dissolveRate;
            m_skinnedMaterial.SetFloat("_DissolveAmount", m_dissolveCounter);

            m_dissolveRefreshStartTime = Time.time;
        }

        if (CompletelyDissolved && m_parentGameObj.activeSelf)
        {
            m_parentGameObj.SetActive(false);
        }
    }

    private void OnEnemyDeath()
    {
        m_dissolveCounter = 0;
        m_dissolveRefreshStartTime = Time.time;
        m_enemyDied = true;
        m_timeDefeated = Time.time;
    }

    private void OnPlayerRespawn()
    {
        m_enemyDied = false;
        m_skinnedMaterial.SetFloat("_DissolveAmount", m_dissolveAmountStart);
    }
}
