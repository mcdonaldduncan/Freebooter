using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlatformNode : MonoBehaviour
{
    MovingPlatform m_MovingPlatform;

    public void Init(MovingPlatform platform)
    {
        m_MovingPlatform = platform;
    }

    //private void OnDestroy()
    //{
    //    if (m_MovingPlatform.m_Nodes.Contains(transform))
    //    {
    //        m_MovingPlatform.m_Nodes.Remove(transform);
    //    }
    //}


    /*
    public Vector3 position => transform.position;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        

        Gizmos.DrawWireCube(transform.position, Vector3.one);
    }
#endif

    */
}
