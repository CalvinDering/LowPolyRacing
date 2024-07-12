using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundMixerManager : MonoBehaviour {

    public static SoundMixerManager Instance;

    [SerializeField] private AudioMixer audioMixer;

    private void Awake() {
        if(Instance != this && Instance != null) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetMasterVolume(float level) {
        audioMixer.SetFloat("MasterVolume", level);    }
    
    public void SetMusicVolume(float level) {
        audioMixer.SetFloat("MusicVolume", level);
    }
    
    public void SetSoundFXVolume(float level) {
        audioMixer.SetFloat("SoundFXVolume", level);
    }

    

}
