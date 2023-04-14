using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald
public class MainMenuManager : MonoBehaviour
{
    [Header("Level Images")]
    [SerializeField] Sprite[] Sprites;
    [SerializeField] Image Image;

    [Header("Panel GameObjects")]
    [SerializeField] GameObject MainPanel;
    [SerializeField] GameObject LoadingPanel;

    [Header("Loading Options")]
    [SerializeField] Image LoadingBar;
    [SerializeField] GameObject LaunchButton;
    [SerializeField] float LoadThreshold;

    AsyncOperation operation;

    private void OnEnable()
    {
        LoadingPanel.SetActive(false);
        LaunchButton.SetActive(false);
        LoadingBar.fillAmount = .25f;
    }

    public void StartLevel(int index)
    {
        //SceneManager.LoadScene(index);
        MainPanel.SetActive(false);
        LoadingPanel.SetActive(true);
        
        StartCoroutine(LoadSceneAsync(index));
    }

    public void Launch()
    {
        operation.allowSceneActivation = true;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SetBackgroundImage(int index)
    {
        Image.sprite = Sprites[index];
    }


    IEnumerator LoadSceneAsync(int index)
    {
        operation = SceneManager.LoadSceneAsync(index);
        operation.allowSceneActivation = false;
        

        while (!operation.isDone)
        {
            LoadingBar.fillAmount = operation.progress + .1f;

            if (operation.progress >= LoadThreshold)
            {
                LaunchButton.SetActive(true);
            }
            yield return null;
        }
    }
    
}
