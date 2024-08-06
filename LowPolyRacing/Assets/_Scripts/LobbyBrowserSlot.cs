using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyBrowserSlot : MonoBehaviour {

    public TextMeshProUGUI lobbyNameText;
    public TextMeshProUGUI lobbyOwnerText;
    public TextMeshProUGUI memberCountText;
    public Button joinButton;

    public void SetValues(string lobbyName, string lobbyOwner, int memberCount, int maxMemberCount, int index) {
        lobbyNameText.text = lobbyName;
        lobbyOwnerText.text = lobbyOwner;
        memberCountText.text = memberCount + " / " + maxMemberCount;
        joinButton.onClick.AddListener(() => TrackSelectionUIHandler.Instance.SetLobbyIndex(index));
    }
}
