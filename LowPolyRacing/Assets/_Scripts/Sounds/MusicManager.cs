using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour {

    [SerializeField] private AudioClip[] musicClips;
    private AudioSource audioSource;

    private void Start() {
        audioSource = GetComponent<AudioSource>();

        audioSource.clip = musicClips[Random.Range(0, musicClips.Length - 1)];
        audioSource.Play();
    }

}
