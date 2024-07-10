using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour {

    public static MusicManager Instance;

    [SerializeField] private AudioSource menuAudioSource;
    [SerializeField] private AudioSource raceAudioSource;
    [SerializeField] private AudioClip menuMusicClip;
    [SerializeField] private AudioClip[] raceMusicClips;
    [SerializeField] private float fadeDuration = 1f;

    private void Awake() {
        if(Instance != this && Instance != null) {
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);
    }

    public void PlayMenuMusic() {
        menuAudioSource.clip = menuMusicClip;
        StartCoroutine(FadeBetween(raceAudioSource, menuAudioSource, fadeDuration, 1f));
    }

    public void PlayGameMusic() {
        raceAudioSource.clip = raceMusicClips[Random.Range(0, raceMusicClips.Length - 1)];
        StartCoroutine(FadeBetween(menuAudioSource, raceAudioSource, fadeDuration, 1f));
    }

    private IEnumerator FadeBetween(AudioSource source, AudioSource target, float duration, float targetVolume) {

        target.volume = 0f;
        target.Play();

        float time = 0f;
        float startVolume = source.volume;

        while(time < 0.98f) {
            time = Mathf.Lerp(time, 1f, Time.deltaTime / duration);
            source.volume = Mathf.Lerp(startVolume, 0f, time);
            target.volume = Mathf.Lerp(0f, targetVolume, time);
            yield return null;
        }

        target.volume = targetVolume;
        source.volume = 0f;
        source.Stop();
        yield break;
    }

}
