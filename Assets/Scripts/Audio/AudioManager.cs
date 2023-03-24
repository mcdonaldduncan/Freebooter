using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 
/// </summary>
/// Author: Duncan McDonald
public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] AudioSource m_AudioSource;// AudioSource component that plays the audio clips
    [SerializeField] AudioSource m_PrimaryMusicSource;
    [SerializeField] AudioSource m_SecondaryMusicSource;
    [SerializeField] Image m_NavImage; // Image component that displays a navigation icon
    [SerializeField] float m_TransitionSpeed;
    [SerializeField] float m_ReleaseTime;
    [SerializeField] float m_InitialCombatVolume;


    [SerializeField] AudioClip m_ExplorationMusic;
    [SerializeField] AudioClip m_CombatMusic;
    [SerializeField] AudioClip m_ReleaseMusic;

    [SerializeField] float m_ExplorationVolume;
    [SerializeField] float m_CombatVolume;
    [SerializeField] float m_ReleaseVolume;

    AudioSource m_CurrentPrimary;
    AudioSource m_CurrentSecondary;

    Queue<AudioClip> m_ClipQueue; // A queue that stores audio clips to be played

    float m_LastReleaseTime;

    bool m_IsReleasing;

    bool m_Shouldrelease => m_IsReleasing && Time.time > m_ReleaseTime + m_LastReleaseTime;

    float m_currentVolume => m_CurrentPrimary.clip == m_ExplorationMusic ? m_ExplorationVolume : m_CurrentPrimary.clip == m_CombatMusic ? m_CombatVolume : m_ReleaseVolume;

    // On start, the AudioSource component is assigned to the Player game object's AudioSource component
    private void Start()
    {
        LevelManager.Instance.CombatStateChanged += OnCombatStateChanged;

        if (m_PrimaryMusicSource == null || m_SecondaryMusicSource == null)
        {
            Debug.LogError("Music sources are not set up correctly");
        }

        m_PrimaryMusicSource.clip = m_ExplorationMusic;
        m_PrimaryMusicSource.Play();

        m_CurrentPrimary = m_PrimaryMusicSource;
        m_CurrentSecondary = m_SecondaryMusicSource;

        if (m_AudioSource != null) return;
        m_AudioSource = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<AudioSource>();
    }

    // In update, if there are any clips in the queue and the audio source is not currently playing, the next clip in the queue is played
    void Update()
    {
        AdjustPrimaryVolume();
        AdjustSecondaryVolume();

        if (m_Shouldrelease) ExplorationTransition();
        


        if (m_ClipQueue == null) return;
        if (!m_AudioSource.isPlaying && m_ClipQueue.Count > 0)
        {
            m_AudioSource.clip = m_ClipQueue.Dequeue();
            m_AudioSource.Play();
        }
    }

    // Method that plays an audio clip immediately
    public void PlayClip(AudioClip clip)
    {
        m_AudioSource.clip = clip;
        m_AudioSource.Play();
    }

    // Method that enables the navigation image and sets its sprite
    public void EnableNavImage(Sprite sprite)
    {
        m_NavImage.sprite = sprite;
        m_NavImage.enabled = true;
    }

    // Method that disables the navigation image
    public void DisableNavImage()
    {
        m_NavImage.enabled = false;
    }

    // Method that adds an audio clip to the queue to be played
    public void QueueSound(AudioClip clip)
    {
        m_ClipQueue.Enqueue(clip);
    }

    // Method that clears the audio clip queue
    public void OverrideQueue()
    {
        m_ClipQueue.Clear();
    }

    // Method that checks if the current audio clip is finished playing
    public bool ClearCheck(AudioClip clip)
    {
        return m_AudioSource.clip == clip && !m_AudioSource.isPlaying;
    }

    private void OnCombatStateChanged(bool inCombat)
    {
        var temp = m_CurrentPrimary;

        m_CurrentPrimary = m_CurrentSecondary;
        m_CurrentPrimary.clip = inCombat ? m_CombatMusic : m_ReleaseMusic;
        m_CurrentPrimary.Play();

        m_CurrentSecondary = temp;
        
        

        if (!inCombat)
        {
            m_IsReleasing = true;
            m_LastReleaseTime = Time.time;
        }
        else
        {
            m_CurrentPrimary.volume = m_InitialCombatVolume;
        }
    }

    private void ExplorationTransition()
    {
        var temp = m_CurrentPrimary;

        m_CurrentPrimary = m_CurrentSecondary;
        m_CurrentPrimary.clip = m_ExplorationMusic;
        m_CurrentPrimary.Play();

        m_CurrentSecondary = temp;

        m_IsReleasing = false;
    }

    public void AdjustPrimaryVolume()
    {
        if (m_CurrentPrimary.volume == 1) return;
        m_CurrentPrimary.volume = Mathf.MoveTowards(m_CurrentPrimary.volume, m_currentVolume, m_TransitionSpeed * Time.deltaTime);
    }

    public void AdjustSecondaryVolume()
    {
        if (m_CurrentPrimary.volume == 0) return;
        m_CurrentSecondary.volume = Mathf.MoveTowards(m_CurrentSecondary.volume, 0, m_TransitionSpeed * Time.deltaTime);
    }
}
