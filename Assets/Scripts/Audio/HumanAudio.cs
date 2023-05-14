using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanAudio : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip findplayerAudioClip;
    public AudioClip beHitAudioClip;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void playAudioSource()
    {
        audioSource.Play();
    }
    public void stopAudioSource()
    {
        audioSource.Stop();
    }

    public void findPlayerAudio()
    {
        if (!audioSource.isPlaying)
        {

            audioSource.clip = findplayerAudioClip;
            playAudioSource();
        }
    }

    public void playHitAudio()
    {
        audioSource.clip = beHitAudioClip;
        playAudioSource();
    }
}
