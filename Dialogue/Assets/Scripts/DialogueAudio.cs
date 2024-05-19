using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueAudio : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    private AudioClip[] sfx;
    private float volume;

    public void InitDialogueAudio(float vol)
    {
        volume = vol;
    }
    public void SetAudioProfile(AudioClip[] sf, float pitch, float volumeMod)
    {
        sfx = sf;
        audioSource.pitch = pitch;
        audioSource.volume = volume + volumeMod;
    }

    public void PlaySingleNote()
    {
        System.Random rnd = new System.Random();
        audioSource.clip = sfx[rnd.Next(0, sfx.Length)];
        audioSource.Play();
    }
}
