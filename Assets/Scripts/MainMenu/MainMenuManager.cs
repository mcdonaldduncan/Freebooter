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
    [SerializeField] Sprite[] Sprites;
    [SerializeField] Image Image;

    public void StartLevel(int index)
    {
        SceneManager.LoadScene(index);
    }


    public void QuitGame()
    {
        Application.Quit();
    }

    public void SetBackgroundImage(int index)
    {
        Image.sprite = Sprites[index];
    }

    
}
