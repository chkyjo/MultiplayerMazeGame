using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinLobbyMenu : MonoBehaviour
{
    [SerializeField] private LobbyNetworkManager networkManager = null;

    [Header("UI")]
    [SerializeField]
    private GameObject playerListPanel = null;
    [SerializeField]
    private TMP_InputField ipAddressField = null;
    [SerializeField]
    private Button joinButton = null;

    public void OnEnable() {
        LobbyNetworkManager.OnClientConnected += HandleClientConnected;
        LobbyNetworkManager.OnClientDisconnected += HandleClientDisconnected;
    }

    private void OnDisable() {
        LobbyNetworkManager.OnClientConnected -= HandleClientConnected;
        LobbyNetworkManager.OnClientDisconnected -= HandleClientDisconnected;
    }

    public void JoinLobby() {
        string ipAddress = ipAddressField.text;

        networkManager.networkAddress = ipAddress;
        networkManager.StartClient();

        joinButton.interactable = false;
    }

    private void HandleClientConnected() {
        joinButton.interactable = true;

        gameObject.SetActive(false);
        playerListPanel.SetActive(true);
    }

    private void HandleClientDisconnected() {
        joinButton.interactable = true;
    }
}
