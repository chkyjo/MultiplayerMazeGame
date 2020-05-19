using Mirror;
using UnityEngine;

public class BodyTriggerHandler : NetworkBehaviour{

    bool active = false;

    private void Start() {
        if (GetComponentInParent<PlayerController>().hasAuthority) {
            active = true;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (active) {
            Debug.Log("[BodyTriggerHandler] Collided with body: " + other.name);

            if (other.name == "Item(Clone)") {
                GetComponentInParent<PlayerController>().PickUpItem(other.gameObject);
            }
        }
    }




}
