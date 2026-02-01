using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.loop = true;
    }

    private void Update()
    {
        source.volume = AudioSettings.MusicVolume;
    }
}
