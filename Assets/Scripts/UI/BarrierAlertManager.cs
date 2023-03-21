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
    
    TextMeshProUGUI BarrierAlertText;

    Key[] Keys;
    Barrier[] Barriers;

    float LastMessageTime;

    bool IsActive;

    bool CounterExpired => Time.time > LastMessageTime + m_DisplayTime;

    void Start()
    {
        BarrierAlertText = m_BarrierAlertPanel.GetComponentInChildren<TextMeshProUGUI>();
        m_BarrierAlertPanel.SetActive(false);

        IsActive = false;

        Keys = FindObjectsOfType<Key>();
        Barriers = FindObjectsOfType<Barrier>();

        foreach (var key in Keys)
        {
            key.KeyCollected += OnKeyCollected;
        }

        foreach (var barrier in Barriers)
        {
            barrier.LockedBarrierAccessed += OnBarrierAccessed;
        }
    }

    private void OnDisable()
    {
        foreach (var key in Keys)
        {
            key.KeyCollected -= OnKeyCollected;
        }

        foreach (var barrier in Barriers)
        {
            barrier.LockedBarrierAccessed -= OnBarrierAccessed;
        }
    }

    void Update()
    {
        if (!IsActive) return;

        if (CounterExpired)
        {
            m_BarrierAlertPanel.SetActive(false);
            IsActive = false;
        }
    }

    void OnKeyCollected(string name)
    {
        m_BarrierAlertPanel.SetActive(true);
        BarrierAlertText.text = m_PrependName ? $"{name} {m_KeyAlertContent}!" : m_KeyAlertContent;
        LastMessageTime = Time.time;
        IsActive = true;
    }

    void OnBarrierAccessed()
    {
        m_BarrierAlertPanel.SetActive(true);
        BarrierAlertText.text = m_BarrierAlertContent;
        LastMessageTime = Time.time;
        IsActive = true;
    }
}
