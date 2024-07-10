using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuUIHandler : MonoBehaviour {

    public void StartGame() {
        SceneHandler.Instance.LoadGameScene();
    }
    
    public void ShowSettings() {

    }
    
    public void ExitGame() {
        Application.Quit();
    }
}
