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
    [SerializeField] private GameObject emptyCameraPrefab;

    private float intervalSecondTimer;
    [SerializeField] private float cameraSwitchInSecondsInterval = 10f;

    private GameObject forthSplitscreenCamera;
    private CinemachineVirtualCamera forthVirtualCamera;
    private int forthCameraPlayerIndex = 0;

    public int selectedTrackLaps = 3;
    public int aiRacerCount = 0;
    private int connectedPlayerCount = 0;

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

        intervalSecondTimer = Time.time;
    }

    private void Update() {
        if(connectedPlayerCount == 3) {
            if(Time.time >= intervalSecondTimer) {
                intervalSecondTimer += cameraSwitchInSecondsInterval;
                forthCameraPlayerIndex++;
                if(forthCameraPlayerIndex >= 3) {
                    forthCameraPlayerIndex = 0;
                }
                SetForthCameraLayerAtPlayer(forthCameraPlayerIndex);
            }
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

    private void CalcConnectecPlayerCount() {
        connectedPlayerCount = players.Where(p => p != null).Count();
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

        CalcConnectecPlayerCount();

        CheckSplitScreenSetup();
    }

    public void SetupAICars() {
        for(int i = 0; i < aiRacerCount; i++) {
            GameObject aiCar = Instantiate(aiCarPrefab);
            aiCars.Add(aiCar);

            AddAICar(aiCar.GetComponent<CarController>(), i);
        }
    }

    private void CheckSplitScreenSetup() {
        if(connectedPlayerCount == 3) {
            SetupEmptyCamera();
        }
    }

    public void SetupEmptyCamera() {
        forthSplitscreenCamera = Instantiate(emptyCameraPrefab);
        forthVirtualCamera = forthSplitscreenCamera.GetComponentInChildren<CinemachineVirtualCamera>();
        int layerToAdd = (int) Mathf.Log(playerLayers[3].value, 2);
        forthSplitscreenCamera.GetComponentInChildren<CinemachineVirtualCamera>().gameObject.layer = layerToAdd;
        SetForthCameraLayerAtPlayer(0);
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

            Rect viewport = GetCameraViewport(i, playerCount);
            players[i].GetComponentInChildren<Camera>().rect = viewport;
        }

        RaceController.Instance.AddRacer(player.GetComponent<CarController>());
    }

    private Rect GetCameraViewport(int index, int playerCount) {

        float viewportX = (0.25f * Mathf.Pow(index, 2) - 0.25f * index) % 1;
        float viewportY = 0.5f * Mathf.Pow(index, 2) % 2;
        float viewportWidth = playerCount < 3 ? 1 : 0.5f;
        float viewportHeight = playerCount == 1 ? 1 : 0.5f;
        return new Rect(viewportX, viewportY, viewportWidth, viewportHeight);
    }

    private void SetForthCameraLayerAtPlayer(int playerIndex) {

        GameObject playerObject = players[playerIndex].gameObject;
        forthVirtualCamera.Follow = playerObject.transform;
        forthVirtualCamera.LookAt = players[playerIndex].transform.Find("CarBody");
        Camera forthCamera = forthSplitscreenCamera.GetComponentInChildren<Camera>();

        for(int i = 0; i < 4; i++) {
            int layerToRemove = (int) Mathf.Log(playerLayers[i].value, 2);
            forthCamera.cullingMask &= ~(1 << layerToRemove);
        }

        int layerToAdd = (int) Mathf.Log(playerLayers[playerIndex].value, 2);
        forthCamera.cullingMask |= 1 << layerToAdd;
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
