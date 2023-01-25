using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPEmissionBlink : MonoBehaviour 
{
    public float BlinkTime;
    [ColorUsageAttribute(true,true,0,10,0,1)]
    public Color Bright;
    bool Mode;

    void Awake()
    {

    }

	// Use this for initialization
	void Start () 
    {
        InvokeRepeating("EBlink", 0, BlinkTime);
	}
	
	// Update is called once per frame
	void Update () 
    {
		
	}

    void EBlink()
    {
        Mode = !Mode;
        if (Mode) GetComponent<Renderer>().materials[0].SetColor("_EmissionColor", Color.black);
        else GetComponent<Renderer>().materials[0].SetColor("_EmissionColor", Bright);
    }
}
