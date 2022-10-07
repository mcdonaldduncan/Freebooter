using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] int nextLevel;

    WaitForSeconds delay = new WaitForSeconds(2);

    bool isLoading;

    private void Start()
    {
        isLoading = false;
    }

    // Check to see if enemies have been defeated
    public void CheckAdvance()
    {
        if (isLoading)
            return;

        if (DataManager.instance.enemiesDefeated >= DataManager.instance.enemies.Length)
        {
            StartCoroutine(AdvanceLevelAfterDelay());
            isLoading = true;
        }
    }

    

    // Move to next level after delay
    IEnumerator AdvanceLevelAfterDelay()
    {
        isLoading = true;
        yield return delay;
        //DataManager.instance.SetPersistentData();
        SceneManager.LoadScene(nextLevel);
    }

    // Reset current level
    public void ResetLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void StartGame()
    {
        SceneManager.LoadScene(nextLevel);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
