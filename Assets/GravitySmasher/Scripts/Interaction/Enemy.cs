using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utility;

public class Enemy : MonoBehaviour
{
    [SerializeField] ParticleSystem explosion;

    UIManager uiManager;
    LevelManager levelManager;

    // Assign level and uiManager
    private void Start()
    {
        uiManager = AssignUIManager();
        levelManager = AssignLevelManager();
    }

    // On trigger enter, destroy gameobject, increment score and call UpdateScore()
    private void OnTriggerEnter(Collider other)
    {
        explosion.Play();
        Destroy(gameObject);
        DataManager.instance.enemiesDefeated++;
        uiManager.UpdateScore();
        levelManager.CheckAdvance();
    }

}
