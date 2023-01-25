using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPElevator : MonoBehaviour 
{
    public Animation[] TriggerAnim;

    [HideInInspector]
    public bool Enable;
    int Locked;
    bool Mode = true;

	// Use this for initialization
	void Start () 
    {
		
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (Enable)
        {
            if (Locked == 0)
            {
                if (!TriggerAnim[1].isPlaying)
                {
                    if (Mode)
                    {
                        TriggerAnim[1].Play("close");
                        TriggerAnim[2].Play("close");
                    }
                    else
                    {
                        TriggerAnim[1].Play("close");
                        TriggerAnim[3].Play("close");
                    }
                    Locked = 1;
                }
            }

            if (Locked == 1)
            {
                if (!TriggerAnim[1].isPlaying)
                {
                    if (Mode)
                    {
                        TriggerAnim[0].Play("up");
                    }
                    else
                    {
                        TriggerAnim[0].Play("down");
                    }
                    Locked = 2;
                }
            }

            if (Locked == 2)
            {
                if (!TriggerAnim[0].isPlaying)
                {
                    if (Mode)
                    {
                        TriggerAnim[1].Play("open");
                        TriggerAnim[3].Play("open");
                        Mode = false;
                    }
                    else
                    {
                        TriggerAnim[1].Play("open");
                        TriggerAnim[2].Play("open");
                        Mode = true;
                    }
                    Locked = 0;
                    Enable = false;
                }
            }
        }
	}
}
