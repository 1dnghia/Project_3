using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : NMonoBehaviour
{
    private static SoundManager _instance;
    public static SoundManager Instance => _instance;

    public AudioSource MusicAudioSource;
    public AudioSource SFXAudioSource;

    public AudioClip musicClip;
    public AudioClip connectClip;
    public AudioClip winClip;

    protected override void Awake()
    {
        if (Instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    protected override void Start()
    {
        MusicAudioSource.clip = musicClip;
        MusicAudioSource.loop = true;
        MusicAudioSource.Play();
    }

    public void PlaySFX(AudioClip sfxClip)
    {
        SFXAudioSource.clip = sfxClip;
        SFXAudioSource.PlayOneShot(sfxClip);
    }
}
