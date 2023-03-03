using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manager for projectile and all projectile related gameobjects
/// Custom pooling to cut down memory consumption
/// </summary>
/// Author: Duncan McDonald
public class ProjectileManager : Singleton<ProjectileManager>
{
    private Dictionary<GameObject, Queue<GameObject>> m_Pool;


    void OnEnable()
    {
        //if (m_Pool != null) m_Pool.Clear();
        m_Pool = new Dictionary<GameObject, Queue<GameObject>>();
    }

    public GameObject TakeFromPool(GameObject prefab, Vector3 startLocation)
    {
        if (!m_Pool.ContainsKey(prefab))
        {
            m_Pool.Add(prefab, new Queue<GameObject>());
        }

        if (m_Pool[prefab].Count > 0)
        {
            GameObject obj = m_Pool[prefab].Dequeue();
            if (obj == null) return TakeFromPool(prefab, startLocation);
            obj.transform.position = startLocation;
            obj.SetActive(true);
            return obj;
        }
        else
        {
            GameObject obj = Instantiate(prefab, startLocation, Quaternion.identity);
            IPoolable poolable = obj.GetComponent<IPoolable>();
            if (poolable == null)
            {
                Debug.LogError("Prefab " + prefab.name + " is not poolable and cannot be used.");
                return null;
            }
            poolable.Prefab = prefab;
            return obj;
        }
    }

    //Override that allows for manual startRotation
    public GameObject TakeFromPool(GameObject prefab, Vector3 startLocation, Quaternion startRotation)
    {
        if (!m_Pool.ContainsKey(prefab))
        {
            m_Pool.Add(prefab, new Queue<GameObject>());
        }

        if (m_Pool[prefab].Count > 0)
        {
            GameObject obj = m_Pool[prefab].Dequeue();
            if (obj == null) return TakeFromPool(prefab, startLocation, startRotation);
            obj.transform.position = startLocation;
            obj.transform.rotation = startRotation;
            obj.SetActive(true);
            return obj;
        }
        else
        {
            GameObject obj = Instantiate(prefab, startLocation, startRotation);
            IPoolable poolable = obj.GetComponent<IPoolable>();
            if (poolable == null)
            {
                Debug.LogError("Prefab " + prefab.name + " is not poolable and cannot be used.");
                return null;
            }
            poolable.Prefab = prefab;
            return obj;
        }
    }

    public GameObject TakeFromPool(GameObject prefab, Vector3 startLocation, out Projectile poolable)
    {
        if (!m_Pool.ContainsKey(prefab))
        {
            m_Pool.Add(prefab, new Queue<GameObject>());
        }

        if (m_Pool[prefab].Count > 0)
        {
            GameObject obj = m_Pool[prefab].Dequeue();
            if (obj == null) return TakeFromPool(prefab, startLocation, out poolable);
            poolable = obj.GetComponent<Projectile>();
            obj.transform.position = startLocation;
            obj.SetActive(true);
            return obj;
        }
        else
        {
            GameObject obj = Instantiate(prefab, startLocation, Quaternion.identity);
            poolable = obj.GetComponent<Projectile>();
            if (poolable == null)
            {
                Debug.LogError("Prefab " + prefab.name + " is not poolable and cannot be used.");
                return null;
            }
            poolable.Prefab = prefab;
            return obj;
        }
    }

    public GameObject TakeFromPool(GameObject prefab, Vector3 startLocation, out BulletTrail poolable)
    {
        if (!m_Pool.ContainsKey(prefab))
        {
            m_Pool.Add(prefab, new Queue<GameObject>());
        }

        if (m_Pool[prefab].Count > 0)
        {
            GameObject obj = m_Pool[prefab].Dequeue();
            if (obj == null) return TakeFromPool(prefab, startLocation, out poolable);
            poolable = obj.GetComponent<BulletTrail>();
            obj.transform.position = startLocation;
            obj.SetActive(true);
            return obj;
        }
        else
        {
            GameObject obj = Instantiate(prefab, startLocation, Quaternion.identity);
            poolable = obj.GetComponent<BulletTrail>();
            if (poolable == null)
            {
                Debug.LogError("Prefab " + prefab.name + " is not poolable and cannot be used.");
                return null;
            }
            poolable.Prefab = prefab;
            return obj;
        }
    }
    public GameObject TakeFromPool(GameObject prefab, Vector3 startLocation, out GrenadeBehavior poolable)
    {
        if (!m_Pool.ContainsKey(prefab))
        {
            m_Pool.Add(prefab, new Queue<GameObject>());
        }

        if (m_Pool[prefab].Count > 0)
        {
            GameObject obj = m_Pool[prefab].Dequeue();
            if (obj == null) return TakeFromPool(prefab, startLocation, out poolable);
            poolable = obj.GetComponent<GrenadeBehavior>();
            obj.transform.position = startLocation;
            obj.SetActive(true);
            return obj;
        }
        else
        {
            GameObject obj = Instantiate(prefab, startLocation, Quaternion.identity);
            poolable = obj.GetComponent<GrenadeBehavior>();
            if (poolable == null)
            {
                Debug.LogError("Prefab " + prefab.name + " is not poolable and cannot be used.");
                return null;
            }
            poolable.Prefab = prefab;
            return obj;
        }
    }

    public void ReturnToPool(GameObject obj)
    {
        IPoolable poolItem = obj.GetComponent<IPoolable>();
        if (poolItem == null)
        {
            Debug.LogError("Object " + obj.name + " was not created from the object pool and cannot be returned.");
            return;
        }
        obj.SetActive(false);
        m_Pool[poolItem.Prefab].Enqueue(obj);
    }
}
