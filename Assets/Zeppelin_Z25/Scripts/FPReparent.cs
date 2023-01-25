using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPReparent : MonoBehaviour 
{
	// Use this for initialization
	void Start () 
    {
		
	}
	
	// Update is called once per frame
	void Update () 
    {
		
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<CharacterController>())
        {
            other.gameObject.transform.parent = this.transform;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<CharacterController>())
        {
            other.gameObject.transform.parent = null;
        }
    }
}
