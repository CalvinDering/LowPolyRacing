using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAudioHandler : MonoBehaviour {

    [Header("Engine Sound")]
    [SerializeField] private AudioSource engineSound;
    [SerializeField] private float minVolume;
    [SerializeField] private float maxVolume;
    [SerializeField] private float minPitch;
    [SerializeField] private float maxPitch;

    [Header("Rev Limiter")]
    [SerializeField] private float limiterSound = 1f;
    [SerializeField] private float limiterFrequency = 3f;
    [SerializeField] private float limiterEngage = 0.8f;
    private float revLimiter;

    private float speedRatio;

    private AdvancedCarController carController;

    private void Start() {
        carController = GetComponent<AdvancedCarController>();
        engineSound.volume = minVolume;
        engineSound.pitch = minPitch;
    }

    private void Update() {
        if(carController) {
            speedRatio = Mathf.Abs(carController.GetSpeedRatio());
        }
        if(speedRatio > limiterEngage) {
            revLimiter = (Mathf.Sin(Time.time * limiterFrequency) + 1f) * limiterSound * (speedRatio - limiterEngage);
        }

        engineSound.volume = Mathf.Lerp(minVolume, maxVolume, speedRatio);
        engineSound.pitch = Mathf.Lerp(engineSound.pitch, Mathf.Lerp(minPitch, maxPitch, speedRatio) + revLimiter, Time.deltaTime);
       // engineSound.pitch = Mathf.Lerp(minPitch, maxPitch, speedRatio);
    }

}
