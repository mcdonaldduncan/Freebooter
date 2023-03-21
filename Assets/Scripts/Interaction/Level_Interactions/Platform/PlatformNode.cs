using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald
public class PlatformNode : MonoBehaviour
{
    MovingPlatform m_MovingPlatform;

    public void Init(MovingPlatform platform)
    {
        m_MovingPlatform = platform;
    }

}
