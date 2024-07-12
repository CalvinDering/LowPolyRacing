using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class PlayerManager : MonoBehaviour {

    public static PlayerManager Instance;

    private List<PlayerInput> players = new List<PlayerInput>();
    [SerializeField] private List<Transform> spawnpoints;
    [SerializeField] private List<LayerMask> playerLayers;

    private PlayerInputManager playerInputManager;

    private void Awake() {
        if(Instance != null && Instance != null) {
            Destroy(this);
            return;
        }
        Instance = this;

        playerInputManager = GetComponent<PlayerInputManager>();
    }

    private void OnEnable() {
        playerInputManager.onPlayerJoined += AddPlayer;
    }

    private void OnDisable() {
        playerInputManager.onPlayerJoined -= AddPlayer;
    }

    public void AddPlayer(PlayerInput player) {
        players.Add(player);

        player.transform.GetComponent<Rigidbody>().position = spawnpoints[players.Count - 1].position;

        int layerToAdd = (int) Mathf.Log(playerLayers[players.Count - 1].value, 2);

        player.GetComponentInChildren<CinemachineVirtualCamera>().gameObject.layer = layerToAdd;
        player.GetComponentInChildren<Camera>().cullingMask |= 1 << layerToAdd;

        int playerCount = players.Count;

        for(int i = 0; i <playerCount; i++) {

            float viewportX = (0.25f * Mathf.Pow(i, 2) - 0.25f * i) % 1;
            float viewportY = 0.5f * Mathf.Pow(i, 2) % 2;
            float viewportWidth = playerCount < 3 ? 1 : 0.5f;
            float viewportHeight = playerCount == 1 ? 1 : 0.5f;
            Rect viewportRect = new Rect(viewportX, viewportY, viewportWidth, viewportHeight);

            players[i].GetComponentInChildren<Camera>().rect = viewportRect;
        }

        RaceController.Instance.AddRacer(player.GetComponent<CarController>());
    }

    public List<CarController> GetCarControllerFromAllPlayers() {
        List<CarController> controllers = players.Select(p => p.GetComponent<CarController>()).ToList();
        return controllers;
    }

}
