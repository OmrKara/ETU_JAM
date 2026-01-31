using UnityEngine;

public enum SoundType
{
    levelAtlama,
    maskeAlma,
    die,
    walk,
    running,
    jump
}

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] soundList;
    private static SoundManager instance;
    private AudioSource audioSource;

    private void Awake()
    {
        instance = this;
    }

    public static void PlaySound(SoundType soundType , float volume = 1)
    {

    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
}
