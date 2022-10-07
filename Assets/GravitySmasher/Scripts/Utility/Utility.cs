using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility : MonoBehaviour
{
    // Utility function for assigning the DataManager
    public static DataManager AssignDataManager()
    {
        return GameObject.Find("DataManager").GetComponent<DataManager>();
    }

    // Utility function for assigning the UIManager
    public static UIManager AssignUIManager()
    {
        return GameObject.Find("UIManager").GetComponent<UIManager>();
    }

    // Utility function for assigning the LevelManager
    public static LevelManager AssignLevelManager()
    {
        return GameObject.Find("LevelManager").GetComponent<LevelManager>();
    }

    // Utility method for finding maximum and minimum screen bounds
    public static Vector2 FindWindowLimits()
    {
        return Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
    }
}
