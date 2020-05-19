using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawnManager : NetworkBehaviour{

    [SerializeField]
    private GameObject trapPrefab;
    [SerializeField]
    private GameObject itemPrefab;

    private void Awake() {
        if (!isServer) {
            Debug.Log("[ItemSpawnManager] Registering item prefabs");
            ClientScene.RegisterPrefab(trapPrefab);
            ClientScene.RegisterPrefab(itemPrefab);
        }
    }

    public override void OnStartClient() {

        base.OnStartClient();

    }

    public void SpawnTrap(Vector3 position) {
        GameObject trapInstance = Instantiate(trapPrefab, position, Quaternion.identity);
        NetworkServer.Spawn(trapInstance);
    }

    public void SpawnItem(Vector3 position) {
        GameObject itemInstance = Instantiate(itemPrefab, position, Quaternion.identity);
        NetworkServer.Spawn(itemInstance);
    }
}
