using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmerDissolveController : MonoBehaviour
{
    [SerializeField] private float dissolveTime = 0.025f;
    [SerializeField] private float dissolveRate = 0.0125f;

    private SkinnedMeshRenderer skinnedMesh;
    private Material skinnedMaterial;
    private EnemySwarmerBehavior swarmerScript;

    private float dissolveCounter;
    private float dissolveRefreshStartTime;
    private float dissolveAmountStart;
    
    private bool swarmerDied = false;

    private bool ShouldDissolve => skinnedMaterial != null && skinnedMaterial.GetFloat("_DissolveAmount") < 1 && swarmerDied;
    private bool CompletelyDissolved => skinnedMaterial.GetFloat("_DissolveAmount") >= 1;

    // Start is called before the first frame update
    void Start()
    {
        skinnedMesh = GetComponent<SkinnedMeshRenderer>();
        swarmerScript = GetComponentInParent<EnemySwarmerBehavior>();

        if (skinnedMesh != null) skinnedMaterial = skinnedMesh.materials[0];

        dissolveAmountStart = skinnedMaterial.GetFloat("_DissolveAmount");

        LevelManager.PlayerRespawn += OnPlayerRespawn;
        swarmerScript.SwarmerDeath += OnSwarmerDeath;
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
        if (dissolveRefreshStartTime + dissolveTime < Time.time)
        {
            dissolveCounter += dissolveRate;
            skinnedMaterial.SetFloat("_DissolveAmount", dissolveCounter);

            dissolveRefreshStartTime = Time.time;
        }

        if (CompletelyDissolved && swarmerScript.gameObject.activeSelf)
        {
            swarmerScript.gameObject.SetActive(false);
        }
    }

    private void OnSwarmerDeath()
    {
        dissolveCounter = 0;
        dissolveRefreshStartTime = Time.time;
        swarmerDied = true;
    }

    private void OnPlayerRespawn()
    {
        swarmerDied = false;
        skinnedMaterial.SetFloat("_DissolveAmount", dissolveAmountStart);
    }
}
