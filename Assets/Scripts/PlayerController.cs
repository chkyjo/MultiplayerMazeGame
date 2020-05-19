using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityTemplateProjects;

public class PlayerController : NetworkBehaviour{

    GameManager gameManager;
    ItemSpawnManager itemSpawnManager;
    GameObject camera;

    public bool forward = true;
    public bool right = true;
    public bool back = true;
    public bool left = true;
    private bool moving = false;

    private string displayName = null;

    GameObject itemToPickUp;
    List<GameObject> items;

    private void Awake() {
        GameManager.gameManagerActive += GameManagerActive;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        items = new List<GameObject>();
        if (!hasAuthority) {
            transform.GetChild(0).GetComponent<BodyTriggerHandler>().enabled = false;
            transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
        }
    }

    [Client]
    // Update is called once per frame
    void Update(){

        if (!hasAuthority) {
            return;
        }

        if(camera != null) {
            camera.transform.position = new Vector3(transform.position.x, 8, transform.position.z - 2);
        }

        if (Input.GetKeyDown(KeyCode.W)) {
            CmdMoveFromServer(Direction.forward);
        }
        else if (Input.GetKeyDown(KeyCode.D)) {
            CmdMoveFromServer(Direction.right);
        }
        else if (Input.GetKeyDown(KeyCode.S)) {
            CmdMoveFromServer(Direction.backward);
        }
        else if (Input.GetKeyDown(KeyCode.A)) {
            CmdMoveFromServer(Direction.left);
        }

        if (Input.GetKeyDown(KeyCode.F)) {
            CmdPlaceTrap();
        }

    }

    private void GameManagerActive() {
        Debug.Log("Active scene: " + SceneManager.GetActiveScene().buildIndex);
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        itemSpawnManager = GameObject.Find("GameManager").GetComponent<ItemSpawnManager>();
        camera = GameObject.Find("Main Camera");
        camera.GetComponent<SimpleCameraController>().enabled = false;
        camera.transform.position = new Vector3(transform.position.x, 8, transform.position.z - 2);
        camera.transform.LookAt(transform);
    }

    public string DisplayName {
        set {
            displayName = value;
        }
        get {
            return displayName;
        }
    }

    public void ResetMobility() {
        forward = true;
        right = true;
        back = true;
        left = true;
    }

    public override void OnStartAuthority() {
        Debug.Log("[PlayerController] Gained client authority over player controller object");
    }

    public override void OnStartClient() {

        LobbyManager.gamePlayers.Add(this);
        //NetworkServer.SetClientReady(connectionToClient);
        Debug.Log("[PlayerController] Player added: " + LobbyManager.gamePlayers[LobbyManager.gamePlayers.Count - 1].DisplayName);

        //transform.position = GameObject.Find("MazeGenerator(Clone)").GetComponent<MazeGenerator>().GetUnusedSpawnPoint();
    }

    public override void OnStopClient() {
        LobbyManager.gamePlayers.Remove(this);
    }

    [Command]
    private void CmdMoveFromServer(Direction direction) {
        if(gameManager == null) {
            return;
        }

        if (gameManager.GetMovement()) {
            if (direction == Direction.forward) {
                if (forward) {
                    RpcMovePlayer(Vector3.forward);
                }
            }
            else if (direction == Direction.right) {
                if (right) {
                    RpcMovePlayer(Vector3.right);
                }
            }
            else if (direction == Direction.backward) {
                if (back) {
                    RpcMovePlayer(Vector3.back);
                }
            }
            else if (direction == Direction.left) {
                if (left) {
                    RpcMovePlayer(Vector3.left);
                }
            }
        }
    }

    [ClientRpc]
    private void RpcMovePlayer(Vector3 direction) {
        if(moving == false) {
            moving = true;
            StartCoroutine(MovePlayer(direction));
        }
    }

    IEnumerator MovePlayer(Vector3 direction) {
        float timeElapsed = 0;
        float endTime = 1;

        Vector3 targetPos = transform.position + (6 * direction);

        while (timeElapsed < endTime) {
            transform.Translate((6 * direction) * Time.deltaTime);
            yield return new WaitForFixedUpdate();
            timeElapsed += Time.deltaTime;
        }

        transform.position = targetPos;
        moving = false;
    }

    public void PickUpItem(GameObject item) {
        CmdPickUpItem(item, gameObject);
    }

    [Command]
    public void CmdPickUpItem(GameObject item, GameObject target) {
        Debug.Log("[PlayerController] Server: Picking up item " + item.name);
        items.Add(item);
        item.SetActive(false);
        TargetPickUpItem(target.GetComponent<NetworkIdentity>().connectionToClient);
    }

    [TargetRpc]
    public void TargetPickUpItem(NetworkConnection conn) {
        Debug.Log("[PlayerController] Item picked up");

    }

    [Command]
    private void CmdPlaceTrap() {
        if (itemSpawnManager == null) {
            return;
        }

        itemSpawnManager.SpawnTrap(transform.position);
    }

    private LobbyNetworkManager lobbyManager;
    private LobbyNetworkManager LobbyManager {
        get {
            if (lobbyManager != null) { return lobbyManager; }
            return lobbyManager = NetworkManager.singleton as LobbyNetworkManager;
        }
    }

    public enum Direction {
        forward,
        right,
        backward,
        left
    }

    public void Die() {
        transform.position = LobbyManager.GetSpawnPoint();
    }

    private void CmdKillPlayer() {
        transform.position = LobbyManager.GetSpawnPoint();
    }
}
