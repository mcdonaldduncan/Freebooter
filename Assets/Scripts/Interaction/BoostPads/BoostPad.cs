using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostPad : MonoBehaviour
{
    [Header("Vertical")]
    [SerializeField] bool Vertical;
    [SerializeField] float VericalForce;
    [Header("Horizontal")]
    [SerializeField] bool Horizontal;
    [SerializeField] float SpeedIncreaseMultiplier, SpeedDuration;
    private GameObject Player;
    private FirstPersonController Controller;

    // Start is called before the first frame update
    void Start()
    {
        Player = LevelManager.Instance.Player.gameObject;
        Controller = Player.GetComponent<FirstPersonController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) { return; }

        if (Horizontal)
        {
            Controller.BoostDash(SpeedIncreaseMultiplier, SpeedDuration);
        }

        if (Vertical)
        {
            Controller.BoostJump(VericalForce);
        }
    }
}
