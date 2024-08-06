using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// Contains data on scene loading progress for the local instance and remote instances.
public class LoadingProgressManager : NetworkBehaviour {

    [SerializeField] GameObject progressTrackerPrefab;

    // Dictionary containing references to the NetworkedLoadingProgessTrackers that contain the loading progress of
    // each client. Keys are ClientIds.
    public Dictionary<ulong, NetworkedLoadingProgressTracker> progressTrackers { get; } = new Dictionary<ulong, NetworkedLoadingProgressTracker>();

    // This is the AsyncOperation of the current load operation. This property should be set each time a new
    // loading operation begins.
    public AsyncOperation LocalLoadOperation {
        set {
            localProgress = 0;
            localLoadOperation = value;
        }
    }

    private AsyncOperation localLoadOperation;

    private float localProgressValue;

    // This event is invoked each time the dictionary of progress trackers is updated (if one is removed or added, for example.)
    public event Action onTrackersUpdated;

    // The current loading progress for the local client. Handled by a local field if not in a networked session,
    // or by a progress tracker from the dictionary.
    public float localProgress {
        get => IsSpawned && progressTrackers.ContainsKey(NetworkManager.LocalClientId) ?
            progressTrackers[NetworkManager.LocalClientId].progress.Value : localProgressValue;
        private set {
            if(IsSpawned && progressTrackers.ContainsKey(NetworkManager.LocalClientId)) {
                progressTrackers[NetworkManager.LocalClientId].progress.Value = value;
            } else {
                localProgressValue = value;
            }
        }
    }

    public override void OnNetworkSpawn() {
        if(IsServer) {
            NetworkManager.OnClientConnectedCallback += AddTracker;
            NetworkManager.OnClientDisconnectCallback += RemoveTracker;
            AddTracker(NetworkManager.LocalClientId);
        }
    }

    public override void OnNetworkDespawn() {
        if(IsServer) {
            NetworkManager.OnClientConnectedCallback -= AddTracker;
            NetworkManager.OnClientDisconnectCallback -= RemoveTracker;
        }
        progressTrackers.Clear();
        onTrackersUpdated?.Invoke();
    }

    private void Update() {
        if(localLoadOperation != null) {
            localProgress = localLoadOperation.isDone ? 1 : localLoadOperation.progress;
        }
    }

    [ClientRpc]
    private void UpdateTrackers_ClientRpc() {
        if(!IsHost) {
            progressTrackers.Clear();
            foreach(var tracker in FindObjectsOfType<NetworkedLoadingProgressTracker>()) {
                // If a tracker is despawned but not destroyed yet, don't add it
                if(tracker.IsSpawned) {
                    progressTrackers[tracker.OwnerClientId] = tracker;
                    if(tracker.OwnerClientId == NetworkManager.LocalClientId) {
                        localProgress = Mathf.Max(localProgressValue, localProgress);
                    }
                }
            }
        }
        onTrackersUpdated?.Invoke();
    }

    private void AddTracker(ulong clientId) {
        if(IsServer) {
            var tracker = Instantiate(progressTrackerPrefab);
            var networkObject = tracker.GetComponent<NetworkObject>();
            networkObject.SpawnWithOwnership(clientId);
            progressTrackers[clientId] = tracker.GetComponent<NetworkedLoadingProgressTracker>();
            UpdateTrackers_ClientRpc();
        }
    }

    private void RemoveTracker(ulong clientId) {
        if(IsServer) {
            if(progressTrackers.ContainsKey(clientId)) {
                var tracker = progressTrackers[clientId];
                progressTrackers.Remove(clientId);
                tracker.NetworkObject.Despawn();
                UpdateTrackers_ClientRpc();
            }
        }
    }

    public void ResetLocalProgress() {
        localProgress = 0;
    }
}