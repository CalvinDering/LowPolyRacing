using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMananger : MonoBehaviour {

    public static GameMananger Instance;

    public bool connected;
    public bool inGame;
    public bool isHost;
    public ulong clientId;

    public int maxMemberLobbySize = 4;

    private void Awake() {
        if(Instance != null && Instance != this) {
            Destroy(this);
        } else {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void HostCreated() {
        isHost = true;
        connected = true;
    }

    public void ConnectedAsClient() {
        isHost = false;
        connected = true;
    }

    public void Disconnect() {
        isHost = false;
        connected = false;
    }

    public void Quit() {
        Application.Quit();
    }
}
