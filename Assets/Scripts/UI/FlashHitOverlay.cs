using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FlashHitOverlay : MonoBehaviour
{
    [SerializeField] private float m_fadeTime;
    
    private float m_alpha;
    private float m_playerHealthBefore;
    private float m_playerHealthAfter;

    private Image m_overlayImage;
    private Color m_overlayColorHit;
    private Color m_overlayColorFaded;

    private void Start()
    {
        m_overlayImage = gameObject.GetComponent<Image>();

        LevelManager.Instance.Player.PlayerHealthChanged += OnPlayerHit;
        m_playerHealthBefore = LevelManager.Instance.Player.Health;
        m_overlayColorFaded = new Color(m_overlayImage.color.r, m_overlayImage.color.g, m_overlayImage.color.b, 0);
        m_overlayColorHit = new Color(m_overlayImage.color.r, m_overlayImage.color.g, m_overlayImage.color.b, 1);

        m_overlayImage.color = m_overlayColorFaded;
    }

    private void Update()
    {
        if (m_alpha > 0f)
        {
            m_alpha -= Time.deltaTime / m_fadeTime;
            m_overlayImage.color = new Color(m_overlayColorHit.r, m_overlayColorHit.g, m_overlayColorHit.b, m_alpha);
        }
        else
        {
            m_overlayImage.color = m_overlayColorFaded;
        }
    }

    private void OnPlayerHit()
    {
        m_playerHealthAfter = LevelManager.Instance.Player.Health;
        if (m_playerHealthAfter < m_playerHealthBefore)
        {
            m_alpha = 1f;
            m_overlayImage.color = m_overlayColorHit;
        }
        m_playerHealthBefore = m_playerHealthAfter;
    }
}
