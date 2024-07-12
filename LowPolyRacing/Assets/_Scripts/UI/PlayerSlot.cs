using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerSlot : MonoBehaviour {

    private string playerName;
    private string carName;

    [SerializeField] private TextMeshProUGUI carNameText;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private GameObject carNameButtons;

    public void SetPlayerName(string playerName, bool hasPlayerJoined = false) {
        this.playerName = playerName;
        SetCarDisplay(hasPlayerJoined);
        UpdateTextDisplay();
    }

    public void SetCarName(string carName) {
        this.carName = carName;
        SetCarDisplay(true);
    }

    public void SetCarDisplay(bool displayCar) {
        carNameText.enabled = displayCar;
        carNameButtons.SetActive(displayCar);
    }

    public void UpdateTextDisplay() {
        carNameText.text = carName;
        playerNameText.text = playerName;
    }
}
