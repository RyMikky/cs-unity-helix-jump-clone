using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioEngine : MonoBehaviour
{

    public AudioSource _buttonClickSource;
    public AudioSource _ballBounceSource;
    public AudioSource _platformBreak;
    public AudioSource _backgroundSource;

    public void PlayClick()
    {
        _buttonClickSource.Play();
    }

    public void PlayBallBounce()
    {
        _ballBounceSource.Play();
    }

    public void PlayPlatformBreak()
    {
        _platformBreak.Play();
    }

    public void IncrementPlatformBreakPitch()
    {
        if (_platformBreak.pitch < 3) _platformBreak.pitch += 0.1f;
    }

    public void SetPlatformBreakPitchDefault()
    {
        _platformBreak.pitch = 1;
    }

    public void SetBackgroundVolume(float volume)
    {
        _backgroundSource.volume = volume;
    }

    public float BackgroundVolume()
    {
        return _backgroundSource.volume;
    }

    public void PLauBackground()
    {
        _backgroundSource.loop = true;
        _backgroundSource.Play();
    }
}
