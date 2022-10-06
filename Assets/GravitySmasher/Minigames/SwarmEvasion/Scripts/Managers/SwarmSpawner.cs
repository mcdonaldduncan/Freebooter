using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utility;

public class SwarmSpawner : MonoBehaviour
{
    [SerializeField] GameObject swarmer;

    [SerializeField] float baseDelay;

    [System.NonSerialized] public int totalSwarmers;

    float timeBetweenSpawn;
    float spawnTime;
    
    Vector2 windowLimits;

    void Start()
    {
        spawnTime = 0.0f;
        totalSwarmers = 0;
        timeBetweenSpawn = baseDelay;
        windowLimits = FindWindowLimits();
    }

    void Update()
    {
        if (EndManager.instance.gameOver)
            return;

        SpawnTimer();
    }

    void Spawn()
    {
        Vector2 startPosition = new Vector2(Random.Range(-windowLimits.x, windowLimits.x), Random.Range(-windowLimits.y, windowLimits.y));
        GameObject newSwarmer = Instantiate(swarmer);
        newSwarmer.transform.position = startPosition;
        spawnTime = Time.time;
        totalSwarmers++;
    }

    void SpawnTimer()
    {
        if (Time.time - spawnTime > timeBetweenSpawn)
        {
            Spawn();
        }
    }

    

    
}
