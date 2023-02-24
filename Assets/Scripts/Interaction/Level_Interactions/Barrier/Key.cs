using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald
public class Key : MonoBehaviour
{
    [SerializeField] float rotationx;
    [SerializeField] float rotationy;
    [SerializeField] float rotationz;

    Transform m_Transform;
    private FirstPersonController player;

    bool shouldRotate => (rotationx + rotationy + rotationz) > 0;


    void Start()
    {
        player = LevelManager.Instance.Player;
        m_Transform = transform;
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (!shouldRotate) return;
        m_Transform.Rotate(rotationx, rotationy, rotationz);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            KeyManager.Instance.KeyInventory.Add(this);
            player.PlayerAudioSource.PlayOneShot(player.KeyPickupAudio);
            gameObject.SetActive(false);
        }
    }
}
