using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : NetworkBehaviour {

    [SerializeField]
    private GameObject playerPrefab;

    List<int> spawnPointsUsed;

    public override void OnStartServer() {
        Debug.Log("[SpawnManager] SpawnManager: active on server ");
        LobbyNetworkManager.OnClientReady += SpawnPlayer;
    }

    [ServerCallback]
    private void OnDestroy() => LobbyNetworkManager.OnClientReady -= SpawnPlayer;

    private void SpawnPlayer(NetworkConnection conn) {
        if(spawnPointsUsed == null) {
            spawnPointsUsed = new List<int>();
        }
        Debug.Log("[SpawnManager] Spawning player");
        int maxIndex = MazeGenerator.Instance.rows * MazeGenerator.Instance.columns;

        int spawnLocation = Random.Range(0, maxIndex);

        bool newSpawn = true;

        do {
            newSpawn = true;
            for (int spawnIndex = 0; spawnIndex < spawnPointsUsed.Count; spawnIndex++) {
                if (spawnLocation == spawnPointsUsed[spawnIndex]) {
                    newSpawn = false;
                }
            }
        } while (newSpawn);

        spawnPointsUsed.Add(spawnLocation);

        Vector3 position = MazeGenerator.Instance.GetCellLocation(spawnLocation);

        GameObject playerInstance = Instantiate(playerPrefab, position, Quaternion.identity);
        NetworkServer.Spawn(playerInstance, conn);

    }
}
