using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadSpider : MonoBehaviour
{
    Vector3 lastPosition;
    bool isAttached;

    // Update is called once per frame
    void Update()
    {
        ApplyMotionToPlayer();
    }

    private void OnCollisionEnter(Collision collision)
    {
        isAttached = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        isAttached = false;
    }

    void ApplyMotionToPlayer()
    {
        if (isAttached)
        {
            Vector3 spiderTranslation = this.transform.position - lastPosition;
            LevelManager.Instance.Player.surfaceMotion += spiderTranslation;
        }
        lastPosition = this.transform.position;
    }
}
