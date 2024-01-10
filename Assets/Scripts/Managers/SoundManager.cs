using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SoundManager : Singleton<SoundManager>
{
    [Header("SFX")]
    [SerializeField] private AudioClip descendTowerSFX;
    [SerializeField] private AudioClip endGameStartSFX;
    [SerializeField] private AudioClip endGameDynamiteSFX;
    [SerializeField] private AudioSource descendAudioSource;
    [SerializeField] private AudioSource endGameAudioSource;

    public void SetVolume(Source source , float volume)
    {
        if(volume < 0f) volume = 0f;
        else if(volume > 1f) volume = 1f;

        switch (source)
        {
            case Source.EndGame:
                endGameAudioSource.volume = volume;
                break;
            case Source.Descend:
                descendAudioSource.volume = volume;
                break;
        }
    }

    public float GetVolume(Source source)
    {
        float volume = 0f;
        switch (source)
        {
            case Source.EndGame:
                volume = endGameAudioSource.volume;
                break;
            case Source.Descend:
                volume = descendAudioSource.volume;
                break;
        }
        return volume;
    }

    public void PlayOneShot(Source source, Sound sound)
    {
        AudioSource audioSource = null;
        AudioClip audioClip = null;

        switch (source)
        {
            case Source.EndGame:
                audioSource = endGameAudioSource;
                break;
            case Source.Descend:
                audioSource = descendAudioSource;
                break;
        }

        switch (sound)
        {
            case Sound.DescendTower:
                audioClip = descendTowerSFX;
                break;
            case Sound.EndGameStart:
                audioClip = endGameStartSFX;
                break;
            case Sound.EndGameDynamite:
                audioClip = endGameDynamiteSFX;
                break;
        }

        audioSource.PlayOneShot(audioClip);
    }
}
