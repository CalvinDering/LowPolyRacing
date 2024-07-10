using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour {

    public static SceneHandler Instance;

    [SerializeField] private int MenuSceneIndex;
    [SerializeField] private int GameSceneIndex;

    private void Awake() {
        if(Instance != this) {
            Destroy(this);
        }

        Instance = this;
    }

    public void LoadMenuScene() {
        SceneManager.LoadScene(MenuSceneIndex);
    }

    public void LoadGameScene() {
        SceneManager.LoadScene(GameSceneIndex);
    }
}
