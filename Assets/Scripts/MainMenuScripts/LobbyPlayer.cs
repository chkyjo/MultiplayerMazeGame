using Mirror;
using Mirror.Examples.Basic;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;

public class LobbyPlayer : NetworkBehaviour{

    [SerializeField]
    private TMP_Text playerName;

    //[SyncVar(hook = nameof(HandleDisplayNameChanged))]
    public string DisplayName = "Loading...";

    private bool isLeader;

    public bool IsLeader {
        set {
            isLeader = value;
        }
    }

    public override void OnStartClient() {
        LobbyManager.lobbyPlayers.Add(this);
        //Debug.Log("[LobbyPlayer] OnClientStart: " + )
        Transform playerList = GameObject.Find("PlayerListPanel").transform.GetChild(1);
        transform.SetParent(playerList, false);

        //GameObject.Find("PlayerListPanel").GetComponent<PlayerListController>().UpdateDisplay(this, LobbyManager.lobbyPlayers);
        for (int playerIndex = 0; playerIndex < playerList.childCount; playerIndex++) {
            Debug.Log("Updating name for player " + playerIndex);
            playerList.GetChild(playerIndex).GetComponent<LobbyPlayer>().UpdateName();
        }

    }

    public override void OnStartAuthority() {
        CmdDisplayName(PlayerInputController.DisplayName);
        GameObject.Find("Canvas").GetComponent<MainMenu>().SetEnabledStartButton(isLeader);

    }

    [Command]
    private void CmdDisplayName(string name) {
        playerName.text = name;
        DisplayName = name;
        Debug.Log("[LobbyPlayer] StartAuthority: " + playerName.text);
    }

    public void UpdateName() {
        CmdGetName();
    }

    [Command]
    private void CmdGetName() {
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
        CmdStartGame();
    }

    [Command]
    private void CmdStartGame() {
        lobbyManager.ServerChangeScene("MainScene");
    }

}
