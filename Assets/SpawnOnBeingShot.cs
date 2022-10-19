using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnOnBeingShot : MonoBehaviour, IDamageable
{
    public GameObject spawnThisEnemy;
    public Transform spawnHere;
    public GameObject spawnedEnemy;
    private float health;

    public float Health { get { return health; } set { health = value; } }

    // Start is called before the first frame update
    void Start()
    {
        spawnHere = GameObject.Find("spawnHere").transform;
    }
    public void TakeDamage(float damageTaken)
    {
        if (spawnedEnemy == null)
        {
            spawnedEnemy = Instantiate(spawnThisEnemy, spawnHere.position, spawnHere.rotation);
        }
    }
    public void CheckForDeath()
    {
        //do nothing
    }
}
