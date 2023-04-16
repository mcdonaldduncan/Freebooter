using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald
public class BarrierAlertManager : Singleton<BarrierAlertManager>
{
    [Header("Alert Panel")]
    [SerializeField] GameObject m_BarrierAlertPanel;

    [Header("Display Uptime")]
    [SerializeField] float m_DisplayTime;

    [Header("Key Alert Content")]
    [SerializeField] string m_KeyAlertContent;
    [SerializeField] bool m_PrependName;

    [Header("Barrier Alert Content")]
    [SerializeField] string m_BarrierAlertContent;

    [Header("Ammo Alert Content")]
    [SerializeField] string m_AmmoAlertContent;
    [SerializeField] string m_AmmoPickupAlertContent;

    TextMeshProUGUI BarrierAlertText;

    GunHandler m_GunHandler;

    Key[] Keys;
    Barrier[] Barriers;

    float LastMessageTime;

    bool IsActive;

    /// <summary>
    /// Returns true if time has expired for message display time
    /// </summary>
    bool CounterExpired => Time.time > LastMessageTime + m_DisplayTime;


    void Start()
    {
        BarrierAlertText = m_BarrierAlertPanel.GetComponentInChildren<TextMeshProUGUI>();
        m_BarrierAlertPanel.SetActive(false);

        IsActive = false;

        // Find all Keys and subscribe to KeyCollected event
        Keys = FindObjectsOfType<Key>();
        foreach (var key in Keys)
        {
            key.KeyCollected += OnKeyCollected;
        }

        // Find all Barriers and subscribe to LockedBarrierAccessed event
        Barriers = FindObjectsOfType<Barrier>();
        foreach (var barrier in Barriers)
        {
            barrier.LockedBarrierAccessed += OnBarrierAccessed;
        }

        m_GunHandler = LevelManager.Instance.Player.GetComponentInChildren<GunHandler>();

        m_GunHandler.AmmoEmpty += OnAmmoEmpty;
        m_GunHandler.AmmoPickup += OnAmmoPickup;
    }

    private void OnDisable()
    {
        // Unsubscribe from all key events
        foreach (var key in Keys)
        {
            key.KeyCollected -= OnKeyCollected;
        }

        // Unsubscribe from all barrier events
        foreach (var barrier in Barriers)
        {
            barrier.LockedBarrierAccessed -= OnBarrierAccessed;
        }
    }

    void Update()
    {
        // If the alert is not currently active, return
        if (!IsActive) return;

        // If the counter is expired, deactivate alerts and set inactive
        if (CounterExpired)
        {
            m_BarrierAlertPanel.SetActive(false);
            IsActive = false;
        }
    }

    void OnAmmoPickup(int value, IGun gun)
    {
        string gunType = gun.GunName;

        m_BarrierAlertPanel.SetActive(true);
        BarrierAlertText.text = $"Aqcuired {value} {gunType} ammo!" ;
        LastMessageTime = Time.time;
        IsActive = true;
    }

    void OnAmmoEmpty()
    {
        m_BarrierAlertPanel.SetActive(true);
        BarrierAlertText.text = m_AmmoAlertContent;
        LastMessageTime = Time.time;
        IsActive = true;
    }

    /// <summary>
    /// Set display panel active and adjust text accordingly, subscribe to key collected events
    /// </summary>
    /// <param name="name"></param>
    void OnKeyCollected(string name)
    {
        m_BarrierAlertPanel.SetActive(true);
        BarrierAlertText.text = m_PrependName ? $"{name} {m_KeyAlertContent}!" : m_KeyAlertContent;
        LastMessageTime = Time.time;
        IsActive = true;
    }

    /// <summary>
    /// Set display panel active and adjust text accordingly, subscribe to barrier accessed events
    /// </summary>
    void OnBarrierAccessed()
    {
        m_BarrierAlertPanel.SetActive(true);
        BarrierAlertText.text = m_BarrierAlertContent;
        LastMessageTime = Time.time;
        IsActive = true;
    }
}
