using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BossInquisitor : MonoBehaviour
{
    [SerializeField] private Animator m_Animator;
    [SerializeField] private float timetostart;
    [SerializeField] private float timebetween;

    float lasttime;

    [SerializeField] List<Transform> followerSpawns;

    Follower follower;

    private void OnEnable()
    {
        //potentialTargets = FindObjectsOfType<FirstPersonController>().Select(item => item.transform).ToList();
    }


    // Start is called before the first frame update
    void Start()
    {
        //Initialize();
    }

    public void ReactOver()
    {
        m_Animator.SetBool("isReacting", false);
    }

    public void AttackOver()
    {
        m_Animator.SetBool("isAttacking", false);
    }

    public void MaintainAttack()
    {
        m_Animator.SetFloat("SpeedMult", 0);
        Invoke(nameof(EndAttack), 5);
    }

    private void EndAttack()
    {
        m_Animator.SetFloat("SpeedMult", 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time < timetostart) return;
        m_Animator.SetBool("Phase2", true);

        if (lasttime + timebetween < Time.time)
        {
            m_Animator.SetBool("isAttacking", true);
            timebetween = Time.time;
        }

    }

}
