using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerAudio : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] jumpAudioClip;
    public AudioClip hitAudioClip;
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

    public void playJumpAudio()
    {
        if (!audioSource.isPlaying)
        {
            int randomNumber = Random.Range(0, jumpAudioClip.Length);
            audioSource.clip = jumpAudioClip[randomNumber];
            playAudioSource();
        }
    }

    public void playHitAudio()
    {
        audioSource.clip = hitAudioClip;
        playAudioSource();
    }
}
