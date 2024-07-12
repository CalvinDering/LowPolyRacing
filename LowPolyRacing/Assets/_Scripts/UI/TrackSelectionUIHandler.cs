using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrackSelectionUIHandler : MonoBehaviour {

    [SerializeField] private TrackSO[] trackList;
    [SerializeField] private TextMeshProUGUI selectedTrackName;
    [SerializeField] private Image selectedTrackImage;
    [SerializeField] private TextMeshProUGUI selectedTrackLapsText;
    [SerializeField] private TextMeshProUGUI aiRacerCountText;

    public int selectedTrackId = 0;
    public int selectedTrackLaps = 3;
    public int aiRacerCount = 0;

    public static TrackSelectionUIHandler Instance;

    private void Awake() {
        if(Instance != null && Instance != null) {
            Destroy(this);
            return;
        }
        Instance = this;

        UpdateTrackDisplay();
    }

    public void NextTrack() {
        selectedTrackId++;

        if(selectedTrackId >= trackList.Length) {
            selectedTrackId = 0;
        }

        UpdateTrackDisplay();
    }

    public void PrevTrack() {
        selectedTrackId--;

        if(selectedTrackId < 0) {
            selectedTrackId = trackList.Length - 1;
        }

        UpdateTrackDisplay();
    }

    public void IncreaseLapCount() {
        selectedTrackLaps++;

        if(selectedTrackLaps >= 10) {
            selectedTrackLaps = 1;
        }

        UpdateLapDisplay();
    }

    public void DecreaseLapCount() {
        selectedTrackLaps--;

        if(selectedTrackLaps < 1) {
            selectedTrackLaps = 9;
        }

        UpdateLapDisplay();
    }

    public void IncreaseAIRacerCount() {
        aiRacerCount++;

        if(aiRacerCount >= 9) {
            aiRacerCount = 0;
        }

        UpdateAICountDisplay();
    }

    public void DecreaseAIRacerCount() {
        aiRacerCount--;

        if(aiRacerCount < 0) {
            aiRacerCount = 8;
        }

        UpdateAICountDisplay();
    }

    private void UpdateTrackDisplay() {
        selectedTrackName.text = trackList[selectedTrackId].trackName;
        selectedTrackImage.sprite = trackList[selectedTrackId].trackImage;
        selectedTrackLaps = trackList[selectedTrackId].trackLapCount;
        UpdateLapDisplay();
    }

    private void UpdateLapDisplay() {
        selectedTrackLapsText.text = "Laps: " + selectedTrackLaps;
    }

    private void UpdateAICountDisplay() {
        aiRacerCountText.text = "COMS: " + aiRacerCount;
    }

    public void StartGame() {
        SceneHandler.Instance.LoadGameScene();
    }

    public void GoToMainMenu() {
        SceneHandler.Instance.LoadMenuScene();
    }
}
