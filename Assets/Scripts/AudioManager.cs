using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    [Header("Slash Sound")]
    //public AudioSource slashSoundSource;
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
    //Magic Sound
    [Header("Magic Sound")]
    //public AudioSource MagicSoundSource;
    public AudioClip MagicSoundClip;
    //Hurt Sound
    [Header("Hurt Sound")]
    //public AudioSource HurtSoundSource;
    public AudioClip HurtSoundClip;
    //Dash Sound
    [Header("Dash Sound")]
    //public AudioSource DashSoundSource;
    public AudioClip DashSoundClip;

    //Enemy Sounds
    //Slime Attack Sound
    [Header("Slime Attack Sound")]
    //public AudioSource SlimeAttackSoundSource;
    public AudioClip SlimeAttackSoundClip;
    //Ghost Attack Sound 
    [Header("Ghost Attack Sound ")]
    //public AudioSource GhostAttackSoundSource;
    public AudioClip GhostAttackSoundClip;

    private void Awake()
    {
        instance = this;
    }

    public void PlaySFXOneShot(AudioClip audioClip, AudioClipSettings settings)
    {
        GameObject sfx = new GameObject(audioClip.name + " Audio Clip", typeof(AudioSource));
        AudioSource sfxAudioSource = sfx.GetComponent<AudioSource>();
        sfxAudioSource.clip = audioClip;
        sfxAudioSource.volume = settings.volume;
        sfxAudioSource.pitch = UnityEngine.Random.Range(settings.pitchRange.x, settings.pitchRange.y);
        sfxAudioSource.spatialBlend = settings.is2D ? 1 : 0;
        sfxAudioSource.Play();
        Destroy(sfx, audioClip.length);
    }

    public void PlayCoinSFX(){
        PlaySFXOneShot(CoinCollectSoundClip, CoinSettings);
    }

    public void PlaySlash()
    {
        PlaySFXOneShot(slashSoundClip, slashSettings);
    }

}

[Serializable]
public class AudioClipSettings
{
    public float volume = 1;
    public Vector2 pitchRange = new(0.8f, 1.2f);
    public bool is2D = true;
}
