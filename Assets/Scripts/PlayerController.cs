using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : NetworkBehaviour{

    public bool forward = true;
    public bool right = true;
    public bool back = true;
    public bool left = true;

    private string displayName = null;

    private void Awake() {
        //DontDestroyOnLoad(gameObject);
    }

    [Client]
    // Update is called once per frame
    void Update(){

        if (!hasAuthority) {
            return;
        }

        CmdMoveFromServer();
        if (Input.GetKeyDown(KeyCode.W)) {
            if (forward) {
                transform.Translate(6 * Vector3.forward);
            }

        }
        else if (Input.GetKeyDown(KeyCode.S)) {
            if (back) {
                transform.Translate(6 * Vector3.back);
            }

        }
        else if (Input.GetKeyDown(KeyCode.D)) {
            if (right) {
                transform.Translate(6 * Vector3.right);
            }

        }
        else if (Input.GetKeyDown(KeyCode.A)) {
            if (left) {
                transform.Translate(6 * Vector3.left);
            }
        }

    }
    public string DisplayName {
        set {
            displayName = value;
        }
        get {
            return displayName;
        }
    }

    public override void OnStartClient() {

        LobbyManager.gamePlayers.Add(this);
        //Debug.Log("Player added: " + LobbyManager.gamePlayers[LobbyManager.gamePlayers.Count - 1].DisplayName);
    }

    public override void OnStopClient() {
        LobbyManager.gamePlayers.Remove(this);
    }

    public override void OnStartAuthority() {
        transform.position = Vector3.zero;
    }

    [Command]
    private void CmdMoveFromServer() {
        RpcMovePlayer();
    }

    [ClientRpc]
    private void RpcMovePlayer() {

    }

    private LobbyNetworkManager lobbyManager;
    private LobbyNetworkManager LobbyManager {
        get {
            if (lobbyManager != null) { return lobbyManager; }
            return lobbyManager = NetworkManager.singleton as LobbyNetworkManager;
        }
    }
}
