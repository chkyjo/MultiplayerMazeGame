using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCollisionHandler : MonoBehaviour{

    public int wallIndex; //0 - forward, 1 - right ...

    public void OnTriggerEnter(Collider other) {
        Debug.Log("Collided with wall");
        if(wallIndex == 0) {
            GetComponentInParent<PlayerController>().forward = false;
        }
        else if(wallIndex == 1) {
            GetComponentInParent<PlayerController>().right = false;
        }
        else if (wallIndex == 2) {
            GetComponentInParent<PlayerController>().back = false;
        }
        else if (wallIndex == 3) {
            GetComponentInParent<PlayerController>().left = false;
        }
    }

    public void OnTriggerExit(Collider other) {
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
