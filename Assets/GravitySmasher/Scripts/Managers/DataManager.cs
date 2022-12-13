using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DataManager : MonoBehaviour
{
    // Data to be handled by the data manager
    [System.NonSerialized] public GameObject star;
    [System.NonSerialized] public Attractor[] bodies;
    //[System.NonSerialized] public Enemy[] enemies;
    [System.NonSerialized] public int enemiesDefeated;
    [System.NonSerialized] public int projectilesUsed;
    //[System.NonSerialized] public AttractionData[] attractors;

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
        //enemies = FindObjectsOfType<Enemy>();
        star = GameObject.Find("Star");
        enemiesDefeated = PlayerPrefs.GetInt("EnemiesDefeated", 0);
        projectilesUsed = PlayerPrefs.GetInt("ProjectileCount", 0);
    }

    //private void Start()
    //{
    //    // Chose not to use struct array as it would have required writing data back to struct values every frame
    //    if (bodies != null)
    //    {
    //        //attractors = bodies.Select(q => new AttractionData(q.transform.position, q.mass)).ToArray();
    //    }
    //}

    // Save persistent data to PlayerPrefs, unused for now
    /*public void SetPersistentData()
    {
        PlayerPrefs.SetInt("EnemiesDefeated", enemiesDefeated);
        PlayerPrefs.SetInt("ProjectileCount", projectilesUsed);
    }*/
    
}
