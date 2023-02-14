using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelGroupBehavior : MonoBehaviour
{
    [SerializeField] private GameObject m_BarrelPrefab;

    public delegate void BarrelGroupDelegate();
    public event BarrelGroupDelegate fractureChildren;
    [HideInInspector]
    public bool activated = false;

    public GameObject AddBarrel()
    {
        GameObject barrel = Instantiate(m_BarrelPrefab, transform.position, Quaternion.identity, transform);
        return barrel;
    }

    public void FractureChildren()
    {
        activated = true;
        fractureChildren?.Invoke();
    }
}
