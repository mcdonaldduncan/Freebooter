using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald
public class Singleton<T> : MonoBehaviour where T : Component
{
    private static T _instance;

    private static bool applicationIsQuitting = false;

    public static T Instance
    {
        get
        {
            if (applicationIsQuitting)
            {
                return null;
            }

            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();




                if (_instance == null)
                {
                    GameObject obj = new GameObject();

                    obj.name = typeof(T).Name;
                    _instance = obj.AddComponent<T>();
                }
            }

            return _instance;
        }
    }

    private void OnDestroy()
    {
        applicationIsQuitting = true;
        _instance = null;
    }


    public virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
        applicationIsQuitting = false;
    }

}
