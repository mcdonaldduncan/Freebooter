using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] Text scoreText;

    //DataManager dataManager;

    // Assign data manager and update score reloadText on start
    void Start()
    {
        //dataManager = Utility.AssignDataManager();
        UpdateScore();
    }

    // Update the score reloadText
    public void UpdateScore()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Projectiles Used: {DataManager.instance.projectilesUsed}\nEnemies Destroyed: {DataManager.instance.enemiesDefeated}";
        }
    }

}
