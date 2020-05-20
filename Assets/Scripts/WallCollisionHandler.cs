using Boo.Lang;
using Mirror;
using UnityEngine;

public class WallCollisionHandler : NetworkBehaviour{

    public int wallIndex; //0 - forward, 1 - right ...
    public PlayerController playerController;

    List<GameObject> frontWalls;
    List<GameObject> rightWalls;
    List<GameObject> backWalls;
    List<GameObject> leftWalls;

    private void Start() {
        
        playerController = GetComponentInParent<PlayerController>();
        if (playerController.isServer) {
            frontWalls = new List<GameObject>();
            rightWalls = new List<GameObject>();
            backWalls = new List<GameObject>();
            leftWalls = new List<GameObject>();
        }
    }

    public void OnTriggerEnter(Collider other) {

        //if our player
        if (playerController.hasAuthority) {
            //if detection has found a player
            if (other.name == "Capsule") {
                if (wallIndex == 0) {
                    //make them visible if there is no wall
                    if (GetComponentInParent<PlayerController>().forward) {
                        other.GetComponent<MeshRenderer>().enabled = true;
                    }
                }
                else if (wallIndex == 1) {
                    //make them visible if there is no wall
                    if (GetComponentInParent<PlayerController>().right) {
                        other.GetComponent<MeshRenderer>().enabled = true;
                    }
                }
                else if (wallIndex == 2) {
                    //make them visible if there is no wall
                    if (GetComponentInParent<PlayerController>().back) {
                        other.GetComponent<MeshRenderer>().enabled = true;
                    }
                }
                else if (wallIndex == 3) {
                    //make them visible if there is no wall
                    if (GetComponentInParent<PlayerController>().left) {
                        other.GetComponent<MeshRenderer>().enabled = true;
                    }
                }

                return;
            }
        }

        if (playerController.isServer) {
            if (wallIndex == 0) {
                playerController.forward = false;
            }
            else if (wallIndex == 1) {
                playerController.right = false;
            }
            else if (wallIndex == 2) {
                playerController.back = false;
            }
            else if (wallIndex == 3) {
                playerController.left = false;
            }
        }
    }

    public void OnTriggerExit(Collider other) {

        //if our player
        if (playerController.hasAuthority) {
            if (other.name == "Capsule") {
                other.GetComponent<MeshRenderer>().enabled = false;
                return;
            }
        }



        if (playerController.isServer) {

            if (wallIndex == 0) {
                GetComponentInParent<PlayerController>().forward = true;
            }
            else if (wallIndex == 1) {
                GetComponentInParent<PlayerController>().right = true;
            }
            else if (wallIndex == 2) {
                GetComponentInParent<PlayerController>().back = true;
            }
            else if (wallIndex == 3) {
                GetComponentInParent<PlayerController>().left = true;
            }
        }
    }

}
