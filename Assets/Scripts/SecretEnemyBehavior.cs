using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SecretEnemyBehavior : MonoBehaviour
{
    [SerializeField] float m_RotationSpeed;
    [SerializeField] float m_DistanceToKill = 1;

    FirstPersonController m_Player;
    Transform m_Target;
    NavMeshAgent m_NavAgent;
    Vector3 m_OriginalPos;
    bool m_ShouldChase;


    public bool ShouldChase { get { return m_ShouldChase; } set { m_ShouldChase = value; } }

    // Start is called before the first frame update
    void Start()
    {
        m_NavAgent = GetComponent<NavMeshAgent>();
        m_OriginalPos = transform.position;
        m_Player = LevelManager.Instance.Player;
        m_Target = m_Player.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_ShouldChase)
        {
            m_NavAgent.SetDestination(m_Target.position);
            FacePlayer();
        }
        else
        {
            m_NavAgent.SetDestination(m_OriginalPos);
        }

        var distance = Vector3.Distance(transform.position, m_Target.position);
        if (distance <= m_DistanceToKill)
        {
            m_Player.TakeDamage(m_Player.MaxHealth, HitBoxType.normal);
        }
    }

    void FacePlayer()
    {
        Vector3 lookPos = m_Target.position - transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, m_RotationSpeed * Time.deltaTime);
    }
}
