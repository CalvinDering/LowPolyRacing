using UnityEngine;
using Unity.Netcode;
using Steamworks;
using Steamworks.Data;
using Netcode.Transports.Facepunch;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

public class SteamNetworkManager : MonoBehaviour {

    public static SteamNetworkManager Instance {
        get; private set;
    }

    private FacepunchTransport facepunchTransport = null;

    public Lobby? currentLobby {
        get; private set;
    }

    public List<Lobby> activeLobbies;

    public ulong hostId;

    private void Awake() {
        if(Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        facepunchTransport = GetComponent<FacepunchTransport>();
        activeLobbies = new List<Lobby>();

        SteamMatchmaking.OnLobbyCreated += SteamMatchmaking_OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += SteamMatchmaking_OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += SteamMatchmaking_OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += SteamMatchmaking_OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite += SteamMatchmaking_OnLobbyInvite;
        SteamMatchmaking.OnLobbyGameCreated += SteamMatchmaking_OnLobbyGameCreated;
        SteamFriends.OnGameLobbyJoinRequested += SteamFriends_OnGameLobbyJoinRequested;
    }

    private void OnDestroy() {
        SteamMatchmaking.OnLobbyCreated -= SteamMatchmaking_OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= SteamMatchmaking_OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined -= SteamMatchmaking_OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= SteamMatchmaking_OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite -= SteamMatchmaking_OnLobbyInvite;
        SteamMatchmaking.OnLobbyGameCreated -= SteamMatchmaking_OnLobbyGameCreated;
        SteamFriends.OnGameLobbyJoinRequested -= SteamFriends_OnGameLobbyJoinRequested;

        if(NetworkManager.Singleton == null) {
            return;
        }

        NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
    }

    public async void StartHost(int maxMembers) {
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.StartHost();

        GameMananger.Instance.clientId = NetworkManager.Singleton.LocalClientId;
        currentLobby = await SteamMatchmaking.CreateLobbyAsync(maxMembers);
        currentLobby?.SetData("lobbyOwner", SteamClient.Name);
    }

    public void StartClient(SteamId steamId) {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
        facepunchTransport.targetSteamId = steamId;
        GameMananger.Instance.clientId = NetworkManager.Singleton.LocalClientId;

        if(NetworkManager.Singleton.StartClient()) {
            Debug.Log("Client has started");
        }
    }

    public void Disconnect() {
        currentLobby?.Leave();
        if(NetworkManager.Singleton == null) {
            return;
        }

        if(NetworkManager.Singleton.IsHost) {
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        } else {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
        }
        NetworkManager.Singleton.Shutdown(true);
        GameMananger.Instance.Disconnect();
        Debug.Log("Disconnected");
    }

    public void CreateLobby() {
        StartHost(GameMananger.Instance.maxMemberLobbySize);
    }

    public async void GetLobbies() {
        await CollectOpenLobbies();
    }

    private async Task<bool> CollectOpenLobbies(int maxResults = 20) {
        try {
            activeLobbies.Clear();
            Debug.Log("Getting all lobbies");
            Lobby[] lobbies = await SteamMatchmaking.LobbyList
                .FilterDistanceClose()
                .WithMaxResults(maxResults)
                .WithSlotsAvailable(1)
                .RequestAsync();

            if(lobbies != null) {
                foreach(Lobby lobby in lobbies.ToList()) {
                    await RefreshAsync(lobby);
                    string owner = lobby.GetData("lobbyOwner");
                    Debug.Log($"Found lobby with id={lobby.Id} from {owner} with {lobby.MemberCount}/{lobby.MaxMembers}");
                    activeLobbies.Add(lobby);
                }
            }
            return true;

        } catch(System.Exception ex) {
            Debug.Log("Error fetching lobbies", this);
            Debug.LogException(ex, this);
            return false;
        }
        
    }

    public static async Task RefreshAsync(Lobby lobby) {
        TaskCompletionSource<bool> resultWaiter = new TaskCompletionSource<bool>();
        System.Action<Lobby> eventHandler = (Lobby queriedLobby) => {
            if(lobby.Id != queriedLobby.Id)
                return;
            resultWaiter.SetResult(true);
        };

        SteamMatchmaking.OnLobbyDataChanged += eventHandler;
        lobby.Refresh();
        var result = await resultWaiter.Task;
        Debug.Log("Refresh result: " + result);
        SteamMatchmaking.OnLobbyDataChanged -= eventHandler;
    }

    public void JoinFirstLobby() {
        if(activeLobbies.Count > 0) {
            activeLobbies[0].Join();
        }
    }

    public void JoinLobby(int lobbyId) {
        if(lobbyId >= 0 && lobbyId < activeLobbies.Count) {
            activeLobbies[lobbyId].Join();
        } else {
            Debug.Log("Could not join lobby with id " + lobbyId);
        }
    }

    private void OnApplicationQuit() {
        Disconnect();
    }

    #region Unity Network Callbacks

    private void OnServerStarted() {
        Debug.Log("Host started");
        GameMananger.Instance.HostCreated();
    }

    private void OnClientConnectedCallback(ulong clientId) {
        Debug.Log("Client conencted with id " + clientId);
        GameMananger.Instance.clientId = clientId;
    }

    private void OnClientDisconnectCallback(ulong clientId) {
        Debug.Log("Client disconencted with id " + clientId);

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
    }

    #endregion

    #region Steam Callbacks

    // Is called when player creates a lobby
    private void SteamMatchmaking_OnLobbyCreated(Result result, Lobby lobby) {
        if(result != Result.OK) {
            Debug.LogError($"Failed to create lobby, {result}", this);
            return;
        }
        lobby.SetPublic();
        lobby.SetJoinable(true);
        lobby.SetGameServer(lobby.Owner.Id);
        //lobby.SetData("lobbyOwner", lobby.Owner.Name);
        Debug.Log($"Lobby created by user {lobby.Owner.Name}");
    }

    // Is called when you enter a lobby
    private void SteamMatchmaking_OnLobbyEntered(Lobby lobby) {
        if(NetworkManager.Singleton.IsHost) {
            return;
        }

        Debug.Log($"{lobby.Owner.Name}'s Lobby entered");
        //currentLobby = lobby;

        StartClient(currentLobby.Value.Owner.Id);
    }

    // Is called when a member joins a lobby
    private void SteamMatchmaking_OnLobbyMemberJoined(Lobby lobby, Friend steamId) {
        Debug.Log($"Member {steamId.Name} joined the lobby");
    }

    // Is called when a member leaves a lobby
    private void SteamMatchmaking_OnLobbyMemberLeave(Lobby lobby, Friend steamId) {
        Debug.Log($"Member {steamId.Name} left the lobby");
    }

    // Is called when a friend sends you a steam invite
    private void SteamMatchmaking_OnLobbyInvite(Friend steamId, Lobby lobby) {
        Debug.Log($"Invite from {steamId.Name}");
    }

    // Is called when a game server is associated with the lobby
    private void SteamMatchmaking_OnLobbyGameCreated(Lobby lobby, uint ip, ushort port, SteamId steamId) {
        Debug.Log("Lobby was created");
    }

    // Is called when you accept the invite and join on a friend
    private async void SteamFriends_OnGameLobbyJoinRequested(Lobby lobby, SteamId steamId) {
        RoomEnter joinedLobby = await lobby.Join();
        if(joinedLobby != RoomEnter.Success) {
            Debug.Log("Failed to join lobby");
        } else {
            currentLobby = lobby;
            GameMananger.Instance.ConnectedAsClient();
            Debug.Log("Joined lobby");

            StartClient(steamId);
        }
    }

    #endregion
}
