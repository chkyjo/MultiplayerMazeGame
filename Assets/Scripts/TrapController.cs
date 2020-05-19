using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapController : NetworkBehaviour{

    int activationTime = 3;
    bool active = false;

    private void Start() {
        StartCoroutine(SetActiveTimer());
    }

    IEnumerator SetActiveTimer() {
        for(int timeIndex = 0; timeIndex < activationTime; timeIndex++) {
            yield return new WaitForSeconds(1);
        }
        active = true;
    }

    public void OnTriggerEnter(Collider other) {
        if (active) {
            if (isServer) {
                other.transform.parent.GetComponent<PlayerController>().Die();
            }
        }

    }
}
