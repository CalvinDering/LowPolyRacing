using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplicationStart : MonoBehaviour {

    private void Start() {
        SceneManager.LoadScene("MenuScene");
    }

}
