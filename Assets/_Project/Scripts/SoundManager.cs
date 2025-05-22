using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : NMonoBehaviour
{
    private static SoundManager _instance;
    public static SoundManager Instance => _instance;

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

    [SerializeField] private AudioSource _effectSource;

    public void PlaySound(AudioClip clip)
    {
        _effectSource.PlayOneShot(clip);
    }
}
