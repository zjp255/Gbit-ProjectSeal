using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public bool isStart = true;
    public AudioSource startAudioSource;
    public AudioClip[] inLevelAudioClip;
    // Start is called before the first frame update
    void Start()
    {
        if (isStart == true)
        {
            startAudioSource.Play();
        }
        else
        {
            playInGameAudio();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!startAudioSource.isPlaying && !isStart)
        {
            playInGameAudio();
        }
    }

    public void playStartAudioSource()
    {
        startAudioSource.Play();
    }

    public void closeStartAudioSource()
    {
        startAudioSource.Stop();
    }

    public void musicStartAudioSourceBut()
    {
        if (startAudioSource.isPlaying)
        {
            if (isStart == true)
            {
                startAudioSource.Play();
            }
            else
            {
                playInGameAudio();
            }
        }
        else
        {
            playStartAudioSource();
        }
    }

    public void playInGameAudio()
    {
        int randomNumber = Random.Range(0, inLevelAudioClip.Length);
        startAudioSource.clip = inLevelAudioClip[randomNumber];
        playStartAudioSource();
    }
}
