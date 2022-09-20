using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderTrigger : MonoBehaviour
{
    [SerializeField]
    private Ladder ladderScript;
    [SerializeField]
    private FirstPersonController firstPersonController;

    // Start is called before the first frame update
    void Start()
    {
        ladderScript = GameObject.FindGameObjectWithTag("Player").GetComponent<Ladder>();
        firstPersonController = GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            if (ladderScript.inside && firstPersonController.playerOnSpecialMovement)
            {
                ladderScript.inside = false;
            }
            else if (!ladderScript.inside && !firstPersonController.playerOnSpecialMovement)
            {
                ladderScript.inside = true;
            }
        }
    }
}
