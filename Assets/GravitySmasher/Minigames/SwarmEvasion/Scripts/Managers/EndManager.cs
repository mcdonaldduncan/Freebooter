using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndManager : MonoBehaviour
{
    [System.NonSerialized] public bool gameOver;

    public static EndManager instance { get; private set; }

    void Start()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        instance.gameOver = false;

    }

    void Update()
    {
        //if(gameOver)
        //{
        //    Time.timeScale = 0;
        //}
    }
}
