using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPCaster : MonoBehaviour 
{
    [Header("'Water' - one of built in Unity layers:")]
    public string InteractLayer;
    [Space(10)]
    public GameObject Canvas;
    public float RayLength = 1;
    int buttonsMask;

    void Awake()
    {
        //interactable layer:
        buttonsMask = LayerMask.GetMask(InteractLayer);
        Canvas.SetActive(false);
    }

	void Start () 
    {
		
	}
	
	void Update () 
    {
        RaycastHit hit;
        if (Physics.Linecast(transform.position, transform.TransformPoint(new Vector3(0, 0, RayLength)), out hit, buttonsMask))
        {
            Canvas.SetActive(true);
            if (Input.GetMouseButtonUp(0))
            {
                hit.collider.gameObject.GetComponent<FPTrigger>().tStart = true;
            }
        }
        else
        {
            Canvas.SetActive(false);
        }
	}

    void FixedUpdate()
    {

    }
}
