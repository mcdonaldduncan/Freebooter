using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetUpdateHealth : MonoBehaviour
{
    [SerializeField] NetworkPlayerController Player;
    private Image HealthBar;
    public float CurrentHealth = 100;
    private float MaxHealth;
    
    void Start()
    {
        HealthBar = GetComponent<Image>();
        this.MaxHealth = Player.MaxHealth;
    }

    private void Update()
    {
        CurrentHealth = Player.Health;
        HealthBar.fillAmount = CurrentHealth / MaxHealth;
    }
}
