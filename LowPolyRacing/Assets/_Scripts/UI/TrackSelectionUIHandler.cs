using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TrackSelectionUIHandler : MonoBehaviour {

    [SerializeField] private GameObject lobbyButtons;
    [SerializeField] private GameObject menuButtons;
    [SerializeField] private GameObject trackDisplay;
    [SerializeField] private GameObject lobbyBrowser;
    [SerializeField] private GameObject lobbyBrowserSlotPrefab;
    [SerializeField] private GameObject playerDisplay;
    [SerializeField] private TrackSO[] trackList;
    [SerializeField] private TextMeshProUGUI selectedTrackName;
    [SerializeField] private Image selectedTrackImage;
    [SerializeField] private TextMeshProUGUI selectedTrackLapsText;
    [SerializeField] private TextMeshProUGUI aiRacerCountText;
    [SerializeField] private Transform[] playerSlots;
    [SerializeField] private string joinMessage;

    private List<LobbyBrowserSlot> lobbies;
    private int lobbyIndex;
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

        lobbies = new();
        RefreshLobbies();
        SetupPlayerSlots();
        UpdateTrackDisplay();

        playerDisplay.SetActive(false);
        trackDisplay.SetActive(false);
        lobbyBrowser.SetActive(false);
        lobbyButtons.SetActive(true);
        menuButtons.SetActive(false);
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

    public void CreateLobby() {
        SteamNetworkManager.Instance.CreateLobby();

        playerDisplay.SetActive(true);
        trackDisplay.SetActive(true);
        lobbyBrowser.SetActive(false);
        lobbyButtons.SetActive(false);
        menuButtons.SetActive(true);
    }

    public void JoinLobby() {
        if(lobbyIndex < 0 || lobbyIndex > SteamNetworkManager.Instance.activeLobbies.Count) {
            Debug.Log("Can not join lobby. Invalid lobby index");
            return;
        } else {
            SteamNetworkManager.Instance.JoinLobby(lobbyIndex);

            playerDisplay.SetActive(true);
            trackDisplay.SetActive(true);
            lobbyBrowser.SetActive(false);
            lobbyButtons.SetActive(false);
            menuButtons.SetActive(true);
        }
    }

    public void ExitLobby() {
        SteamNetworkManager.Instance.Disconnect();

        HideLobbyBrowser();
    }

    public void HideLobbyBrowser() {
        playerDisplay.SetActive(false);
        trackDisplay.SetActive(false);
        lobbyBrowser.SetActive(false);
        lobbyButtons.SetActive(true);
        menuButtons.SetActive(false);
    }

    public void SetLobbyIndex(int index) {
        lobbyIndex = index;
    }

    public void RefreshLobbies() {
        SteamNetworkManager.Instance.GetLobbies();
        foreach(LobbyBrowserSlot lobbyBrowserSlot in lobbies) {
            Destroy(lobbyBrowserSlot.gameObject);
        }
        lobbies = new();

        for(int i = 0; i < SteamNetworkManager.Instance.activeLobbies.Count; i++) {
            Steamworks.Data.Lobby lobby = SteamNetworkManager.Instance.activeLobbies[i];
            string lobbyName = lobby.GetData("lobbyName");
            string lobbyOwner = lobby.GetData("lobbyOwner");

            GameObject lobbyBrowserSlotObject = Instantiate(lobbyBrowserSlotPrefab, lobbyBrowser.transform);
            LobbyBrowserSlot lobbyBrowserSlot = lobbyBrowserSlotObject.GetComponent<LobbyBrowserSlot>();
            lobbyBrowserSlot.SetValues(lobbyName, lobbyOwner, lobby.MemberCount, lobby.MaxMembers, i);
            lobbies.Add(lobbyBrowserSlot);
        }

        playerDisplay.SetActive(false);
        trackDisplay.SetActive(false);
        lobbyBrowser.SetActive(true);
        lobbyButtons.SetActive(true);
        menuButtons.SetActive(false);
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
