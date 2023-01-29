using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : Singleton<ProjectileManager>
{
    private Dictionary<GameObject, Queue<GameObject>> m_Pool;

    void Start()
    {
        m_Pool = new Dictionary<GameObject, Queue<GameObject>>();
    }

    public GameObject GetObject(GameObject prefab)
    {
        if (!m_Pool.ContainsKey(prefab))
        {
            m_Pool.Add(prefab, new Queue<GameObject>());
        }

        if (m_Pool[prefab].Count > 0)
        {
            GameObject obj = m_Pool[prefab].Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            GameObject obj = Instantiate(prefab);
            Projectile poolItem = obj.GetComponent<Projectile>();
            if (poolItem == null)
            {
                Debug.LogError("Object " + obj.name + " was not created from the object pool and cannot be returned.");
                return null;
            }
            //obj.AddComponent<Projectile>().prefab = prefab;
            return obj;
        }
    }

    public void ReturnObject(GameObject obj)
    {
        Projectile poolItem = obj.GetComponent<Projectile>();
        if (poolItem == null)
        {
            Debug.LogError("Object " + obj.name + " was not created from the object pool and cannot be returned.");
            return;
        }
        obj.SetActive(false);
        m_Pool[poolItem.Prefab].Enqueue(obj);
    }
}
