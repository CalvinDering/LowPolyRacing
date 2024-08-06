using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : NetworkBehaviour {

    public static SceneHandler Instance;

    private bool isInitialized;
    private bool isNetworkSceneManagementEnabled => NetworkManager != null && NetworkManager.SceneManager != null && NetworkManager.NetworkConfig.EnableSceneManagement;

    [SerializeField] private LoadingScreen clientLoadingScreen;
    [SerializeField] private int MenuSceneIndex;
    [SerializeField] private int TrackSectionSceneIndex;
    [SerializeField] private int GameSceneIndex;

    private void Awake() {
        if(Instance != this && Instance != null) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }

        DontDestroyOnLoad(this);
    }

    private void Start() {
        SceneManager.sceneLoaded += OnSceneLoaded;
        NetworkManager.OnServerStarted += OnNetworkingSessionStarted;
        NetworkManager.OnClientStarted += OnNetworkingSessionStarted;
        NetworkManager.OnServerStopped += OnNetworkingSessionEnded;
        NetworkManager.OnClientStopped += OnNetworkingSessionEnded;
    }

    private void OnNetworkingSessionStarted() {
        if(!isInitialized) {
            if(isNetworkSceneManagementEnabled) {
                NetworkManager.SceneManager.OnSceneEvent += OnSceneEvent;
            }

            isInitialized = true;
        }
    }

    private void OnNetworkingSessionEnded(bool hostMode) {
        if(isInitialized) {
            if(isNetworkSceneManagementEnabled) {
                NetworkManager.SceneManager.OnSceneEvent -= OnSceneEvent;
            }

            isInitialized = false;
        }
    }

    public override void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if(NetworkManager != null) {
            NetworkManager.OnServerStarted -= OnNetworkingSessionStarted;
            NetworkManager.OnClientStarted -= OnNetworkingSessionStarted;
            NetworkManager.OnServerStopped -= OnNetworkingSessionEnded;
            NetworkManager.OnClientStopped -= OnNetworkingSessionEnded;
        }
        base.OnDestroy();
    }

    private void OnSceneEvent(SceneEvent sceneEvent) {
        switch(sceneEvent.SceneEventType) {
            case SceneEventType.Load:
                // Server told client to load a scene

                if(NetworkManager.IsClient) {
                    // Only start a new loading screen if scene loaded in single mode, else simply update
                    if(sceneEvent.LoadSceneMode == LoadSceneMode.Single) {
                        clientLoadingScreen.StartLoadingScreen(sceneEvent.SceneName);
                    } else {
                        clientLoadingScreen.UpdateLoadingScreen(sceneEvent.SceneName);
                    }
                }

                break;
            case SceneEventType.LoadEventCompleted:
                // Server told client that all clients finished loading a scene

                if(NetworkManager.IsClient) {
                    clientLoadingScreen.StopLoadingScreen();
                }
                break;
            case SceneEventType.Synchronize:
                // Server told client to start synchonizing scenes

                if(NetworkManager.IsClient && !NetworkManager.IsHost) {
                    // unload all currently loaded additive scenes so that if we connect to a server with the same
                    // main scene we properly load and synchronize all appropriate scenes without loading a scene
                    // that is already loaded.
                    UnloadAdditiveScenes();
                }
                break;
            case SceneEventType.SynchronizeComplete:
                // Client told server taht they finished synchronizing

                if(NetworkManager.IsServer) {
                    // Send client RPC to make sure the client stops the loading screen after the server handles
                    // what it needs to after the client finished synchronizing, for example character spawning
                    // done server side should still be hidden by loading screen.
                    StopLoadingScreen_ClientRPC(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { sceneEvent.ClientId } } });
                }
                break;
        }
    }

    private void UnloadAdditiveScenes() {
        Scene activeScene = SceneManager.GetActiveScene();
        for(int i = 0; i < SceneManager.sceneCount; i++) {
            Scene scene = SceneManager.GetSceneAt(i);
            if(scene.isLoaded && scene != activeScene) {
                SceneManager.UnloadSceneAsync(scene);
            }
        }
    }

    [ClientRpc]
    private void StopLoadingScreen_ClientRPC(ClientRpcParams clientRpcParams = default) {
        clientLoadingScreen.StopLoadingScreen();
    }

    // Loads a scene asynchronosly using the specified loadSceneMode with NetworkSceneManager if on a listening
    // server with SceneManagement enabled, or SceneManager otherwise. If a scene is loaded via SceneManager, this
    // method also triggers the start of the loading screen.
    public void LoadScene(string sceneName, bool useNetworkSceneManager, LoadSceneMode loadSceneMode) {
        if(useNetworkSceneManager) {
            if(IsSpawned && isNetworkSceneManagementEnabled && !NetworkManager.ShutdownInProgress) {
                if(NetworkManager.IsServer) {
                    // If is active server and NetworkManager uses scene management, load scene using NetworkManager's SceneManager
                    NetworkManager.SceneManager.LoadScene(sceneName, loadSceneMode);
                }
            }
        } else {
            // Load using SceneManager
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
            if(loadSceneMode == LoadSceneMode.Single) {
                clientLoadingScreen.StartLoadingScreen(sceneName);
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if(!IsSpawned || NetworkManager.ShutdownInProgress) {
            clientLoadingScreen.StopLoadingScreen();
        }

        //if(scene.buildIndex == TrackSectionSceneIndex) {
        //    PlayerManager.Instance.SetAllPlayerComponents(false);
        //}
    }



    public void LoadMenuScene() {
        Debug.LogWarning("Loading using the old loading system!");
        SceneManager.LoadScene(MenuSceneIndex);
        MusicManager.Instance.PlayMenuMusic();
    }

    public void LoadTrackSelectionScene() {
        Debug.LogWarning("Loading using the old loading system!");
        SceneManager.LoadScene(TrackSectionSceneIndex);
    }

    public void LoadTrackScene(int trackId) {
        Debug.LogWarning("Loading using the old loading system!");
        int trackSceneIndex = GameSceneIndex + trackId;
        if(trackSceneIndex >= SceneManager.sceneCountInBuildSettings) {
            trackSceneIndex = GameSceneIndex;
        } 
        SceneManager.LoadScene(trackSceneIndex);
        MusicManager.Instance.PlayGameMusic();
    }
}
