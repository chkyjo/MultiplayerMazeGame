using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Mirror.Websocket;

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

    [SerializeField]
    private GameObject spawnManagerPrefab;
    [SerializeField]
    private GameObject mazeGeneratorPrefab;
    private GameObject mazeGenerator;

    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;
    public static event Action<NetworkConnection> OnServerReadied;

    public List<LobbyPlayer> lobbyPlayers { get; } = new List<LobbyPlayer>();

    public List<PlayerController> gamePlayers { get; } = new List<PlayerController>();

    public override void OnStartHost() {
        Debug.Log("[LobbyNetworkManager] Host started");
        base.OnStartHost();
    }

    public override void OnStartServer() {
        Debug.Log("[LobbyNetworkManager] Server started");
        spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();
        MazeGenerator.mazeGenerated += PositionPlayers;
    }

    //disconnect the client trying to connect if the maximum connections is exceeded or were not in the main menu
    public override void OnServerConnect(NetworkConnection conn) {
        if(numPlayers >= maxConnections) {
            Debug.Log("[LobbyNetworkManager] Disconnected new connection: exceeded max connections");
            conn.Disconnect();
            return;
        }

        if(SceneManager.GetActiveScene().buildIndex != 0) {
            Debug.Log("[LobbyNetworkManager] Disconnected new connection: no new connections accepted");
            conn.Disconnect();
            return;
        }
    }

    public override void OnServerReady(NetworkConnection conn) {
        Debug.Log("[LobbyNetworkManager] Client ready on server");
        base.OnServerReady(conn);
        OnServerReadied?.Invoke(conn);
    }

    public override void OnStartClient() {

        Debug.Log("[LobbyNetworkManager] Client started");

        var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");

        foreach (var prefab in spawnablePrefabs) {
            ClientScene.RegisterPrefab(prefab);
        }

        ClientScene.RegisterPrefab(lobbyPlayerPrefab.gameObject);
        //ClientScene.RegisterPrefab(mazePlayerPrefab.gameObject);
        ClientScene.RegisterPrefab(mazeGeneratorPrefab.gameObject);

    }

    public override void OnClientConnect(NetworkConnection conn) {
        Debug.Log("[LobbyNetworkManager] Connected to server");
        base.OnClientConnect(conn);

        OnClientConnected?.Invoke();
        if(SceneManager.GetActiveScene().buildIndex == 0) {
            ClientScene.AddPlayer(conn);
        }

    }

    public override void OnStopClient() {
        base.OnStopClient();

        if(SceneManager.GetActiveScene().buildIndex == 0) {
            playerListController.gameObject.SetActive(false);
        }

    }

    public override void OnClientDisconnect(NetworkConnection conn) {
        base.OnClientDisconnect(conn);

        lobbyPlayers.Clear();

        OnClientDisconnected?.Invoke();

        if (SceneManager.GetActiveScene().buildIndex == 1) {
            SceneManager.LoadScene(0);
        }
    }

    

    public override void OnServerAddPlayer(NetworkConnection conn) {
        Debug.Log("[LobbyNetworkManager] Adding player object");
        if(SceneManager.GetActiveScene().buildIndex == 0) {
            bool isLeader = lobbyPlayers.Count == 0;

            //instantiate lobby player and add to lobby players list
            LobbyPlayer lobbyPlayerInstance = Instantiate(lobbyPlayerPrefab, lobbyPlayerListPanel);

            if (isLeader) {
                Debug.Log("[LobbyNetworkManager] Setting isLeader to true");
            }
            lobbyPlayerInstance.IsLeader = isLeader;

            NetworkServer.AddPlayerForConnection(conn, lobbyPlayerInstance.gameObject);

            StartCoroutine(PositionPlayers(lobbyPlayerInstance));
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

    //[Server]
    public override void ServerChangeScene(string newSceneName) {
        
        Debug.Log("[LobbyNetworkManager] Changing scene from: " + SceneManager.GetActiveScene().buildIndex + " to " + newSceneName);

        if (SceneManager.GetActiveScene().buildIndex == 0 && newSceneName == "MainScene") {
            for (int playerIndex = lobbyPlayers.Count - 1; playerIndex >= 0; playerIndex--) {
                Debug.Log("[LobbyNetworkManager] Spawning player: " + playerIndex);
                var conn = lobbyPlayers[playerIndex].connectionToClient;
                var playerInstance = Instantiate(mazePlayerPrefab);
                playerInstance.GetComponent<PlayerController>().DisplayName = lobbyPlayers[playerIndex].DisplayName;


                GameObject oldPlayer = conn.identity.gameObject;
                NetworkServer.ReplacePlayerForConnection(conn, playerInstance.gameObject);
                NetworkServer.Destroy(oldPlayer);
            }
        }

        base.ServerChangeScene(newSceneName);

    }

    public override void OnServerSceneChanged(string sceneName) {


        Debug.Log("[LobbyNetworkManager] Spawning spawn manager");
        //GameObject spawnManagerInstance = Instantiate(spawnManagerPrefab);
        //spawn spawnManager for all clients giving authority to the server
        //NetworkServer.Spawn(spawnManagerInstance);

        Debug.Log("[LobbyNetworkManager] Server scene was changed");
        mazeGenerator = Instantiate(mazeGeneratorPrefab);
        NetworkServer.Spawn(mazeGenerator);

    }

    private void PositionPlayers() {
        Debug.Log("[LobbyNetworkManager] Positioning players");
        Transform playersParent = GameObject.Find("Players").transform;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        for(int playerIndex = 0; playerIndex < players.Length; playerIndex++) {
            players[playerIndex].transform.position = GetSpawnPoint();
            players[playerIndex].transform.position += Vector3.up;
            players[playerIndex].transform.SetParent(playersParent);
            players[playerIndex].GetComponent<PlayerController>().ResetMobility();

        }
    }

    public Vector3 GetSpawnPoint() {
        return mazeGenerator.GetComponent<MazeGenerator>().GetUnusedSpawnPoint();
    }

}
