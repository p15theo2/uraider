using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerSFX : MonoBehaviour
{
    private AudioSource playerSource;

    [Header("Properties")]
    public float footMinVol = 0.22f;
    public float footMaxVol = 0.3f;
    [Header("Sound Files")]
    public AudioClip[] feetSounds;
    public AudioClip[] jumpSounds;
    public AudioClip[] grabSounds;
    public AudioClip[] vaultSounds;
    public AudioClip[] screamSounds;
    public AudioClip[] slapSounds;

    private void Start()
    {
        playerSource = GetComponent<AudioSource>();
    }

    public void PlayFootSound()
    {
        PlayRandomSound(feetSounds, Random.Range(footMinVol, footMaxVol));
    }

    public void PlayJumpSound()
    {
        PlayRandomSound(jumpSounds, 1);
    }

    public void PlayGrabSound()
    {
        PlayRandomSound(grabSounds, 1);
    }

    public void PlayVaultSound()
    {
        PlayRandomSound(vaultSounds, 1);
    }

    public void PlayScreamSound()
    {
        PlayRandomSound(screamSounds, 1);
    }

    public void PlaySlapSounds()
    {
        PlayRandomSound(slapSounds, 0.25f);
    }

    private void PlayRandomSound(AudioClip[] sounds, float volume)
    {
        int random = Random.Range(0, sounds.Length - 1);
        playerSource.PlayOneShot(sounds[random], volume);
    }
}
