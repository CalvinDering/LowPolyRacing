using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuUIHandler : MonoBehaviour {

    public void ShowTrackSelection() {
        SceneHandler.Instance.LoadTrackSelectionScene();
    }
    
    public void ShowSettings() {

    }
    
    public void ExitGame() {
        Application.Quit();
    }
}
