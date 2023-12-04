using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioSettings : MonoBehaviour
{
    public AudioMixer audioMixer;

    public void SetMasterVolumn (float masterVolumn)
    {
        audioMixer.SetFloat("Master", masterVolumn);
    }

    public void SetMusicVolumn(float musicVolumn)
    {
        audioMixer.SetFloat("Music", musicVolumn);
    }

    public void SetSFXVolumn(float sFXVolumn)
    {
        audioMixer.SetFloat("SFX", sFXVolumn);
    }
}
