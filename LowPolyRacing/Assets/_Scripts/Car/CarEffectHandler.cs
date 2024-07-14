using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEffectHandler : MonoBehaviour {

    private float speedModifier = 1f;
    private float speedModifierDecreaseStep = 1f;

    private bool isSpeedModifierActive = false;

    private void Update() {

        if(isSpeedModifierActive) {
            speedModifier = Mathf.Lerp(speedModifier, 1f, speedModifierDecreaseStep * Time.deltaTime);

            if(speedModifier <= 1.005f) {
                speedModifier = 1f;
                isSpeedModifierActive = false;
            }
        }
    }

    public void ActivateSpeedModifier() {
        isSpeedModifierActive = true;
    }

    public bool IsSpeedModifierActive() {
        return isSpeedModifierActive;
    }

    public void SetSpeedModifier(float modifier) {
        speedModifier = modifier;
    }

    public float GetSpeedModifier() {
        return speedModifier;
    }

    public void SetSpeedModifierDecreaseStep(float step) {
        speedModifierDecreaseStep = step;
    }
}
