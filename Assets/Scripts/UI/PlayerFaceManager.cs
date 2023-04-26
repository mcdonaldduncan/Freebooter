using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerFaceManager : MonoBehaviour
{
    [SerializeField] private Sprite PlayerFace100;
    [SerializeField] private Sprite PlayerFace80;
    [SerializeField] private Sprite PlayerFace60;
    [SerializeField] private Sprite PlayerFace40;
    [SerializeField] private Sprite PlayerFace20;
    [SerializeField] private Sprite PlayerFace0;

    private Image m_imageComponent;
    private FirstPersonController m_player;
    private float m_currentHealthPercent;
    private Dictionary<float, Sprite> m_healthFaceDict;

    // Start is called before the first frame update
    void Start()
    {
        m_imageComponent = GetComponent<Image>();
        m_player = LevelManager.Instance.Player;
        m_player.PlayerHealthChanged += UpdateFace;

        m_healthFaceDict = new Dictionary<float, Sprite>() 
        { 
            { 1, PlayerFace100 }, 
            { 0.8f, PlayerFace80 }, 
            { 0.6f, PlayerFace60 },
            { 0.4f, PlayerFace40 },
            { 0.2f, PlayerFace20 },
            { 0, PlayerFace0 }
        };
    }

    // Update is called once per frame
    void Update()
    {
        //UpdateFace();
    }

    private void UpdateFace()
    {
        m_currentHealthPercent = (float)Math.Round((m_player.Health / m_player.MaxHealth) * 5, MidpointRounding.AwayFromZero) / 5; //This is to make sure the decimal always rounds to a certain fifth of 1
        m_currentHealthPercent = m_currentHealthPercent == 0 && m_player.Health > 0 ? 0.2f : m_currentHealthPercent;

        m_currentHealthPercent = m_currentHealthPercent >= 0 ? m_currentHealthPercent : 0;
        m_imageComponent.sprite = m_healthFaceDict[m_currentHealthPercent];
    }
}
