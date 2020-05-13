using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LobbyNetworkManager : NetworkManager{
    [Scene]
    [SerializeField]
    private string menuScene = string.Empty;

    [Header("Room")]
    [SerializeField]
    private LobbyPlayer lobbyPlayerPrefab = null;
    [SerializeField]
    private PlayerController mazePlayerPrefab = null;
    [SerializeField]
    private Transform lobbyPlayerListPanel = null;

    [SerializeField]
    private PlayerListController playerListController;

    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;
    public static event Action<NetworkConnection> OnClientReady;

    public List<LobbyPlayer> lobbyPlayers { get; } = new List<LobbyPlayer>();

    public List<PlayerController> gamePlayers { get; } = new List<PlayerController>();

    public override void OnStartServer() => spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();

    [SerializeField]
    private GameObject spawnManagerPrefab;

    public override void OnStartClient() {
        var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");

        foreach(var prefab in spawnablePrefabs) {
            ClientScene.RegisterPrefab(prefab);
        }

        ClientScene.RegisterPrefab(lobbyPlayerPrefab.gameObject);
        ClientScene.RegisterPrefab(mazePlayerPrefab.gameObject);
    }

    public override void OnStopClient() {
        base.OnStopClient();

        if(SceneManager.GetActiveScene().buildIndex == 0) {
            playerListController.gameObject.SetActive(false);
        }

    }

    public override void OnClientConnect(NetworkConnection conn) {
        base.OnClientConnect(conn);

        OnClientConnected?.Invoke();
    }

    public override void OnClientDisconnect(NetworkConnection conn) {
        base.OnClientDisconnect(conn);

        lobbyPlayers.Clear();

        OnClientDisconnected?.Invoke();
    }
    
    //disconnect the client trying to connect if the maximum connections is exceeded or were not in the main menu
    public override void OnServerConnect(NetworkConnection conn) {
        if(numPlayers >= maxConnections) {
            conn.Disconnect();
            return;
        }

        if(SceneManager.GetActiveScene().buildIndex != 0) {
            conn.Disconnect();
            return;
        }
    }

    public override void OnServerAddPlayer(NetworkConnection conn) {
        if(SceneManager.GetActiveScene().buildIndex == 0) {
            bool isLeader = lobbyPlayers.Count == 0;

            //instantiate lobby player and add to lobby players list
            LobbyPlayer lobbyPlayerInstance = Instantiate(lobbyPlayerPrefab, lobbyPlayerListPanel);

            StartCoroutine(PositionPlayers(lobbyPlayerInstance));

            lobbyPlayerInstance.IsLeader = isLeader;

            NetworkServer.AddPlayerForConnection(conn, lobbyPlayerInstance.gameObject);

        }

    }

    IEnumerator PositionPlayers(LobbyPlayer instance) {
        instance.transform.SetParent(lobbyPlayerListPanel, false);
        yield return new WaitForEndOfFrame();
    }

    public override void OnServerDisconnect(NetworkConnection conn) {
        if(conn.identity != null) {
            var player = conn.identity.GetComponent<LobbyPlayer>();

            lobbyPlayers.Remove(player);
        }

        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer() {
        lobbyPlayers.Clear();
    }

    public override void ServerChangeScene(string newSceneName) {

        Debug.Log("Changing scene from: " + SceneManager.GetActiveScene().buildIndex + " to " + newSceneName);

        if (SceneManager.GetActiveScene().buildIndex == 0 && newSceneName == "MainScene") {
            for (int playerIndex = lobbyPlayers.Count - 1; playerIndex >= 0; playerIndex--) {
                Debug.Log("Spawning player: " + playerIndex);
                var conn = lobbyPlayers[playerIndex].connectionToClient;
                var playerInstance = Instantiate(mazePlayerPrefab);
                playerInstance.GetComponent<PlayerController>().DisplayName = lobbyPlayers[playerIndex].DisplayName;

                NetworkServer.Destroy(conn.identity.gameObject);
                //NetworkServer.ReplacePlayerForConnection(conn, playerInstance.gameObject, true);

            }
        }

        base.ServerChangeScene(newSceneName);

        Debug.Log("NetworkManager: Spawning spawn manager");
        GameObject spawnManagerInstance = Instantiate(spawnManagerPrefab);
        //spawn spawnManager for all clients giving authority to the server
        NetworkServer.Spawn(spawnManagerInstance);
    }

    public override void OnServerSceneChanged(string sceneName) {



    }

    public override void OnServerReady(NetworkConnection conn) {
        base.OnServerReady(conn);

        OnClientReady?.Invoke(conn);
    }

}
