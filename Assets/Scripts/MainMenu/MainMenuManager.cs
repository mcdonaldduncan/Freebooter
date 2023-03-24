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
    [SerializeField] float LoadThreshold;

    private void OnEnable()
    {
        LoadingPanel.SetActive(false);
        LoadingBar.fillAmount = .1f;
    }

    public void StartLevel(int index)
    {
        //SceneManager.LoadScene(index);
        MainPanel.SetActive(false);
        LoadingPanel.SetActive(true);
        StartCoroutine(LoadSceneAsync(index));
        
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
        
        AsyncOperation operation = SceneManager.LoadSceneAsync(index);
        operation.allowSceneActivation = false;
        

        while (!operation.isDone)
        {
            Debug.Log(operation.progress);
            LoadingBar.fillAmount = operation.progress;

            if (operation.progress >= LoadThreshold)
            {
                operation.allowSceneActivation = true;
            }
            yield return null;
        }
    }
    
}
