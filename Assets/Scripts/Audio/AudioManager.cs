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
    [Header("Navigator Audio Source and Image")]
    [SerializeField] AudioSource m_AudioSource; // AudioSource component that plays the audio clips
    [SerializeField] Image m_NavImage; // Image component that displays a navigation icon

    [Header("Soundtrack Audio Sources")]
    [SerializeField] AudioSource m_PrimaryMusicSource;
    [SerializeField] AudioSource m_SecondaryMusicSource;

    [Header("Soundtrack Clips")]
    [SerializeField] AudioClip m_ExplorationMusic;
    [SerializeField] AudioClip m_CombatMusic;
    [SerializeField] AudioClip m_ReleaseMusic;

    [Header("Soundtrack Transition options")]
    [SerializeField] float m_TransitionSpeed;
    [SerializeField] float m_ReleaseTime;
    [SerializeField] float m_InitialCombatVolume;
    [SerializeField] float m_CombatExitDelay;

    [Header("Soundtrack Volume Options")]
    [SerializeField] float m_ExplorationVolume;
    [SerializeField] float m_CombatVolume;
    [SerializeField] float m_ReleaseVolume;

    AudioSource m_CurrentPrimary;
    AudioSource m_CurrentSecondary;

    Queue<AudioClip> m_ClipQueue; // A queue that stores audio clips to be played

    Coroutine m_CombatTransitionRoutine;
    WaitForSeconds m_CombatExitDelayWFS;

    float m_LastReleaseTime;

    bool m_IsReleasing;

    bool m_IsCombat;

    bool m_Shouldrelease => m_IsReleasing && Time.time > m_ReleaseTime + m_LastReleaseTime;

    float m_CurrentVolume => m_CurrentPrimary.clip == m_ExplorationMusic ? m_ExplorationVolume : m_CurrentPrimary.clip == m_CombatMusic ? m_CombatVolume : m_ReleaseVolume;

    // On start, the AudioSource component is assigned to the Player game object's AudioSource component
    private void Start()
    {
        m_CombatExitDelayWFS = new WaitForSeconds(m_CombatExitDelay);
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
        //debug stuff
        //Debug.Log(m_CurrentPrimary.clip.name.Substring(m_CurrentPrimary.clip.name.Length - 20));

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

    private void OnCombatStateChanged(bool newCombatState)
    {
        if (newCombatState && m_IsReleasing) m_IsReleasing = false;

        if (m_IsCombat && !newCombatState) m_CombatTransitionRoutine = StartCoroutine(CombatTransition());
        else
        {
            if (m_CombatTransitionRoutine != null) StopCoroutine(m_CombatTransitionRoutine);

            AdjustCurrentMusic(newCombatState);
        }
    }

    private void AdjustCurrentMusic(bool newCombatState)
    {
        if (newCombatState && m_IsCombat) return;
        m_IsCombat = newCombatState;

        var temp = m_CurrentPrimary;

        m_CurrentPrimary = m_CurrentSecondary;
        m_CurrentPrimary.clip = newCombatState ? m_CombatMusic : m_ReleaseMusic;

        if (m_CurrentPrimary.clip != m_CombatMusic || !m_CurrentPrimary.isPlaying)
        {
            m_CurrentPrimary.Play();
        }

        m_CurrentSecondary = temp;


        if (!newCombatState)
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

    IEnumerator CombatTransition()
    {
        yield return m_CombatExitDelayWFS;

        AdjustCurrentMusic(false);
    }

    private void AdjustPrimaryVolume()
    {
        if (m_CurrentPrimary.volume == m_CurrentVolume) return;
        m_CurrentPrimary.volume = Mathf.MoveTowards(m_CurrentPrimary.volume, m_CurrentVolume, m_TransitionSpeed * Time.deltaTime);
    }

    private void AdjustSecondaryVolume()
    {
        if (m_CurrentSecondary.volume == 0) return;
        m_CurrentSecondary.volume = Mathf.MoveTowards(m_CurrentSecondary.volume, 0, m_TransitionSpeed * Time.deltaTime);
    }

}
