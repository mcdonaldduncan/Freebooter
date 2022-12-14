using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] AudioSource m_AudioSource;

    Queue<AudioClip> m_ClipQueue;

    private void Start()
    {
        if (m_AudioSource != null) return;
        m_AudioSource = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<AudioSource>();
    }

    void Update()
    {
        if (m_ClipQueue == null) return;
        if (!m_AudioSource.isPlaying && m_ClipQueue.Count > 0)
        {
            m_AudioSource.clip = m_ClipQueue.Dequeue();
            m_AudioSource.Play();
        }
    }


    public void QueueSound(AudioClip clip)
    {
        m_ClipQueue.Enqueue(clip);
    }

    public void OverrideQueue()
    {
        m_ClipQueue.Clear();
    }
}
