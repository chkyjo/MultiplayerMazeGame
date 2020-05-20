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

    bool currentPlayer = false;

    public bool forward = true;
    public bool right = true;
    public bool back = true;
    public bool left = true;

    private bool moving = false;
    private bool rotating = false;
    float rotationSpeed = 2.5f;

    private string displayName = null;

    GameObject itemToPickUp;
    List<GameObject> items;

    bool detected = false;
    bool visible = false;

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
        if (isServer) {
            StartCoroutine(WallDetection());
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

        if (hasAuthority) {
            StartCoroutine(ListenForInput());
        }

    }

    IEnumerator ListenForInput() {
        while (true) {
            camera.transform.position = transform.position + transform.up * 7 + transform.forward * -3;
            camera.transform.LookAt(transform);

            if(!moving && !rotating) {
                if (Input.GetKey(KeyCode.W)) {
                    CmdMoveFromServer(Direction.forward);
                }
                else if (Input.GetKey(KeyCode.D)) {
                    CmdRotate(1);
                }
                else if (Input.GetKey(KeyCode.S)) {
                    CmdMoveFromServer(Direction.backward);
                }
                else if (Input.GetKey(KeyCode.A)) {
                    CmdRotate(-1);
                }

                if (Input.GetKeyDown(KeyCode.F)) {
                    CmdPlaceTrap();
                }
            }


            yield return null;
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

    public void ResetMobility() {
        forward = true;
        right = true;
        back = true;
        left = true;
    }

    public override void OnStartAuthority() {
        Debug.Log("[PlayerController] Gained client authority over player controller object");
        StartCoroutine(EnemyDetection());
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

    IEnumerator EnemyDetection() {
        RaycastHit hit;
        Ray forwardRay;
        Ray rightRay;
        Ray backRay;
        Ray leftRay;
        while (true) {

            forwardRay = new Ray(transform.position + transform.forward, transform.forward);
            if (Physics.Raycast(forwardRay, out hit, 6)) {
                if (hit.collider.name == "Capsule") {
                    Debug.DrawRay(transform.position + transform.forward, transform.forward * hit.distance, Color.red);
                    hit.collider.GetComponentInParent<PlayerController>().Discover();
                }
            }
            else {
                Debug.DrawRay(transform.position + transform.forward, transform.forward, Color.white);
            }

            rightRay = new Ray(transform.position + transform.right, transform.right);
            if (Physics.Raycast(rightRay, out hit, 6)) {
                if (hit.collider.name == "Capsule") {
                    Debug.DrawRay(transform.position + transform.right, transform.right * hit.distance, Color.red);
                    hit.collider.GetComponentInParent<PlayerController>().Discover();
                }
            }
            else {
                Debug.DrawRay(transform.position + transform.right, transform.right, Color.white);
            }

            backRay = new Ray(transform.position + -transform.forward, -transform.forward);
            if (Physics.Raycast(backRay, out hit, 6)) {
                if (hit.collider.name == "Capsule") {
                    Debug.DrawRay(transform.position + -transform.forward, -transform.forward * hit.distance, Color.red);
                    hit.collider.GetComponentInParent<PlayerController>().Discover();
                }
            }
            else {
                Debug.DrawRay(transform.position + -transform.forward, -transform.forward, Color.white);
            }

            leftRay = new Ray(transform.position + -transform.right, -transform.right);
            if (Physics.Raycast(leftRay, out hit, 6)) {
                if (hit.collider.name == "Capsule") {
                    Debug.DrawRay(transform.position + -transform.right, -transform.right * hit.distance, Color.red);
                    hit.collider.GetComponentInParent<PlayerController>().Discover();
                }
            }
            else {
                Debug.DrawRay(transform.position + -transform.right, -transform.right, Color.white);
            }

            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator WallDetection() {
        RaycastHit hit;
        Ray forwardRay;
        Ray rightRay;
        Ray backRay;
        Ray leftRay;
        while (true) {

            forward = true;
            right = true;
            back = true;
            left = true;

            forwardRay = new Ray(transform.position + transform.forward, transform.forward);
            if (Physics.Raycast(forwardRay, out hit, 6)) {
                Debug.DrawRay(transform.position + transform.forward, transform.forward * hit.distance, Color.yellow);
                Debug.Log("[PlayerController] Server: " + DisplayName + " forward ray hit " + hit.collider.name);
                if (hit.collider.name != "Capsule") {
                    forward = false;
                }
            }
            else {
                Debug.DrawRay(transform.position + transform.forward, transform.forward, Color.white);
            }

            rightRay = new Ray(transform.position + transform.right, transform.right);
            if (Physics.Raycast(rightRay, out hit, 6)) {
                Debug.DrawRay(transform.position + transform.right, transform.right * hit.distance, Color.yellow);
                Debug.Log("[PlayerController] Server: " + DisplayName + " right ray hit " + hit.collider.name);
                if (hit.collider.name != "Capsule") {
                    right = false;
                }
            }
            else {
                Debug.DrawRay(transform.position + transform.right, transform.right, Color.white);
            }

            backRay = new Ray(transform.position + -transform.forward, -transform.forward);
            if (Physics.Raycast(backRay, out hit, 6)) {
                Debug.DrawRay(transform.position + -transform.forward, -transform.forward * hit.distance, Color.yellow);
                Debug.Log("[PlayerController] Server: " + DisplayName + " back ray hit " + hit.collider.name);
                if (hit.collider.name != "Capsule") {
                    back = false;
                }
            }
            else {
                Debug.DrawRay(transform.position + -transform.forward, -transform.forward, Color.white);
            }

            leftRay = new Ray(transform.position + -transform.right, -transform.right);
            if (Physics.Raycast(leftRay, out hit, 6)) {
                Debug.DrawRay(transform.position + -transform.right, -transform.right * hit.distance, Color.yellow);
                Debug.Log("[PlayerController] Server: " + DisplayName + " left ray hit " + hit.collider.name);
                if (hit.collider.name != "Capsule") {
                    left = false;
                }
            }
            else {
                Debug.DrawRay(transform.position + -transform.right, -transform.right, Color.white);
            }

            yield return new WaitForFixedUpdate();
        }
    }

    public void Discover() {

        detected = true;
        if (!visible) {
            StartCoroutine(DiscoverTime());
        }

    }

    IEnumerator DiscoverTime() {
        transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
        visible = true;
        while (detected) {
            detected = false;
            yield return new WaitForSeconds(1);
        }
        transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
        visible = false;
    }

    [Command]
    private void CmdMoveFromServer(Direction direction) {
        if (currentPlayer) {
            if (gameManager.GetMovement()) {
                if(direction == Direction.forward && forward) {
                    RpcMovePlayer(direction);
                }
                if(direction == Direction.backward && back) {
                    RpcMovePlayer(direction);
                }
            }
        }
    }

    [ClientRpc]
    private void RpcMovePlayer(Direction direction) {
        if(!moving && !rotating) {
            moving = true;
            StartCoroutine(MovePlayer(direction));
        }
    }

    IEnumerator MovePlayer(Direction direction) {
        float timeElapsed = 0;
        float endTime = 1;

        Vector3 targetPos;

        if (direction == Direction.forward) {
            targetPos = transform.position + (6 * transform.forward);

            while (timeElapsed < endTime) {
                transform.position += (6 * transform.forward * Time.deltaTime);
                yield return new WaitForFixedUpdate();
                timeElapsed += Time.deltaTime;
            }
        }
        else {
            targetPos = transform.position + (6 * -transform.forward);

            while (timeElapsed < endTime) {
                transform.position += (6 * -transform.forward * Time.deltaTime);
                yield return new WaitForFixedUpdate();
                timeElapsed += Time.deltaTime;
            }
        }

        transform.position = targetPos;
        moving = false;
    }

    [Command]
    private void CmdRotate(int direction) {
        if (direction == 1 || direction == -1) {
            RpcRotate(direction);
        }

    }

    [ClientRpc]
    private void RpcRotate(int direction) {
        if (!moving && !rotating) {
            Debug.Log("[PlayerController] Rotating player");
            StartCoroutine(RotatePlayer(direction));
        }
    }

    IEnumerator RotatePlayer(int direction) {
        float timeElapsed = 0;
        rotating = true;
        Quaternion oldRotation = transform.rotation;
        Quaternion targetAngle = Quaternion.Euler(transform.eulerAngles + new Vector3(0, 90, 0) * direction);
        Debug.Log("[PlayerController] Rotating from: " + oldRotation.eulerAngles + " to " + targetAngle.eulerAngles);

        while (timeElapsed < 1) {
            timeElapsed += Time.deltaTime * rotationSpeed;
            transform.rotation = Quaternion.Slerp(oldRotation, targetAngle, timeElapsed);
            yield return new WaitForFixedUpdate();
        }
        rotating = false;
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

    public void SetCurrentPlayer(bool value) {
        currentPlayer = value;
    }
    public bool GetCurrentPlayer() {
        return currentPlayer;
    }
}
