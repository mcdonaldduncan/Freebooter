using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class DamageDisplay : MonoBehaviour, IPoolable
{
    [SerializeField] float m_Speed;
    [SerializeField] float m_Duration;
    [SerializeField] float m_ScaleSpeed;

    [SerializeField] public GameObject Prefab { get;  set; }
    
    TextMeshPro m_TextMeshPro;

    float StartTime;

    float StartSize;

    private void Start()
    {
        m_TextMeshPro = gameObject.GetComponent<TextMeshPro>();
        StartSize = m_TextMeshPro.fontSize;
    }

    private void OnEnable()
    {
        StartTime = Time.time;
        if (m_TextMeshPro != null) m_TextMeshPro.fontSize = StartSize;
    }

    void Update()
    {
        if (Time.time > StartTime + m_Duration)
        {
            ProjectileManager.Instance.ReturnToPool(gameObject);
        }

        gameObject.transform.position += Vector3.up * Time.deltaTime * m_Speed;
        transform.rotation = Camera.main.transform.rotation;
        m_TextMeshPro.fontSize += Time.deltaTime * m_ScaleSpeed;
    }
}
