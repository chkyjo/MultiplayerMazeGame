using Mirror;
using Mirror.Examples.Basic;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyPlayer : NetworkBehaviour{

    [SerializeField]
    private TMP_Text playerName;

    [SyncVar()]
    public string DisplayName = "Loading...";

    [SyncVar(hook = nameof(UpdateLeaderStatus))]
    private bool isLeader;
    [SerializeField]
    private GameObject leaderSymbol;

    public bool IsLeader {
        set {
            isLeader = value;
        }
    }

    //called on all clients for each new client
    public override void OnStartClient() {
        if(SceneManager.GetActiveScene().buildIndex == 0) {
            Debug.Log("[LobbyPlayer] Started client: " + DisplayName);
            LobbyManager.lobbyPlayers.Add(this);

            Transform playerList = GameObject.Find("PlayerListPanel").transform.GetChild(1);
            transform.SetParent(playerList, false);

            playerName.text = DisplayName;
        }
    }

    public override void OnStartAuthority() {
        if(SceneManager.GetActiveScene().buildIndex == 0) {
            Debug.Log("[LobbyPlayer] StartAuthority: " + PlayerInputController.DisplayName);
            DisplayName = PlayerInputController.DisplayName;
            CmdDisplayName(PlayerInputController.DisplayName);

            CmdGetName();

        }
    }

    private void UpdateLeaderStatus(bool oldValue, bool newValue) {
        isLeader = newValue;

        if (isLeader) {
            Debug.Log("[LobbyPlayer] Setting leader mode");
            leaderSymbol.SetActive(true);
        }
        else {
            Debug.Log("[LobbyPlayer] Client is not the leader");
            leaderSymbol.SetActive(false);
        }

        //if (hasAuthority) {
            GameObject.Find("Canvas").GetComponent<MainMenu>().SetEnabledStartButton(isLeader);
        //}

    }

    [Command]
    private void CmdDisplayName(string name) {
        Debug.Log("Setting name for client: " + name);
        playerName.text = name;
        DisplayName = name;
    }

    [Command]
    private void CmdGetName() {
        Debug.Log("Giving client name: " + DisplayName);
        RpcSetName(DisplayName);
    }

    [ClientRpc]
    private void RpcSetName(string name) {
        playerName.text = name;
    }

    //public void HandleDisplayNameChanged(string oldValue, string newValue) => UpdateDisplay();

    private LobbyNetworkManager lobbyManager;
    private LobbyNetworkManager LobbyManager {
        get {
            if (lobbyManager != null) { return lobbyManager; }
            return lobbyManager = NetworkManager.singleton as LobbyNetworkManager;
        }
    }

    public void StartGame() {
        if (hasAuthority) {
            CmdStartGame();
        }

    }

    [Command]
    public void CmdStartGame() {
        
        Debug.Log("[LobbyPlayer] Calling ServerChangeScene on server");
        LobbyManager.ServerChangeScene("MainScene");
        
    }
}
