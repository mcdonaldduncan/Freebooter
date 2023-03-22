using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DataManager : MonoBehaviour
{
    // Data to be handled by the data manager
    [System.NonSerialized] public GameObject star;
    [System.NonSerialized] public Attractor[] bodies;

    public static DataManager instance { get; private set; }

    // Singleton set up
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    // Cache relevant objects and assign saved or default values
    void OnEnable()
    {
        bodies = FindObjectsOfType<Attractor>();
        star = GameObject.Find("Star");
    }

}
