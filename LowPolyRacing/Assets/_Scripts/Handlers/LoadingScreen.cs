using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour {

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI sceneNameTextfield;
    [SerializeField] private Slider progressBar;
    [SerializeField] private float delayBeforeFadeOut = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.1f;
    [SerializeField] private LoadingProgressManager loadingProgressManager;

    [SerializeField] List<Slider> otherPlayersProgressBars;
    [SerializeField] List<TextMeshProUGUI> otherPlayerNamesTexts;

    private Dictionary<ulong, LoadingProgressBar> loadingProgressBars = new Dictionary<ulong, LoadingProgressBar>();

    private Coroutine fadeOutCoroutine;
    private bool loadingScreenRunning;

    public class LoadingProgressBar {
        public Slider progressBar {
            get; set;
        }

        public TextMeshProUGUI nameText {
            get; set;
        }

        public LoadingProgressBar(Slider otherPlayerProgressBar, TextMeshProUGUI otherPlayerNameText) {
            progressBar = otherPlayerProgressBar;
            nameText = otherPlayerNameText;
        }

        public void UpdateProgress(float value, float newValue) {
            progressBar.value = newValue;
        }
    }

    private void Awake() {
        DontDestroyOnLoad(this);
        loadingProgressManager.onTrackersUpdated += OnProgressTrackersUpdated;
    }

    private void Start() {
        SetCanvasVisibility(false);
    }

    private void Update() {
        if(loadingScreenRunning) {
            progressBar.value = loadingProgressManager.localProgress;

        }
    }

    private void OnDestroy() {
        loadingProgressManager.onTrackersUpdated -= OnProgressTrackersUpdated;
    }

    public void StartLoadingScreen(string sceneName) {
        SetCanvasVisibility(true);
        loadingScreenRunning = true;
        UpdateLoadingScreen(sceneName);
        ReinitializeProgressBars();
    }

    public void UpdateLoadingScreen(string sceneName) {
        if(loadingScreenRunning) {
            sceneNameTextfield.text = sceneName;
            if(fadeOutCoroutine != null) {
                StopCoroutine(fadeOutCoroutine);
            }
        }
    }

    public void StopLoadingScreen() {
        if(loadingScreenRunning) {
            if(fadeOutCoroutine != null) {
                StopCoroutine(fadeOutCoroutine);
            }
            fadeOutCoroutine = StartCoroutine(FadeOutCoroutine());
        }
    }

    private void OnProgressTrackersUpdated() {
        // Deactivate progress bars of clients that are no longer tracked
        List<ulong> clientIdsToRemove = new List<ulong>();
        foreach(ulong clientId in loadingProgressBars.Keys) {
            if(!loadingProgressManager.progressTrackers.ContainsKey(clientId)) {
                clientIdsToRemove.Add(clientId);
            }
        }

        foreach(ulong clientId in clientIdsToRemove) {
            RemoveOtherPlayerProgressBar(clientId);
        }

        // Add progress bars for clients that are now tracked
        foreach(KeyValuePair<ulong, NetworkedLoadingProgressTracker> progressTracker in loadingProgressManager.progressTrackers) {
            ulong clientId = progressTracker.Key;
            if(clientId != GameMananger.Instance.clientId && !loadingProgressBars.ContainsKey(clientId)) {
                AddOtherPlayerProgressBar(clientId, progressTracker.Value);
            }
        }
    }

    private void AddOtherPlayerProgressBar(ulong clientId, NetworkedLoadingProgressTracker progressTracker) {
        if(loadingProgressBars.Count < otherPlayersProgressBars.Count && loadingProgressBars.Count < otherPlayerNamesTexts.Count) {
            int index = loadingProgressBars.Count;
            loadingProgressBars[clientId] = new LoadingProgressBar(otherPlayersProgressBars[index], otherPlayerNamesTexts[index]);
            progressTracker.progress.OnValueChanged += loadingProgressBars[clientId].UpdateProgress;
            loadingProgressBars[clientId].progressBar.value = progressTracker.progress.Value;
            loadingProgressBars[clientId].progressBar.gameObject.SetActive(true);
            loadingProgressBars[clientId].nameText.gameObject.SetActive(true);
            loadingProgressBars[clientId].nameText.text = $"Client {clientId}";
        } else {
            throw new Exception("There are not enough progress bars to track the progress of all the players.");
        }
    }

    private void RemoveOtherPlayerProgressBar(ulong clientId, NetworkedLoadingProgressTracker progressTracker = null) {
        if(progressTracker != null) {
            progressTracker.progress.OnValueChanged -= loadingProgressBars[clientId].UpdateProgress;
        }

        loadingProgressBars[clientId].progressBar.gameObject.SetActive(false);
        loadingProgressBars[clientId].nameText.gameObject.SetActive(false);
        loadingProgressBars.Remove(clientId);
    }

    private void UpdateOtherPlayerProgressBar(ulong clientId, int progressBarIndex) {
        loadingProgressBars[clientId].progressBar = otherPlayersProgressBars[progressBarIndex];
        loadingProgressBars[clientId].progressBar.gameObject.SetActive(true);
        loadingProgressBars[clientId].nameText = otherPlayerNamesTexts[progressBarIndex];
        loadingProgressBars[clientId].nameText.gameObject.SetActive(true);
    }

    private void ReinitializeProgressBars() {
        // Deactivate progress bars of clients that are no longer tracked
        List<ulong> clientIdsToRemove = new List<ulong>();
        foreach(ulong clientId in loadingProgressBars.Keys) {
            if(!loadingProgressManager.progressTrackers.ContainsKey(clientId)) {
                clientIdsToRemove.Add(clientId);
            }
        }

        foreach(ulong clientId in clientIdsToRemove) {
            RemoveOtherPlayerProgressBar(clientId);
        }

        for(int i = 0; i < otherPlayersProgressBars.Count; i++) {
            otherPlayersProgressBars[i].gameObject.SetActive(false);
            otherPlayerNamesTexts[i].gameObject.SetActive(false);
        }

        int index = 0;

        foreach(KeyValuePair<ulong, NetworkedLoadingProgressTracker> progressTracker in loadingProgressManager.progressTrackers) {
            ulong clientId = progressTracker.Key;
            if(clientId != GameMananger.Instance.clientId) {
                UpdateOtherPlayerProgressBar(clientId, index++);
            }
        }
    }

    private void SetCanvasVisibility(bool visible) {
        canvasGroup.alpha = visible ? 1 : 0;
        canvasGroup.blocksRaycasts = visible;
    }

    private IEnumerator FadeOutCoroutine() {
        yield return new WaitForSeconds(delayBeforeFadeOut);
        loadingScreenRunning = false;

        float currentTIme = 0;
        while(currentTIme < fadeOutDuration) {
            canvasGroup.alpha = Mathf.Lerp(1, 0, currentTIme / fadeOutDuration);
            yield return null;
            currentTIme += Time.deltaTime;
        }

        SetCanvasVisibility(false);
    }
}
