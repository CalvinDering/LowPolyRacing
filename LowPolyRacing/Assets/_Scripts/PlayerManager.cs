using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.InputSystem.Utilities;

public class PlayerManager : MonoBehaviour {

    public static PlayerManager Instance;

    private PlayerInput[] players = new PlayerInput[4];
    private List<GameObject> aiCars = new List<GameObject>();
    private List<Transform> spawnpoints;
    [SerializeField] private List<LayerMask> playerLayers;
    [SerializeField] private GameObject aiCarPrefab;

    public int selectedTrackLaps = 3;
    public int aiRacerCount = 0;

    private PlayerInputManager playerInputManager;

    private void Awake() {
        if(Instance != this && Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        for(int i = 0; i < players.Length; i++) {
            players[i] = null;
        }
    }

    private void OnEnable() {
        playerInputManager = GetComponent<PlayerInputManager>();
        playerInputManager.onPlayerJoined += AddPlayer;
        playerInputManager.onPlayerLeft += RemovePlayer;
    }

    private void OnDisable() {
        playerInputManager = GetComponent<PlayerInputManager>();
        playerInputManager.onPlayerJoined -= AddPlayer;
        playerInputManager.onPlayerLeft -= RemovePlayer;
    }

    public void SetAllPlayerComponents(bool isActive) {
        foreach(PlayerInput player in players) {
            if(player != null) {
                SetPlayerComponents(player.GetComponent<Racer>(), isActive, false);
            }
        }
    }

    public void SetPlayerComponents(Racer racer, bool isActive, bool isAI) {
        racer.GetComponent<CarController>().enabled = isActive;
        racer.GetComponent<Rigidbody>().isKinematic = !isActive;

        AudioSource[] audioSources = racer.GetComponents<AudioSource>();
        foreach(AudioSource audio in audioSources) {
            audio.enabled = isActive;
        }

        if(!isAI) {
            racer.GetComponent<CarInputHandler>().enabled = isActive;
            racer.GetComponentInChildren<AudioListener>().enabled = isActive;
            racer.transform.Find("Canvas").gameObject.SetActive(isActive);
        }
    }

    public void AddPlayer(PlayerInput player) {

        SetPlayerComponents(player.GetComponent<Racer>(), false, false);

        int playerIndex = -1;
        for(int i = 0; i < players.Length; i++) {
            if(players[i] == null) {
                playerIndex = i;
                break;
            }
        }

        if(playerIndex == -1) {
            Debug.LogError("Could not add player!");
            return;
        }

        players[playerIndex] = player;
        TrackSelectionUIHandler.Instance.AddPlayerDisplay(playerIndex);
    }

    public PlayerInput[] GetPlayers() {
        return players;
    }

    public void RemovePlayer(PlayerInput player) {
        int playerIndex = players.First(p => p == player).playerIndex;
        Destroy(players[playerIndex].gameObject);
        TrackSelectionUIHandler.Instance.RemovePlayerDisplay(playerIndex);
        players[playerIndex] = null;
    }

    private int GetPlayerCount() {
        int playerCount = players.Count(p => p != null);
        return playerCount;
    }

    public void SetupPlayerCars() {
        for(int i = 0; i < players.Count(); i++) {
            if(players[i] != null) {
                AddPlayerCar(players[i], i);
            }
        }
    }

    public void SetupAICars() {
        for(int i = 0; i < aiRacerCount; i++) {
            GameObject aiCar = Instantiate(aiCarPrefab);
            aiCars.Add(aiCar);

            AddAICar(aiCar.GetComponent<CarController>(), i);
        }
    }

    public void SetupSpawnpoints(List<Transform> spawnpoints) {
        this.spawnpoints = spawnpoints;
    }

    public void AddPlayerCar(PlayerInput player, int playerIndex) {
        int activePlayers = 0;
        for(int i = 0; i < players.Length; i++) {
            if(players[i] != null) {
                activePlayers++;
            }
        }

        player.transform.GetComponent<Rigidbody>().position = spawnpoints[playerIndex + aiRacerCount].position;

        int layerToAdd = (int) Mathf.Log(playerLayers[playerIndex].value, 2);

        player.GetComponentInChildren<CinemachineVirtualCamera>().gameObject.layer = layerToAdd;
        player.GetComponentInChildren<Camera>().cullingMask |= 1 << layerToAdd;

        int playerCount = GetPlayerCount();

        for(int i = 0; i < playerCount; i++) {

            float viewportX = (0.25f * Mathf.Pow(i, 2) - 0.25f * i) % 1;
            float viewportY = 0.5f * Mathf.Pow(i, 2) % 2;
            float viewportWidth = playerCount < 3 ? 1 : 0.5f;
            float viewportHeight = playerCount == 1 ? 1 : 0.5f;
            Rect viewportRect = new Rect(viewportX, viewportY, viewportWidth, viewportHeight);

            players[i].GetComponentInChildren<Camera>().rect = viewportRect;
        }

        RaceController.Instance.AddRacer(player.GetComponent<CarController>());
    }

    public void AddAICar(CarController controller, int spawnIndex) {
        controller.GetComponent<Rigidbody>().position = spawnpoints[spawnIndex].position;

        RaceController.Instance.AddRacer(controller, true);
    }

    public List<CarController> GetCarControllerFromAllPlayers() {
        List<CarController> controllers = players.Where(r => r != null).Select(p => p.GetComponent<CarController>()).ToList();
        return controllers;
    }

}
