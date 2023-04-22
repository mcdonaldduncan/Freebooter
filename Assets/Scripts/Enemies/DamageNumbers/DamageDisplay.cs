using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class DamageDisplay : MonoBehaviour, IPoolable
{
    [SerializeField] float m_Speed;
    [SerializeField] float m_Duration;
    [SerializeField] float m_ScaleSpeed;

    int PositiveNegative;
    public float m_AmplitudeY = .1f;
    public float m_SpeedY = .5f;
    Vector3 m_Startposition;


    public float m_Index;

    public bool SineWaveMovement = false;

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
        m_Index = 1f;
        StartTime = Time.time;
        if (m_TextMeshPro != null) m_TextMeshPro.fontSize = StartSize;
        m_Startposition = gameObject.transform.position;
        PositiveNegative = Random.Range(0, 2);
    }
    private void OnDisable()
    {
        gameObject.transform.position = m_Startposition; 
    }

    void Update()
    {
        
        if (SineWaveMovement == true)
        {
            //sine wave movement

            m_Index += Time.deltaTime;

            //int PositiveNegative = Random.Range(0, 2);
            float y = m_AmplitudeY * Mathf.Sin(m_SpeedY * m_Index);
            if (PositiveNegative == 1)
            {
                gameObject.transform.localPosition += new Vector3((Time.deltaTime * m_Speed), y, 0);

            }
            else if (PositiveNegative == 0)
            {
                gameObject.transform.localPosition += new Vector3(-(Time.deltaTime * m_Speed), y, 0);

            }
            //Return to pool once the Y position is lower than the starting Y position
            if (gameObject.transform.position.y < m_Startposition.y - 1)
            {
                ProjectileManager.Instance.ReturnToPool(gameObject);
            }
        }
        else
        {
            //old movement that just sent straight up
            gameObject.transform.position += Vector3.up * Time.deltaTime * m_Speed;

            //Timer for this mode of movement
            
        }

        if (Time.time > StartTime + m_Duration)
        {
            ProjectileManager.Instance.ReturnToPool(gameObject);
        }

        gameObject.transform.rotation = Camera.main.transform.rotation;
        m_TextMeshPro.fontSize += Time.deltaTime * m_ScaleSpeed;
    }
}
