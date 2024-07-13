using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerUIStats : MonoBehaviour {


    [SerializeField] public TextMeshProUGUI countdownTimerText;
    [SerializeField] public TextMeshProUGUI lapCounterText;
    [SerializeField] public TextMeshProUGUI checkpointCounterText;
    [SerializeField] public TextMeshProUGUI checkpointTimeText;
    [SerializeField] public TextMeshProUGUI positionText;

    [SerializeField] private float raceCountdownFadeOutTimer = 0.5f;
    [SerializeField] private float checkpointTimeFading = 2f;

    public void SetCountdownTimerText(string raceStartText) {
        countdownTimerText.text = raceStartText;
        StartCoroutine(FadeTextToZeroAlpha(raceCountdownFadeOutTimer, countdownTimerText));
    }

    public void SetLapCounterText(int laps, int maxLaps) {
        lapCounterText.text = "Laps: " + laps.ToString() + " / " + maxLaps;
    }

    public void SetCheckpointCounterText(string checkpointCounter, int maxCheckpoint) {
        checkpointCounterText.text = "CP: " + checkpointCounter + " / " + maxCheckpoint;
    }

    public void SetCheckpointTimeText(string text, bool fadeOut) {
        checkpointTimeText.text = text;
        if(fadeOut) {
            StartCoroutine(FadeTextToZeroAlpha(checkpointTimeFading, checkpointTimeText));
        } else {
            checkpointTimeText.color = new Color(checkpointTimeText.color.r, checkpointTimeText.color.g, checkpointTimeText.color.b, 1.0f);
        }
    }

    public void SetCheckpointTimeText(float checkpointTime, bool fadeOut = true) {
        int minutes = (int) (checkpointTime / 60);
        int seconds = (int) (checkpointTime % 60);
        int milliseconds = (int) ((checkpointTime % 1f) * 1000);

        SetCheckpointTimeText(string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds), fadeOut);
    }

    public void SetPosition(int position) {
        positionText.text = position + ".";
    }

    private IEnumerator FadeTextToZeroAlpha(float timer, TextMeshProUGUI text) {
        text.color = new Color(text.color.r, text.color.g, text.color.b);
        while(text.color.a > 0.0f) {
            text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a - (Time.deltaTime / timer));
            yield return null;
        }
    }
}
