using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UpdateHealth : MonoBehaviour
{
    private Image HealthBar;
    [SerializeField] private GameObject HealthCriticalOverlayObject;
    [SerializeField] private Sprite HealthCriticalOverlay1;
    [SerializeField] private Sprite HealthCriticalOverlay2;
    [SerializeField] private Sprite HealthCriticalOverlay3;
    [SerializeField] private float overlay1Health;
    [SerializeField] private float overlay2Health;
    [SerializeField] private float overlay3Health;
    private float CurrentHealth = 100;
    private float MaxHealth;
    private bool soundPlayed = false;
    FirstPersonController Player;
    private Image HealthCriticalOverlayImage;

    private bool subscribedToDamageEvent;

    void Start()
    {
        HealthBar = GetComponent<Image>();
        HealthCriticalOverlayImage = HealthCriticalOverlayObject.GetComponent<Image>();
        Player = LevelManager.Instance.Player;
        this.MaxHealth = Player.MaxHealth;
        CurrentHealth = Player.Health;
        HealthCriticalOverlayImage.enabled = false;
        Player.PlayerHealthChanged += CheckHealth;
        subscribedToDamageEvent = true;
    }

    private void OnEnable()
    {
        if (Player != null && !subscribedToDamageEvent)
        {
            Player.PlayerHealthChanged += CheckHealth;
        }
    }
    private void OnDisable()
    {
        if (Player != null && subscribedToDamageEvent)
        {
            Player.PlayerHealthChanged -= CheckHealth;
            subscribedToDamageEvent = false;
        }
    }

    private void CheckHealth()
    {
        CurrentHealth = Player.Health;
        HealthBar.fillAmount = CurrentHealth / MaxHealth;

        if (CurrentHealth > overlay1Health)
        {
            HealthCriticalOverlayImage.enabled = false;
            return;
        }

        if (CurrentHealth <= overlay3Health)
        {
            HealthCriticalOverlayImage.sprite = HealthCriticalOverlay3;
            HealthCriticalOverlayImage.enabled = true;
            return;
        }

        if (CurrentHealth <= overlay2Health)
        {
            HealthCriticalOverlayImage.sprite = HealthCriticalOverlay2;
            HealthCriticalOverlayImage.enabled = true;
            return;
        }

        if (CurrentHealth <= overlay1Health)
        {
            HealthCriticalOverlayImage.sprite = HealthCriticalOverlay1;
            HealthCriticalOverlayImage.enabled = true;
            return;
        }
    }

}
