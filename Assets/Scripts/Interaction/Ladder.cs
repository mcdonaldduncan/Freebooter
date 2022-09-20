using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour
{
    [SerializeField]
    private CharacterController chController;
    [SerializeField]
    private Transform playerTransform;
    [SerializeField]
    private Transform groundTransform;
    
    public bool inside = false;


    [SerializeField]
    private float climbSpeed = 3.2f;

    [SerializeField]
    private FirstPersonController firstPersonController;

    // Start is called before the first frame update
    void Start()
    {
        firstPersonController = GetComponent<FirstPersonController>();
        playerTransform = GetComponent<Transform>();
        chController = GetComponent<CharacterController>();
        groundTransform = GameObject.FindGameObjectWithTag("Ground").GetComponent<Transform>();
        inside = false;
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Ladder")
        {
            firstPersonController.playerOnSpecialMovement = true;
            //inside = true;
        }
    }

    void OnTriggerStay(Collider col)
    {
        if (col.gameObject.tag == "Ladder")
        {
            if (Input.GetKey(KeyCode.Space))
            {
                firstPersonController.playerOnSpecialMovement = false;
                inside = false;
            }
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.gameObject.tag == "Ladder")
        {
            firstPersonController.playerOnSpecialMovement = false;
            //inside = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (inside && Input.GetKey(KeyCode.W))
        {
            playerTransform.transform.position += Vector3.up / climbSpeed;
        }

        if (inside && Input.GetKey(KeyCode.S))
        {
            if(playerTransform.localPosition.y > (groundTransform.localPosition.y + chController.height + chController.skinWidth))
            {
                playerTransform.transform.position += Vector3.down / climbSpeed;
            }
        }

        if (!inside)
        {
            firstPersonController.playerOnSpecialMovement = false;
        }
    }
}
