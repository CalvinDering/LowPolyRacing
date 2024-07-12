using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour {

    public static SceneHandler Instance;

    [SerializeField] private int MenuSceneIndex;
    [SerializeField] private int TrackSectionSceneIndex;
    [SerializeField] private int GameSceneIndex;

    private void Awake() {
        if(Instance != this && Instance != null) {
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);
    }

    private void Start() {
        MusicManager.Instance.PlayMenuMusic();
    }

    public void LoadMenuScene() {
        SceneManager.LoadScene(MenuSceneIndex);
        MusicManager.Instance.PlayMenuMusic();
    }

    public void LoadTrackSelectionScene() {
        SceneManager.LoadScene(TrackSectionSceneIndex);
    }

    public void LoadGameScene() {
        LoadTrackScene(TrackSelectionUIHandler.Instance.selectedTrackId);
    }

    public void LoadTrackScene(int trackId) {
        int trackSceneIndex = GameSceneIndex + trackId;
        if(trackSceneIndex >= SceneManager.sceneCountInBuildSettings) {
            trackSceneIndex = GameSceneIndex;
        } 
        SceneManager.LoadScene(trackSceneIndex);
        MusicManager.Instance.PlayGameMusic();
    }
}
