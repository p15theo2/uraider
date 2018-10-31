using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSFX : MonoBehaviour
{
    private AudioSource playerSource;

    public AudioClip[] bangSounds;

    private void Start()
    {
        playerSource = GetComponent<AudioSource>();
    }

    public void PlayBang()
    {
        PlayRandomSound(bangSounds, .25f);
    }

    private void PlayRandomSound(AudioClip[] sounds, float volume)
    {
        int random = Random.Range(0, sounds.Length - 1);
        playerSource.PlayOneShot(sounds[random], volume);
    }
}
