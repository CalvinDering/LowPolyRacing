using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseUIHandler : MonoBehaviour {

    [SerializeField] private GameObject pauseMenu;

    public static PauseUIHandler Instance;

    private bool isPaused = false;

    private void Awake() {
        if(Instance != null && Instance != null) {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Start() {
        isPaused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    public void TogglePause() {
        isPaused = !isPaused;
        pauseMenu.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
        RaceController.Instance.PauseCarSounds(isPaused);
    }

    public bool IsPaused() {
        return isPaused;
    }

    public void GoToMainMenu() {
        Time.timeScale = 1f;
        SceneHandler.Instance.LoadMenuScene();
    }

}
