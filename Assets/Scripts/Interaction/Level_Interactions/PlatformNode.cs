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

}
