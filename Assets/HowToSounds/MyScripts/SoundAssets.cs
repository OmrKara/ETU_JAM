using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
[System.Serializable]
public class SoundAssets : MonoBehaviour
{
    public static SoundAssets i;

    private void Awake()
    {
        if (i == null)
        {
            i = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        StartMusic?.Invoke();
    }
    public SoundAudioClip[] soundAudioClipArray;
    public float MasterVolume;
    public float MusicVolume;
    public float EffectVolume;
    public float DialogueVolume;


    public AudioSource Music;
    public AudioSource Atmosfer;
    public AudioSource UIEffect;

    public UnityEvent StartMusic;


    [System.Serializable]
    public class SoundAudioClip
    {
        public SoundManager.Sound sound;

        public AudioClips[] ClipKinds;
        [System.Serializable]
        public class AudioClips
        {
            public AudioClip[] clips;
            [Range(0f, 1f)]
            public float volume = 0.5f;
        }
        public float WaitTime;

        [Range(.1f, 3f)]
        public float pitch;
        public bool loop;
    }
}

