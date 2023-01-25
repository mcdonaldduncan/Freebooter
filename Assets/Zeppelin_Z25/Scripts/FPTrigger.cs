using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPTrigger : MonoBehaviour 
{
    public FPElevator _FPElevator;
    public Animation[] TriggerAnim;
    public string Animation1;
    public string Animation2;

    public enum FMode
    {
        simple = 0,
        elevator = 1,
    }
    public FMode _FMode = FMode.simple;

    [HideInInspector]
    public bool tStart;
    bool Mode;

	// Use this for initialization
	void Start () 
    {
		
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (tStart)
        {
            if (_FMode == FMode.simple)
            {
                if (!TriggerAnim[0].isPlaying)
                {
                    if (!Mode)
                    {
                        if (!string.IsNullOrEmpty(Animation1))
                        {
                            foreach (Animation anim in TriggerAnim)
                            {
                                if (anim) anim.Play(Animation1);
                            }
                        }
                        Mode = true;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(Animation2))
                        {
                            foreach (Animation anim in TriggerAnim)
                            {
                                if (anim) anim.Play(Animation2);
                            }
                        }
                        Mode = false;
                    }
                }
            }
            else
            {
                _FPElevator.Enable = true;
            }
        }
        tStart = false;
	}
}
