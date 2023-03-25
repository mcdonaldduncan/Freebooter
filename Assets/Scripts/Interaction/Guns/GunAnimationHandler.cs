using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunAnimationHandler : MonoBehaviour
{
    public Animator RecoilAnim { get { return recoilAnim; } }
    public AnimationClip RecoilAnimClip { get { return recoilAnimClip; } }

    [SerializeField] private Animator recoilAnim;
    [SerializeField] private AnimationClip recoilAnimClip;

    public void RecoilEnd()
    {
        RecoilAnim.ResetTrigger("RecoilTrigger");
    }
}
