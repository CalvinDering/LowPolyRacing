using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerUIStats : MonoBehaviour {

    [SerializeField] public TextMeshProUGUI countdownTimerText;
    [SerializeField] public TextMeshProUGUI lapCounterText;
    [SerializeField] public TextMeshProUGUI checkpointCounterText;

    public void SetCountdownTimerText(string raceStartText, float raceCountdownFadeOutTimer) {
        countdownTimerText.text = raceStartText;
        StartCoroutine(FadeTextToZeroAlpha(raceCountdownFadeOutTimer, countdownTimerText));
    }

    public void SetLapCounterText(int laps, int maxLaps) {
        lapCounterText.text = "Laps: " + laps.ToString() + " / " + maxLaps;
    }

    public void SetCheckpointCounterText(string checkpointCounter, int maxCheckpoint) {
        checkpointCounterText.text = "CP: " + checkpointCounter + " / " + maxCheckpoint;
    }

    private IEnumerator FadeTextToZeroAlpha(float timer, TextMeshProUGUI text) {
        text.color = new Color(text.color.r, text.color.g, text.color.b);
        while(text.color.a > 0.0f) {
            text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a - (Time.deltaTime / timer));
            yield return null;
        }
    }
}
