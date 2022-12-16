using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UpdateHealth : MonoBehaviour
{
    private Image HealthBar;
    public Image HealthCriticalOverlay;
    public float CurrentHealth = 100;
    private float MaxHealth;
    private bool soundPlayed = false;
    FirstPersonController Player;

    void Start()
    {
        HealthBar = GetComponent<Image>();
        Player = FindObjectOfType<FirstPersonController>();
        this.MaxHealth = Player.MaxHealth;
        HealthCriticalOverlay.enabled = false;
    }

    private void Update()
    {
        CurrentHealth = Player.Health;
        HealthBar.fillAmount = CurrentHealth / MaxHealth;
        if(CurrentHealth <= MaxHealth * .30 && !HealthCriticalOverlay.enabled)
        {
            HealthCriticalOverlay.enabled = true;
            if (!soundPlayed)
            {
                Player.PlayerAudioSource.PlayOneShot(Player.LowHealthAudio);
                soundPlayed = true;
                Debug.Log("Sound Played");
            }

        }
        else if (CurrentHealth > MaxHealth * .30 && HealthCriticalOverlay.enabled)
        {
            HealthCriticalOverlay.enabled = false;
            soundPlayed = false;
        }
    }

}
