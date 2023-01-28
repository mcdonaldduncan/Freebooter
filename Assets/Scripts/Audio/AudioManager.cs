using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// AudioManager is a singleton class that manages audio playback in the game.
public class AudioManager : Singleton<AudioManager>
{
    // AudioSource component that plays the audio clips
    [SerializeField] AudioSource m_AudioSource;
    // Image component that displays a navigation icon
    [SerializeField] Image m_NavImage;

    // A queue that stores audio clips to be played
    Queue<AudioClip> m_ClipQueue;

    // On start, the AudioSource component is assigned to the Player game object's AudioSource component
    private void Start()
    {
        if (m_AudioSource != null) return;
        m_AudioSource = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<AudioSource>();
    }

    // In update, if there are any clips in the queue and the audio source is not currently playing, the next clip in the queue is played
    void Update()
    {
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
}
