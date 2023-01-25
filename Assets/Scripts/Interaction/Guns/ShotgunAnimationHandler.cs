using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunAnimationHandler : MonoBehaviour
{
    public Animator RecoilAnim { get { return recoilAnim; } }
    public AnimationClip RecoilAnimClip { get { return recoilAnimClip; } }

    [SerializeField] private Animator recoilAnim;
    [SerializeField] private AnimationClip recoilAnimClip;
}
