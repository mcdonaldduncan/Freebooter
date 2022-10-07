using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayManager : MonoBehaviour
{
    [SerializeField] Text scoreText;

    SwarmSpawner swarmSpawner;

    void Start()
    {
        swarmSpawner = GameObject.Find("Spawner").GetComponent<SwarmSpawner>();
    }

    void Update()
    {
        if (EndManager.instance.gameOver)
        {
            scoreText.text = $"Game Over, it took {swarmSpawner.totalSwarmers} swarmers to get you!";
        }
        else
        {
            scoreText.text = $"{swarmSpawner.totalSwarmers} swarmers are after you!";
        }
    }
}
