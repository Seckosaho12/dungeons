using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    [Header("Audio Mixer Groups")]
    public AudioMixerGroup masterMixerGroup;
    public AudioMixerGroup sfxMixerGroup;
    public AudioMixerGroup musicMixerGroup;

    [Header("Music")]
    public AudioSource musicSource;
    public AudioClip menuMusicClip;
    public AudioClipSettings menuMusicSettings;
    public AudioClip gameMusicClip;
    public AudioClipSettings gameMusicSettings;
    [Header("Step Sound")]
    public AudioClip stepSoundClip;
    public AudioClipSettings stepSettings;
    [Header("Slash Sound")]
    public AudioClip slashSoundClip;
    public AudioClipSettings slashSettings;
    //CoinCollect Sound
    [Header("CoinCollectSound")]
    //public AudioSource CoinCollectSoundSource;
    public AudioClip CoinCollectSoundClip;
    public AudioClipSettings CoinSettings;

    //Arrow Sound
    [Header("Arrow Sound")]
    //public AudioSource ArrowSoundSource;
    public AudioClip ArrowSoundClip;
    public AudioClipSettings ArrowSettings;
    //Magic Sound
    [Header("Magic Sound")]
    //public AudioSource MagicSoundSource;
    public AudioClip MagicSoundClip;
    public AudioClipSettings MagicSettings;

    //Hurt Sound
    [Header("Hurt Sound")]
    //public AudioSource HurtSoundSource;
    public AudioClip HurtSoundClip;
    public AudioClipSettings HurtSettings;
    //Dash Sound
    [Header("Dash Sound")]
    //public AudioSource DashSoundSource;
    public AudioClip DashSoundClip;
    public AudioClipSettings DashSettings;


    [Header("Enemy Damage Sound")]
    //public AudioSource SlimeAttackSoundSource;
    public AudioClip EnemyDamageSoundClip;
    public AudioClipSettings EnemyDamageSettings;


    //Ghost Attack Sound 
    [Header("Ghost Attack Sound ")]
    //public AudioSource GhostAttackSoundSource;
    public AudioClip GhostAttackSoundClip;

    private void Awake()
    {
        // Check if instance already exists
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // Destroy the new instance
            return; // Exit the method
        }
        // Set the instance to this object
        instance = this;
        // Make this object persistent across scenes
        DontDestroyOnLoad(gameObject);
        PlayRelevantMusic(SceneManager.GetActiveScene().name);
        // Subscribe to the sceneLoaded event to play music when a new scene is loaded
        SceneManager.sceneLoaded += (scene, sceneLoadMode) => PlayRelevantMusic(scene.name);
    }

    private void PlayRelevantMusic(string sceneName)
    {
        if (sceneName.Contains("Menu"))
        {
            PlayMusic(menuMusicClip, menuMusicSettings);
        }
        else
        {
            PlayMusic(gameMusicClip, gameMusicSettings);
        }
    }

    private void PlayMusic(AudioClip clip, AudioClipSettings clipSettings)
    {
        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            return; // Don't play the music again if it's already playing
        }
        musicSource.clip = clip;
        musicSource.volume = clipSettings.volume;
        musicSource.pitch = UnityEngine.Random.Range(clipSettings.pitchRange.x, clipSettings.pitchRange.y);
        musicSource.spatialBlend = clipSettings.is2D ? 0 : 1;
        musicSource.Play();
    }

    public void PlaySFXOneShot(AudioClip audioClip, AudioClipSettings settings)
    {
        GameObject sfx = new GameObject(audioClip.name + " Audio Clip", typeof(AudioSource));
        AudioSource sfxAudioSource = sfx.GetComponent<AudioSource>();
        sfxAudioSource.clip = audioClip;
        sfxAudioSource.outputAudioMixerGroup = sfxMixerGroup;
        sfxAudioSource.volume = settings.volume;
        sfxAudioSource.pitch = UnityEngine.Random.Range(settings.pitchRange.x, settings.pitchRange.y);
        sfxAudioSource.spatialBlend = settings.is2D ? 0 : 1;
        sfxAudioSource.Play();
        Destroy(sfx, audioClip.length);
    }

    public void PlayStepSFX()
    {
        PlaySFXOneShot(stepSoundClip, stepSettings);
    }

    public void PlayCoinSFX()
    {
        PlaySFXOneShot(CoinCollectSoundClip, CoinSettings);
    }
    public void PlayArrowSFX()
    {
        PlaySFXOneShot(ArrowSoundClip, ArrowSettings);
    }
    public void PlayDashSFX()
    {
        PlaySFXOneShot(DashSoundClip, DashSettings);
    }
    public void PlayMagicSFX()
    {
        PlaySFXOneShot(MagicSoundClip, MagicSettings);
    }

    public void PlayHurtSFX()
    {
        PlaySFXOneShot(HurtSoundClip, HurtSettings);
    }

    public void PlayEnemyDamageSFX()
    {
        PlaySFXOneShot(EnemyDamageSoundClip, EnemyDamageSettings);
    }





    public void PlaySlash()
    {
        PlaySFXOneShot(slashSoundClip, slashSettings);
    }

    public void SetMasterVolume(float value)
    {
        // if value is 0, set it to 0.0001 to avoid log10(0) which is undefined
        if (value == 0)
        {
            value = 0.0001f;
        }
        masterMixerGroup.audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);
    }
    public void SetSFXVolume(float value)
    {
        if (value == 0)
        {
            value = 0.0001f;
        }
        sfxMixerGroup.audioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
    }
    public void SetMusicVolume(float value)
    {
        if (value == 0)
        {
            value = 0.0001f;
        }
        musicMixerGroup.audioMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
    }
}

[Serializable]
public class AudioClipSettings
{
    public float volume = 1;
    public Vector2 pitchRange = new(0.8f, 1.2f);
    public bool is2D = true;
}
