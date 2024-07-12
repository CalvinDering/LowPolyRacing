using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TrackSelectionUIHandler : MonoBehaviour {

    [SerializeField] private TrackSO[] trackList;
    [SerializeField] private TextMeshProUGUI selectedTrackName;
    [SerializeField] private Image selectedTrackImage;
    [SerializeField] private TextMeshProUGUI selectedTrackLapsText;
    [SerializeField] private TextMeshProUGUI aiRacerCountText;
    [SerializeField] private Transform[] playerSlots;
    [SerializeField] private string joinMessage;

    private int selectedTrackId = 0;
    private int selectedTrackLaps = 3;
    private int aiRacerCount = 0;

    public static TrackSelectionUIHandler Instance;

    private void Awake() {
        if(Instance != this && Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        //DontDestroyOnLoad(gameObject);

        SetupPlayerSlots();
        UpdateTrackDisplay();
    }

    private void SetupPlayerSlots() {
        PlayerInput[] players = PlayerManager.Instance.GetPlayers();
        for(int i = 0; i < playerSlots.Length; i++) {
            PlayerSlot playerSlot = playerSlots[i].GetComponent<PlayerSlot>();
            if(players[i] != null) {
                int increasedPlayerId = i + 1;
                playerSlot.SetPlayerName("Player " + increasedPlayerId, true);
            } else {
                playerSlot.SetPlayerName(joinMessage);
                playerSlot.SetCarDisplay(false);
            }
        }
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

    public void AddPlayerDisplay(int playerId) {
        PlayerSlot playerSlot = playerSlots[playerId].GetComponent<PlayerSlot>();
        int increasedPlayerId = playerId + 1;
        playerSlot.SetPlayerName("Player " + increasedPlayerId, true);
    }

    public void RemovePlayerDisplay(int playerId) {
        PlayerSlot playerSlot = playerSlots[playerId].GetComponent<PlayerSlot>();
        playerSlot.SetPlayerName(joinMessage);
    }

    public void StartGame() {
        PlayerManager.Instance.selectedTrackLaps = selectedTrackLaps;
        PlayerManager.Instance.aiRacerCount = aiRacerCount;
        SceneHandler.Instance.LoadTrackScene(selectedTrackId);
    }

    public void GoToMainMenu() {
        SceneHandler.Instance.LoadMenuScene();
    }
}
