using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour{
    [SerializeField]
    private LobbyNetworkManager networkManager = null;

    [Header("UI")]
    [SerializeField]
    private GameObject landingPagePanel = null;

    [SerializeField]
    private GameObject startGameButton = null;
    [SerializeField]
    private GameObject cancelHostButton = null;

    public void HostLobby() {
        networkManager.StartHost();
        landingPagePanel.SetActive(false);
    }

    public void SetEnabledStartButton(bool value) {
        Debug.Log("[MainMenu] Setting leader buttons: " + value);
        startGameButton.SetActive(value);
        cancelHostButton.SetActive(value);
    }

    public void LeaderMode() {
        startGameButton.SetActive(true);
    }
}
