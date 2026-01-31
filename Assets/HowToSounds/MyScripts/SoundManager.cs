/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using CodeMonkey;

public static class SoundManager {

    public enum Sound {
        PlayerDash,
        PlayerWalk,
        PlayerRun,
        PlayerLand,
        PlayerCrouchWalk,
        PlayerSlide,
        PlayerJump,

        //implement edilmedi daha
        PlayerDie,
        PortalSound,
        PortalEnter,
        ItemCollect,
        ButtonClick,
        ButtonOver
    }

    private static Dictionary<Sound, float> soundTimerDictionary;
    private static GameObject oneShotGameObject;
    private static AudioSource oneShotAudioSource;

    public static void Initialize() {
        soundTimerDictionary = new Dictionary<Sound, float>();
        soundTimerDictionary[Sound.PlayerWalk] = 0f;
        soundTimerDictionary[Sound.PlayerRun] = 0f;
        soundTimerDictionary[Sound.PlayerCrouchWalk] = 0f;
        soundTimerDictionary[Sound.PortalSound] = 0f;

    }

    public static void PlaySound(Sound sound, Vector3 position) {
        if (CanPlaySound(sound)) {
            GameObject soundGameObject = new GameObject("Sound");
            soundGameObject.transform.position = position;
            AudioSource audioSource = soundGameObject.AddComponent<AudioSource>();
            audioSource.clip = GetAudioClip(sound);
            audioSource.maxDistance = 100f;
            audioSource.spatialBlend = 1f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.dopplerLevel = 0f;
            oneShotAudioSource.volume = GetVolume(sound);
            audioSource.Play();

            Object.Destroy(soundGameObject, audioSource.clip.length);
        }
    }

    public static void PlaySound(Sound sound)
    {
        if (!CanPlaySound(sound)) return;

        if (oneShotGameObject == null)
        {
            oneShotGameObject = new GameObject("One Shot Sound");
            oneShotAudioSource = oneShotGameObject.AddComponent<AudioSource>();
            // burada sabit ayarlar (spatial blend, output mixer, rolloff vs) yapabilirsin
        }

        AudioClip clip = GetAudioClip(sound);
        float volume = GetVolume(sound); // 0..1 aralığında döndür

        oneShotAudioSource.PlayOneShot(clip, volume);
    }


    private static bool CanPlaySound(Sound sound) {
        if ( sound == Sound.PlayerWalk ||  sound == Sound.PlayerCrouchWalk || sound == Sound.PlayerRun || sound == Sound.PortalSound) {
            if (soundTimerDictionary.ContainsKey(sound))
            {
                float lastTimePlayed = soundTimerDictionary[sound];
                float playerMoveTimerMax = 0.15f;
                foreach (GameAssets.SoundAudioClip soundAudioClip in GameAssets.i.soundAudioClipArray)
                {
                    if (soundAudioClip.sound == sound)
                    {
                        playerMoveTimerMax = soundAudioClip.coolDown;
                    }
                }
                if (lastTimePlayed + playerMoveTimerMax < Time.time)
                {
                    soundTimerDictionary[sound] = Time.time;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        } 
        else{
            return true;
        }
    }

    private static AudioClip GetAudioClip(Sound sound) {
        foreach (GameAssets.SoundAudioClip soundAudioClip in GameAssets.i.soundAudioClipArray) {
            if (soundAudioClip.sound == sound) {
                int v = Random.Range(0, soundAudioClip.audioClip.Length);
                return soundAudioClip.audioClip[v];
            }
        }
        Debug.LogError("Sound " + sound + " not found!");
        return null;
    }

    private static float GetVolume(Sound sound)
    {
        foreach (GameAssets.SoundAudioClip soundAudioClip in GameAssets.i.soundAudioClipArray)
        {
            if (soundAudioClip.sound == sound)
            {
                return soundAudioClip.volumeMultiply;
            }
        }
        Debug.LogError("Sound " + sound + " not found!");
        return 0.5f;
    }
    public static void AddButtonSounds(this Button_UI buttonUI)
    {
        buttonUI.ClickFunc += () => SoundManager.PlaySound(Sound.ButtonClick);
        buttonUI.MouseOverOnceFunc += () => SoundManager.PlaySound(Sound.ButtonOver);
    }

}
